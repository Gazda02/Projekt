using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;
using WorkshopManager.API.Services.Interfaces;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using OfficeOpenXml;

namespace WorkshopManager.API.Services;

public class ReportService : IReportService
{
    private readonly ApplicationDbContext _context;
    private readonly string _reportDirectory;

    public ReportService(ApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _reportDirectory = configuration.GetValue<string>("FileStorage:ReportDirectory") 
            ?? Path.Combine(Directory.GetCurrentDirectory(), "Reports");

        if (!Directory.Exists(_reportDirectory))
        {
            Directory.CreateDirectory(_reportDirectory);
        }

        // Set EPPlus license
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<Report> GenerateServiceOrderReportAsync(int serviceOrderId, string format = "PDF")
    {
        var serviceOrder = await _context.ServiceOrders
            .Include(so => so.Vehicle)
                .ThenInclude(v => v.Customer)
            .Include(so => so.Tasks)
                .ThenInclude(t => t.UsedParts)
                    .ThenInclude(up => up.Part)
            .FirstOrDefaultAsync(so => so.Id == serviceOrderId);

        if (serviceOrder == null)
            throw new KeyNotFoundException($"Service order with ID {serviceOrderId} not found.");

        string fileName = $"ServiceOrder_{serviceOrderId}_{DateTime.UtcNow:yyyyMMddHHmmss}.{format.ToLower()}";
        string filePath = Path.Combine(_reportDirectory, fileName);

        if (format.ToUpper() == "PDF")
        {
            await GenerateServiceOrderPdfAsync(serviceOrder, filePath);
        }
        else
        {
            throw new ArgumentException($"Format {format} is not supported for service order reports.");
        }

        var report = new Report
        {
            Title = $"Service Order Report - {serviceOrder.Vehicle.Make} {serviceOrder.Vehicle.Model}",
            Type = "ServiceOrder",
            GeneratedDate = DateTime.UtcNow,
            FilePath = fileName,
            Format = format,
            ServiceOrderId = serviceOrderId,
            VehicleId = serviceOrder.VehicleId,
            CustomerId = serviceOrder.Vehicle.CustomerId
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return report;
    }

    public async Task<Report> GenerateVehicleHistoryReportAsync(int vehicleId, DateTime? startDate = null, DateTime? endDate = null, string format = "PDF")
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Customer)
            .Include(v => v.ServiceOrders)
                .ThenInclude(so => so.Tasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(up => up.Part)
            .FirstOrDefaultAsync(v => v.Id == vehicleId);

        if (vehicle == null)
            throw new KeyNotFoundException($"Vehicle with ID {vehicleId} not found.");

        string fileName = $"VehicleHistory_{vehicleId}_{DateTime.UtcNow:yyyyMMddHHmmss}.{format.ToLower()}";
        string filePath = Path.Combine(_reportDirectory, fileName);

        if (format.ToUpper() == "PDF")
        {
            await GenerateVehicleHistoryPdfAsync(vehicle, startDate, endDate, filePath);
        }
        else
        {
            throw new ArgumentException($"Format {format} is not supported for vehicle history reports.");
        }

        var report = new Report
        {
            Title = $"Vehicle History Report - {vehicle.Make} {vehicle.Model}",
            Type = "VehicleHistory",
            GeneratedDate = DateTime.UtcNow,
            FilePath = fileName,
            Format = format,
            VehicleId = vehicleId,
            CustomerId = vehicle.CustomerId,
            StartDate = startDate,
            EndDate = endDate
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return report;
    }

    public async Task<Report> GenerateFinancialReportAsync(DateTime startDate, DateTime endDate, string format = "Excel")
    {
        var serviceOrders = await _context.ServiceOrders
            .Include(so => so.Tasks)
                .ThenInclude(t => t.UsedParts)
                    .ThenInclude(up => up.Part)
            .Where(so => so.CreatedAt >= startDate && so.CreatedAt <= endDate)
            .ToListAsync();

        string fileName = $"FinancialReport_{startDate:yyyyMMdd}_{endDate:yyyyMMdd}_{DateTime.UtcNow:yyyyMMddHHmmss}.{format.ToLower()}";
        string filePath = Path.Combine(_reportDirectory, fileName);

        if (format.ToUpper() == "EXCEL")
        {
            await GenerateFinancialExcelAsync(serviceOrders, startDate, endDate, filePath);
        }
        else
        {
            throw new ArgumentException($"Format {format} is not supported for financial reports.");
        }

        var report = new Report
        {
            Title = $"Financial Report ({startDate:d} - {endDate:d})",
            Type = "Financial",
            GeneratedDate = DateTime.UtcNow,
            FilePath = fileName,
            Format = format,
            StartDate = startDate,
            EndDate = endDate
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return report;
    }

    public async Task<Report> GenerateCustomerHistoryReportAsync(int customerId, DateTime? startDate = null, DateTime? endDate = null, string format = "PDF")
    {
        var customer = await _context.Customers
            .Include(c => c.Vehicles)
                .ThenInclude(v => v.ServiceOrders)
                    .ThenInclude(so => so.Tasks)
            .FirstOrDefaultAsync(c => c.Id == customerId);

        if (customer == null)
            throw new KeyNotFoundException($"Customer with ID {customerId} not found.");

        string fileName = $"CustomerHistory_{customerId}_{DateTime.UtcNow:yyyyMMddHHmmss}.{format.ToLower()}";
        string filePath = Path.Combine(_reportDirectory, fileName);

        if (format.ToUpper() == "PDF")
        {
            await GenerateCustomerHistoryPdfAsync(customer, startDate, endDate, filePath);
        }
        else
        {
            throw new ArgumentException($"Format {format} is not supported for customer history reports.");
        }

        var report = new Report
        {
            Title = $"Customer History Report - {customer.FirstName} {customer.LastName}",
            Type = "CustomerHistory",
            GeneratedDate = DateTime.UtcNow,
            FilePath = fileName,
            Format = format,
            CustomerId = customerId,
            StartDate = startDate,
            EndDate = endDate
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return report;
    }

    public async Task<Report> GenerateInventoryReportAsync(string format = "Excel")
    {
        var parts = await _context.Parts
            .Include(p => p.UsedInTasks)
            .ToListAsync();

        string fileName = $"InventoryReport_{DateTime.UtcNow:yyyyMMddHHmmss}.{format.ToLower()}";
        string filePath = Path.Combine(_reportDirectory, fileName);

        if (format.ToUpper() == "EXCEL")
        {
            await GenerateInventoryExcelAsync(parts, filePath);
        }
        else
        {
            throw new ArgumentException($"Format {format} is not supported for inventory reports.");
        }

        var report = new Report
        {
            Title = "Inventory Report",
            Type = "Inventory",
            GeneratedDate = DateTime.UtcNow,
            FilePath = fileName,
            Format = format
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        return report;
    }

    public async Task<IEnumerable<Report>> GetReportHistoryAsync(string type = null, DateTime? startDate = null, DateTime? endDate = null)
    {
        IQueryable<Report> query = _context.Reports;

        if (!string.IsNullOrEmpty(type))
            query = query.Where(r => r.Type == type);

        if (startDate.HasValue)
            query = query.Where(r => r.GeneratedDate >= startDate.Value);

        if (endDate.HasValue)
            query = query.Where(r => r.GeneratedDate <= endDate.Value);

        return await query.OrderByDescending(r => r.GeneratedDate).ToListAsync();
    }

    public async Task<byte[]> GetReportFileAsync(int reportId)
    {
        var report = await _context.Reports.FindAsync(reportId);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID {reportId} not found.");

        string filePath = Path.Combine(_reportDirectory, report.FilePath);
        if (!File.Exists(filePath))
            throw new FileNotFoundException($"Report file not found at {filePath}");

        return await File.ReadAllBytesAsync(filePath);
    }

    public async Task DeleteReportAsync(int reportId)
    {
        var report = await _context.Reports.FindAsync(reportId);
        if (report == null)
            throw new KeyNotFoundException($"Report with ID {reportId} not found.");

        string filePath = Path.Combine(_reportDirectory, report.FilePath);
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }

        _context.Reports.Remove(report);
        await _context.SaveChangesAsync();
    }

    private async Task GenerateServiceOrderPdfAsync(ServiceOrder order, string filePath)
    {
        using var writer = new PdfWriter(filePath);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf);

        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        // Title
        var title = new Paragraph($"Service Order #{order.Id}")
            .SetFont(boldFont)
            .SetFontSize(18);
        document.Add(title);

        // Vehicle Information
        document.Add(new Paragraph("Vehicle Information")
            .SetFont(boldFont)
            .SetFontSize(14));
        document.Add(new Paragraph($"Make: {order.Vehicle?.Make ?? "N/A"}"));
        document.Add(new Paragraph($"Model: {order.Vehicle?.Model ?? "N/A"}"));
        document.Add(new Paragraph($"Year: {order.Vehicle?.Year ?? 0}"));
        document.Add(new Paragraph($"VIN: {order.Vehicle?.VIN ?? "N/A"}"));

        // Customer Information
        document.Add(new Paragraph("Customer Information")
            .SetFont(boldFont)
            .SetFontSize(14));
        document.Add(new Paragraph($"Name: {order.Vehicle?.Customer?.FirstName ?? "N/A"} {order.Vehicle?.Customer?.LastName ?? ""}"));
        document.Add(new Paragraph($"Email: {order.Vehicle?.Customer?.Email ?? "N/A"}"));
        document.Add(new Paragraph($"Phone: {order.Vehicle?.Customer?.PhoneNumber ?? "N/A"}"));

        // Service Details
        document.Add(new Paragraph("Service Details")
            .SetFont(boldFont)
            .SetFontSize(14));
        document.Add(new Paragraph($"Status: {order.Status}"));
        document.Add(new Paragraph($"Created: {order.CreatedAt:g}"));
        document.Add(new Paragraph($"Description: {order.Description}"));

        // Tasks
        document.Add(new Paragraph("Tasks")
            .SetFont(boldFont)
            .SetFontSize(14));
        if (order.Tasks != null)
        {
            foreach (var task in order.Tasks)
            {
                document.Add(new Paragraph($"• {task.Description}"));
                if (task.UsedParts != null && task.UsedParts.Any())
                {
                    var parts = new List<string>();
                    foreach (var usedPart in task.UsedParts)
                    {
                        parts.Add($"  - {usedPart.Part?.Name ?? "N/A"} (Quantity: {usedPart.Quantity})");
                    }
                    document.Add(new Paragraph(string.Join("\n", parts)));
                }
            }
        }

        // Total Cost
        document.Add(new Paragraph("Cost Summary")
            .SetFont(boldFont)
            .SetFontSize(14));
        document.Add(new Paragraph($"Total Cost: ${order.TotalCost:F2}"));
    }

    private async Task GenerateVehicleHistoryPdfAsync(Vehicle vehicle, DateTime? startDate, DateTime? endDate, string filePath)
    {
        using var writer = new PdfWriter(filePath);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf);

        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        // Title
        document.Add(new Paragraph($"Vehicle History Report")
            .SetFont(boldFont)
            .SetFontSize(18));

        // Vehicle Information
        document.Add(new Paragraph("Vehicle Information")
            .SetFont(boldFont)
            .SetFontSize(14));
        document.Add(new Paragraph($"Make: {vehicle.Make}"));
        document.Add(new Paragraph($"Model: {vehicle.Model}"));
        document.Add(new Paragraph($"Year: {vehicle.Year}"));
        document.Add(new Paragraph($"VIN: {vehicle.VIN}"));

        // Filter service orders by date if specified
        var serviceOrders = vehicle.ServiceOrders
            .Where(so => (!startDate.HasValue || so.CreatedAt >= startDate.Value) &&
                        (!endDate.HasValue || so.CreatedAt <= endDate.Value))
            .OrderByDescending(so => so.CreatedAt);

        // Service History
        document.Add(new Paragraph("Service History")
            .SetFont(boldFont)
            .SetFontSize(14));

        foreach (var order in serviceOrders)
        {
            document.Add(new Paragraph($"Service Order #{order.Id}")
                .SetFont(boldFont));
            document.Add(new Paragraph($"Date: {order.CreatedAt:d}"));
            document.Add(new Paragraph($"Status: {order.Status}"));
            document.Add(new Paragraph($"Description: {order.Description}"));
            
            if (order.Tasks.Any())
            {
                document.Add(new Paragraph("Tasks:"));
                foreach (var task in order.Tasks)
                {
                    document.Add(new Paragraph($"• {task.Description}"));
                }
            }
            
            document.Add(new Paragraph($"Cost: ${order.TotalCost:F2}"));
            document.Add(new Paragraph(""));
        }
    }

    private async Task GenerateFinancialExcelAsync(List<ServiceOrder> orders, DateTime startDate, DateTime endDate, string filePath)
    {
        using var package = new ExcelPackage(new FileInfo(filePath));
        
        // Summary worksheet
        var summarySheet = package.Workbook.Worksheets.Add("Summary");
        summarySheet.Cells[1, 1].Value = "Financial Report";
        summarySheet.Cells[2, 1].Value = $"Period: {startDate:d} - {endDate:d}";

        decimal totalRevenue = 0;
        decimal totalLaborCost = 0;
        decimal totalPartsCost = 0;

        // Detailed orders worksheet
        var ordersSheet = package.Workbook.Worksheets.Add("Orders");
        ordersSheet.Cells[1, 1].Value = "Order ID";
        ordersSheet.Cells[1, 2].Value = "Date";
        ordersSheet.Cells[1, 3].Value = "Labor Cost";
        ordersSheet.Cells[1, 4].Value = "Parts Cost";
        ordersSheet.Cells[1, 5].Value = "Total";

        int row = 2;
        foreach (var order in orders)
        {
            decimal orderLaborCost = order.Tasks.Sum(t => t.LaborCost);
            decimal orderPartsCost = order.Tasks.Sum(t => 
                t.UsedParts.Sum(up => up.Part.UnitPrice * up.Quantity));

            ordersSheet.Cells[row, 1].Value = order.Id;
            ordersSheet.Cells[row, 2].Value = order.CreatedAt;
            ordersSheet.Cells[row, 3].Value = orderLaborCost;
            ordersSheet.Cells[row, 4].Value = orderPartsCost;
            ordersSheet.Cells[row, 5].Formula = $"=C{row}+D{row}";

            totalLaborCost += orderLaborCost;
            totalPartsCost += orderPartsCost;
            row++;
        }

        totalRevenue = totalLaborCost + totalPartsCost;

        // Update summary
        summarySheet.Cells[4, 1].Value = "Total Revenue:";
        summarySheet.Cells[4, 2].Value = totalRevenue;
        summarySheet.Cells[5, 1].Value = "Total Labor Cost:";
        summarySheet.Cells[5, 2].Value = totalLaborCost;
        summarySheet.Cells[6, 1].Value = "Total Parts Cost:";
        summarySheet.Cells[6, 2].Value = totalPartsCost;

        await package.SaveAsync();
    }

    private async Task GenerateCustomerHistoryPdfAsync(Customer customer, DateTime? startDate, DateTime? endDate, string filePath)
    {
        using var writer = new PdfWriter(filePath);
        using var pdf = new PdfDocument(writer);
        using var document = new Document(pdf);

        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

        // Title
        document.Add(new Paragraph("Customer History Report")
            .SetFont(boldFont)
            .SetFontSize(18));

        // Customer Information
        document.Add(new Paragraph("Customer Information")
            .SetFont(boldFont)
            .SetFontSize(14));
        document.Add(new Paragraph($"Name: {customer.FirstName} {customer.LastName}"));
        document.Add(new Paragraph($"Email: {customer.Email}"));
        document.Add(new Paragraph($"Phone: {customer.PhoneNumber ?? "N/A"}"));

        // Vehicles
        document.Add(new Paragraph("Vehicles")
            .SetFont(boldFont)
            .SetFontSize(14));

        if (customer.Vehicles != null)
        {
            foreach (var vehicle in customer.Vehicles)
            {
                document.Add(new Paragraph($"{vehicle.Year} {vehicle.Make} {vehicle.Model}")
                    .SetFont(boldFont));
                document.Add(new Paragraph($"VIN: {vehicle.VIN}"));

                var serviceOrders = vehicle.ServiceOrders?
                    .Where(so => (!startDate.HasValue || so.CreatedAt >= startDate.Value) &&
                                (!endDate.HasValue || so.CreatedAt <= endDate.Value))
                    .OrderByDescending(so => so.CreatedAt);

                if (serviceOrders != null && serviceOrders.Any())
                {
                    document.Add(new Paragraph("Service History:"));
                    foreach (var order in serviceOrders)
                    {
                        document.Add(new Paragraph($"• Service Order #{order.Id} - {order.CreatedAt:d}"));
                        document.Add(new Paragraph($"  Status: {order.Status}"));
                        document.Add(new Paragraph($"  Cost: ${order.TotalCost:F2}"));
                    }
                }
                document.Add(new Paragraph(""));
            }
        }
    }

    private async Task GenerateInventoryExcelAsync(List<Part> parts, string filePath)
    {
        using var package = new ExcelPackage(new FileInfo(filePath));
        var worksheet = package.Workbook.Worksheets.Add("Inventory");

        // Headers
        worksheet.Cells[1, 1].Value = "Part Number";
        worksheet.Cells[1, 2].Value = "Name";
        worksheet.Cells[1, 3].Value = "Unit Price";
        worksheet.Cells[1, 4].Value = "Stock Quantity";
        worksheet.Cells[1, 5].Value = "Total Value";
        worksheet.Cells[1, 6].Value = "Used in Tasks";

        int row = 2;
        foreach (var part in parts)
        {
            worksheet.Cells[row, 1].Value = part.PartNumber;
            worksheet.Cells[row, 2].Value = part.Name;
            worksheet.Cells[row, 3].Value = part.UnitPrice;
            worksheet.Cells[row, 4].Value = part.StockQuantity;
            worksheet.Cells[row, 5].Formula = $"=C{row}*D{row}";
            worksheet.Cells[row, 6].Value = part.UsedInTasks?.Count ?? 0;
            row++;
        }

        // Add totals
        worksheet.Cells[row, 1].Value = "Totals";
        worksheet.Cells[row, 3].Formula = $"=SUM(C2:C{row-1})";
        worksheet.Cells[row, 4].Formula = $"=SUM(D2:D{row-1})";
        worksheet.Cells[row, 5].Formula = $"=SUM(E2:E{row-1})";

        // Format currency
        worksheet.Cells[2, 3, row, 3].Style.Numberformat.Format = "$#,##0.00";
        worksheet.Cells[2, 5, row, 5].Style.Numberformat.Format = "$#,##0.00";

        await package.SaveAsync();
    }
} 