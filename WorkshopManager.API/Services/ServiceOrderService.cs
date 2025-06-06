using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;
using WorkshopManager.API.Services.Interfaces;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;

namespace WorkshopManager.API.Services;

public class ServiceOrderService : IServiceOrderService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ServiceOrderService> _logger;

    public ServiceOrderService(ApplicationDbContext context, ILogger<ServiceOrderService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ServiceOrder>> GetAllServiceOrdersAsync()
    {
        return await _context.ServiceOrders
            .Include(o => o.Vehicle)
            .Include(o => o.Tasks)
            .Include(o => o.Comments)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceOrder>> GetActiveServiceOrdersAsync()
    {
        return await _context.ServiceOrders
            .Include(o => o.Vehicle)
            .Include(o => o.Tasks)
            .Include(o => o.Comments)
            .Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
            .ToListAsync();
    }

    public async Task<ServiceOrder> GetServiceOrderByIdAsync(int id)
    {
        var order = await _context.ServiceOrders
            .Include(o => o.Vehicle)
            .Include(o => o.Tasks)
            .Include(o => o.Comments)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
        {
            _logger.LogWarning("Service order with ID {Id} not found", id);
            throw new KeyNotFoundException($"Service order with ID {id} not found");
        }

        return order;
    }

    public async Task<ServiceOrder> CreateServiceOrderAsync(ServiceOrder order)
    {
        order.CreatedAt = DateTime.UtcNow;
        order.Status = OrderStatus.Created;

        _context.ServiceOrders.Add(order);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Created new service order with ID {Id}", order.Id);

        return order;
    }

    public async Task<ServiceOrder> UpdateServiceOrderAsync(ServiceOrder order)
    {
        var existingOrder = await GetServiceOrderByIdAsync(order.Id);
        
        _context.Entry(existingOrder).CurrentValues.SetValues(order);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated service order with ID {Id}", order.Id);
        return order;
    }

    public async Task DeleteServiceOrderAsync(int id)
    {
        var order = await GetServiceOrderByIdAsync(id);
        
        _context.ServiceOrders.Remove(order);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted service order with ID {Id}", id);
    }

    public async Task AssignMechanicAsync(int orderId, string mechanicId)
    {
        var order = await GetServiceOrderByIdAsync(orderId);
        
        order.AssignedMechanicId = mechanicId;
        order.Status = OrderStatus.InProgress;
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Assigned mechanic {MechanicId} to order {OrderId}", mechanicId, orderId);
    }

    public async Task<Comment> AddCommentAsync(int orderId, Comment comment)
    {
        var order = await GetServiceOrderByIdAsync(orderId);
        
        comment.ServiceOrderId = orderId;
        comment.CreatedAt = DateTime.UtcNow;
        
        _context.Comments.Add(comment);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Added comment to order {OrderId}", orderId);
        return comment;
    }

    public async Task<IEnumerable<Comment>> GetOrderCommentsAsync(int orderId)
    {
        await GetServiceOrderByIdAsync(orderId); // Verify order exists
        
        return await _context.Comments
            .Where(c => c.ServiceOrderId == orderId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync();
    }

    public async Task<ServiceTask> AddServiceTaskAsync(int orderId, ServiceTask task)
    {
        var order = await GetServiceOrderByIdAsync(orderId);
        
        task.ServiceOrderId = orderId;
        _context.ServiceTasks.Add(task);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Added task to order {OrderId}", orderId);
        return task;
    }

    public async Task<Stream> GenerateReportAsync(DateTime startDate, DateTime endDate)
    {
        var orders = await _context.ServiceOrders
            .Include(o => o.Vehicle)
            .Include(o => o.Tasks)
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToListAsync();

        // TODO: Implement PDF generation using a library like iTextSharp or QuestPDF
        throw new NotImplementedException("PDF report generation not implemented yet");
    }

    public async Task<ServiceOrder> UpdateOrderStatusAsync(int orderId, OrderStatus status)
    {
        try
        {
            var order = await _context.ServiceOrders.FindAsync(orderId);
            if (order == null)
                throw new KeyNotFoundException($"Service order with ID {orderId} not found");

            order.Status = status;
            if (status == OrderStatus.Completed)
                order.CompletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating status for order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<ServiceOrder> AddTaskToOrderAsync(int orderId, ServiceTask task)
    {
        try
        {
            var order = await _context.ServiceOrders
                .Include(so => so.Tasks)
                .FirstOrDefaultAsync(so => so.Id == orderId);

            if (order == null)
                throw new KeyNotFoundException($"Service order with ID {orderId} not found");

            order.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding task to order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<ServiceOrder> AddCommentToOrderAsync(int orderId, Comment comment)
    {
        try
        {
            var order = await _context.ServiceOrders
                .Include(so => so.Comments)
                .FirstOrDefaultAsync(so => so.Id == orderId);

            if (order == null)
                throw new KeyNotFoundException($"Service order with ID {orderId} not found");

            comment.CreatedAt = DateTime.UtcNow;
            order.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return order;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding comment to order {OrderId}", orderId);
            throw;
        }
    }

    public async Task<byte[]> GenerateOrderPdfReportAsync(int orderId)
    {
        try
        {
            var order = await _context.ServiceOrders
                .Include(so => so.Vehicle)
                    .ThenInclude(v => v.Customer)
                .Include(so => so.Tasks)
                .FirstOrDefaultAsync(so => so.Id == orderId);

            if (order == null)
                throw new KeyNotFoundException($"Service order with ID {orderId} not found");

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            document.Add(new Paragraph($"Service Order #{order.Id}"));
            document.Add(new Paragraph($"Date: {order.CreatedAt:d}"));
            document.Add(new Paragraph($"Status: {order.Status}"));

            if (order.Vehicle != null)
            {
                document.Add(new Paragraph("Vehicle Information"));
                document.Add(new Paragraph($"Make: {order.Vehicle.Make}"));
                document.Add(new Paragraph($"Model: {order.Vehicle.Model}"));
                document.Add(new Paragraph($"Year: {order.Vehicle.Year}"));
                document.Add(new Paragraph($"VIN: {order.Vehicle.VIN}"));
            }

            if (order.Vehicle?.Customer != null)
            {
                document.Add(new Paragraph("Customer Information"));
                document.Add(new Paragraph($"Name: {order.Vehicle.Customer.FirstName} {order.Vehicle.Customer.LastName}"));
                document.Add(new Paragraph($"Email: {order.Vehicle.Customer.Email}"));
                document.Add(new Paragraph($"Phone: {order.Vehicle.Customer.PhoneNumber}"));
            }

            document.Add(new Paragraph("Service Tasks"));
            foreach (var task in order.Tasks)
            {
                document.Add(new Paragraph($"- {task.Description}"));
                document.Add(new Paragraph($"  Labor Cost: ${task.LaborCost:F2}"));
            }

            var total = await CalculateOrderTotalAsync(orderId);
            document.Add(new Paragraph($"Total Amount: ${total:F2}"));

            document.Close();
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating PDF report for order ID: {Id}", orderId);
            throw;
        }
    }

    public async Task<decimal> CalculateOrderTotalAsync(int orderId)
    {
        try
        {
            var order = await _context.ServiceOrders
                .Include(so => so.Tasks)
                .FirstOrDefaultAsync(so => so.Id == orderId);

            if (order == null)
                throw new KeyNotFoundException($"Service order with ID {orderId} not found");

            var laborTotal = order.Tasks.Sum(t => t.LaborCost);
            var partsTotal = order.UsedParts.Sum(up => up.Quantity * up.Part!.UnitPrice);

            return laborTotal + partsTotal;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while calculating total for order ID: {Id}", orderId);
            throw;
        }
    }

    public async Task<UsedPart> AddPartToOrderAsync(int orderId, UsedPart usedPart)
    {
        var order = await _context.ServiceOrders.Include(o => o.UsedParts).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) throw new KeyNotFoundException($"Service order with ID {orderId} not found");
        usedPart.ServiceOrderId = orderId;
        _context.UsedParts.Add(usedPart);
        await _context.SaveChangesAsync();
        return usedPart;
    }

    public async Task<IEnumerable<UsedPart>> GetOrderPartsAsync(int orderId)
    {
        var order = await _context.ServiceOrders.Include(o => o.UsedParts).ThenInclude(up => up.Part).FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) throw new KeyNotFoundException($"Service order with ID {orderId} not found");
        return order.UsedParts;
    }
} 