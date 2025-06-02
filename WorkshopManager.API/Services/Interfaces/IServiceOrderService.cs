using WorkshopManager.API.Models;

namespace WorkshopManager.API.Services.Interfaces;

public interface IServiceOrderService
{
    Task<IEnumerable<ServiceOrder>> GetAllServiceOrdersAsync();
    Task<IEnumerable<ServiceOrder>> GetActiveServiceOrdersAsync();
    Task<ServiceOrder> GetServiceOrderByIdAsync(int id);
    Task<ServiceOrder> CreateServiceOrderAsync(ServiceOrder order);
    Task<ServiceOrder> UpdateServiceOrderAsync(ServiceOrder order);
    Task DeleteServiceOrderAsync(int id);
    Task AssignMechanicAsync(int orderId, string mechanicId);
    Task<Comment> AddCommentAsync(int orderId, Comment comment);
    Task<IEnumerable<Comment>> GetOrderCommentsAsync(int orderId);
    Task<ServiceTask> AddServiceTaskAsync(int orderId, ServiceTask task);
    Task<Stream> GenerateReportAsync(DateTime startDate, DateTime endDate);
} 