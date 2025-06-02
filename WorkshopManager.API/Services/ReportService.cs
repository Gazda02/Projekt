using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;
using WorkshopManager.API.Services.Interfaces;

namespace WorkshopManager.API.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReportService> _logger;

    public ReportService(ApplicationDbContext context, ILogger<ReportService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<byte[]> GenerateCustomerReportAsync(int customerId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var customer = await _context.Customers
                .Include(c => c.Vehicles)
                    .ThenInclude(v => v.ServiceOrders)
                        .ThenInclude(so => so.Tasks)
                .FirstOrDefaultAsync(c => c.Id == customerId);

            if (customer == null)
                throw new KeyNotFoundException($"Customer with ID {customerId} not found");

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            document.Add(new Paragraph($"Customer Report: {customer.FirstName} {customer.LastName}"));
            document.Add(new Paragraph($"Period: {startDate:d} - {endDate:d}"));

            foreach (var vehicle in customer.Vehicles)
            {
                var orders = vehicle.ServiceOrders
                    .Where(so => so.CreatedAt >= startDate && so.CreatedAt <= endDate)
                    .OrderBy(so => so.CreatedAt);

                if (!orders.Any()) continue;

                document.Add(new Paragraph($"Vehicle: {vehicle.Make} {vehicle.Model} ({vehicle.Year})"));
                document.Add(new Paragraph($"VIN: {vehicle.VIN}"));

                foreach (var order in orders)
                {
                    document.Add(new Paragraph($"Service Order #{order.Id} - {order.CreatedAt:d}"));
                    document.Add(new Paragraph($"Status: {order.Status}"));
                    
                    foreach (var task in order.Tasks)
                    {
                        document.Add(new Paragraph($"- {task.Description} (${task.LaborCost:F2})"));
                    }
                }
            }

            document.Close();
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating customer report for ID: {Id}", customerId);
            throw;
        }
    }

    public async Task<byte[]> GenerateVehicleReportAsync(int vehicleId, DateTime startDate, DateTime endDate)
    {
        try
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Customer)
                .Include(v => v.ServiceOrders)
                    .ThenInclude(so => so.Tasks)
                        .ThenInclude(t => t.UsedParts)
                            .ThenInclude(up => up.Part)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);

            if (vehicle == null)
                throw new KeyNotFoundException($"Vehicle with ID {vehicleId} not found");

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            document.Add(new Paragraph($"Vehicle Service History Report"));
            document.Add(new Paragraph($"Vehicle: {vehicle.Make} {vehicle.Model} ({vehicle.Year})"));
            document.Add(new Paragraph($"VIN: {vehicle.VIN}"));
            document.Add(new Paragraph($"Owner: {vehicle.Customer?.FirstName} {vehicle.Customer?.LastName}"));
            document.Add(new Paragraph($"Period: {startDate:d} - {endDate:d}"));

            var orders = vehicle.ServiceOrders
                .Where(so => so.CreatedAt >= startDate && so.CreatedAt <= endDate)
                .OrderBy(so => so.CreatedAt);

            foreach (var order in orders)
            {
                document.Add(new Paragraph($"Service Order #{order.Id} - {order.CreatedAt:d}"));
                document.Add(new Paragraph($"Status: {order.Status}"));

                foreach (var task in order.Tasks)
                {
                    document.Add(new Paragraph($"- {task.Description} (${task.LaborCost:F2})"));
                    foreach (var usedPart in task.UsedParts)
                    {
                        document.Add(new Paragraph($"  * {usedPart.Part?.Name} x{usedPart.Quantity}"));
                    }
                }
            }

            document.Close();
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating vehicle report for ID: {Id}", vehicleId);
            throw;
        }
    }

    public async Task<byte[]> GenerateMonthlyReportAsync(DateTime month)
    {
        try
        {
            var startDate = new DateTime(month.Year, month.Month, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            var orders = await _context.ServiceOrders
                .Include(so => so.Vehicle)
                .Include(so => so.Tasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(up => up.Part)
                .Where(so => so.CreatedAt >= startDate && so.CreatedAt <= endDate)
                .OrderBy(so => so.CreatedAt)
                .ToListAsync();

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            document.Add(new Paragraph($"Monthly Service Report - {month:MMMM yyyy}"));

            var totalRevenue = orders.SelectMany(o => o.Tasks).Sum(t => t.LaborCost);
            var totalParts = orders.SelectMany(o => o.Tasks)
                .SelectMany(t => t.UsedParts)
                .Sum(up => up.Quantity * up.Part!.UnitPrice);

            document.Add(new Paragraph($"Total Orders: {orders.Count}"));
            document.Add(new Paragraph($"Total Labor Revenue: ${totalRevenue:F2}"));
            document.Add(new Paragraph($"Total Parts Revenue: ${totalParts:F2}"));
            document.Add(new Paragraph($"Total Revenue: ${(totalRevenue + totalParts):F2}"));

            foreach (var order in orders)
            {
                document.Add(new Paragraph($"Order #{order.Id} - {order.Vehicle?.Make} {order.Vehicle?.Model}"));
                document.Add(new Paragraph($"Status: {order.Status}"));
                document.Add(new Paragraph($"Revenue: ${order.Tasks.Sum(t => t.LaborCost):F2}"));
            }

            document.Close();
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating monthly report for {Month}", month);
            throw;
        }
    }

    public async Task<byte[]> GenerateActiveOrdersReportAsync()
    {
        try
        {
            var orders = await _context.ServiceOrders
                .Include(so => so.Vehicle)
                .Include(so => so.Tasks)
                .Where(so => so.Status != OrderStatus.Completed && so.Status != OrderStatus.Cancelled)
                .OrderBy(so => so.CreatedAt)
                .ToListAsync();

            using var stream = new MemoryStream();
            using var writer = new PdfWriter(stream);
            using var pdf = new PdfDocument(writer);
            using var document = new Document(pdf);

            document.Add(new Paragraph("Active Service Orders Report"));
            document.Add(new Paragraph($"Generated on: {DateTime.Now:g}"));
            document.Add(new Paragraph($"Total Active Orders: {orders.Count}"));

            foreach (var order in orders)
            {
                document.Add(new Paragraph($"Order #{order.Id} - {order.Vehicle?.Make} {order.Vehicle?.Model}"));
                document.Add(new Paragraph($"Status: {order.Status}"));
                document.Add(new Paragraph($"Created: {order.CreatedAt:d}"));
                document.Add(new Paragraph($"Tasks: {order.Tasks.Count}"));
            }

            document.Close();
            return stream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating active orders report");
            throw;
        }
    }

    public async Task<Dictionary<string, decimal>> GetMechanicPerformanceStatsAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var orders = await _context.ServiceOrders
                .Include(so => so.Tasks)
                .Where(so => so.CreatedAt >= startDate && 
                            so.CreatedAt <= endDate && 
                            so.Status == OrderStatus.Completed &&
                            so.AssignedMechanicId != null)
                .ToListAsync();

            return orders
                .GroupBy(so => so.AssignedMechanicId!)
                .ToDictionary(
                    g => g.Key,
                    g => g.SelectMany(so => so.Tasks).Sum(t => t.LaborCost)
                );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting mechanic performance stats");
            throw;
        }
    }

    public async Task<Dictionary<string, decimal>> GetRevenueByServiceTypeAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            var tasks = await _context.ServiceTasks
                .Include(t => t.ServiceOrder)
                .Where(t => t.ServiceOrder!.CreatedAt >= startDate && 
                           t.ServiceOrder.CreatedAt <= endDate)
                .ToListAsync();

            return tasks
                .GroupBy(t => t.Description)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(t => t.LaborCost)
                );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting revenue by service type");
            throw;
        }
    }

    public async Task<IEnumerable<ServiceOrder>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.ServiceOrders
            .Include(o => o.Vehicle)
            .Include(o => o.Tasks)
            .ThenInclude(t => t.UsedParts)
            .ThenInclude(up => up.Part)
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .OrderBy(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceOrder>> GetOrdersByCustomerAsync(int customerId)
    {
        return await _context.ServiceOrders
            .Include(o => o.Vehicle)
            .Include(o => o.Tasks)
            .ThenInclude(t => t.UsedParts)
            .ThenInclude(up => up.Part)
            .Where(o => o.Vehicle.CustomerId == customerId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceOrder>> GetOrdersByVehicleAsync(int vehicleId)
    {
        return await _context.ServiceOrders
            .Include(o => o.Vehicle)
            .Include(o => o.Tasks)
            .ThenInclude(t => t.UsedParts)
            .ThenInclude(up => up.Part)
            .Where(o => o.VehicleId == vehicleId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<ServiceOrder>> GetOrdersByStatusAsync(OrderStatus status)
    {
        return await _context.ServiceOrders
            .Include(o => o.Vehicle)
            .Include(o => o.Tasks)
            .ThenInclude(t => t.UsedParts)
            .ThenInclude(up => up.Part)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();
    }

    public async Task<Dictionary<string, decimal>> GetMonthlyRevenueReportAsync(int year, int month)
    {
        var startDate = new DateTime(year, month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var orders = await _context.ServiceOrders
            .Include(o => o.Tasks)
            .ThenInclude(t => t.UsedParts)
            .ThenInclude(up => up.Part)
            .Where(o => o.CreatedAt >= startDate && o.CreatedAt <= endDate)
            .ToListAsync();

        return new Dictionary<string, decimal>
        {
            { "TotalRevenue", orders.Sum(o => o.TotalCost) },
            { "LaborRevenue", orders.Sum(o => o.Tasks.Sum(t => t.LaborCost)) },
            { "PartsRevenue", orders.Sum(o => o.Tasks.Sum(t => t.UsedParts.Sum(up => up.Part.UnitPrice * up.Quantity))) },
            { "CompletedOrders", orders.Count(o => o.Status == OrderStatus.Completed) },
            { "InProgressOrders", orders.Count(o => o.Status == OrderStatus.InProgress) }
        };
    }

    public async Task<Dictionary<string, int>> GetOrderStatusSummaryAsync()
    {
        var orders = await _context.ServiceOrders.ToListAsync();

        return new Dictionary<string, int>
        {
            { OrderStatus.Created.ToString(), orders.Count(o => o.Status == OrderStatus.Created) },
            { OrderStatus.InProgress.ToString(), orders.Count(o => o.Status == OrderStatus.InProgress) },
            { OrderStatus.Completed.ToString(), orders.Count(o => o.Status == OrderStatus.Completed) },
            { OrderStatus.Cancelled.ToString(), orders.Count(o => o.Status == OrderStatus.Cancelled) }
        };
    }
} 