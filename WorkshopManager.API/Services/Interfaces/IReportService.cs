using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Services.Interfaces;

public interface IReportService
{
    Task<Report> GenerateServiceOrderReportAsync(int serviceOrderId, string format = "PDF");
    Task<Report> GenerateVehicleHistoryReportAsync(int vehicleId, DateTime? startDate = null, DateTime? endDate = null, string format = "PDF");
    Task<Report> GenerateFinancialReportAsync(DateTime startDate, DateTime endDate, string format = "Excel");
    Task<Report> GenerateCustomerHistoryReportAsync(int customerId, DateTime? startDate = null, DateTime? endDate = null, string format = "PDF");
    Task<Report> GenerateInventoryReportAsync(string format = "Excel");
    Task<IEnumerable<Report>> GetReportHistoryAsync(string type = null, DateTime? startDate = null, DateTime? endDate = null);
    Task<byte[]> GetReportFileAsync(int reportId);
    Task DeleteReportAsync(int reportId);
} 