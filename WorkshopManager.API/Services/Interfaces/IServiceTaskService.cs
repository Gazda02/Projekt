using WorkshopManager.API.Models;

namespace WorkshopManager.API.Services.Interfaces;

public interface IServiceTaskService
{
    Task<IEnumerable<ServiceTask>> GetAllServiceTasksAsync();
    Task<ServiceTask> GetServiceTaskByIdAsync(int id);
    Task<ServiceTask> CreateServiceTaskAsync(ServiceTask task);
    Task<ServiceTask> UpdateServiceTaskAsync(ServiceTask task);
    Task DeleteServiceTaskAsync(int id);
    Task<UsedPart> AddPartToTaskAsync(int taskId, UsedPart usedPart);
    Task<IEnumerable<UsedPart>> GetTaskPartsAsync(int taskId);
    Task CompleteTaskAsync(int taskId);
    Task<IEnumerable<ServiceTask>> GetMechanicTasksAsync(int mechanicId);
} 