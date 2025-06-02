using WorkshopManager.API.Models;

namespace WorkshopManager.API.Services.Interfaces;

public interface IReportService
{
    Task<byte[]> GenerateCustomerReportAsync(int customerId, DateTime startDate, DateTime endDate);
    Task<byte[]> GenerateVehicleReportAsync(int vehicleId, DateTime startDate, DateTime endDate);
    Task<byte[]> GenerateMonthlyReportAsync(DateTime month);
    Task<byte[]> GenerateActiveOrdersReportAsync();
    Task<Dictionary<string, decimal>> GetMechanicPerformanceStatsAsync(DateTime startDate, DateTime endDate);
    Task<Dictionary<string, decimal>> GetRevenueByServiceTypeAsync(DateTime startDate, DateTime endDate);
} 