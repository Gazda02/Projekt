using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;
using WorkshopManager.API.Services.Interfaces;

namespace WorkshopManager.API.Services;

public class ServiceTaskService : IServiceTaskService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ServiceTaskService> _logger;

    public ServiceTaskService(ApplicationDbContext context, ILogger<ServiceTaskService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<ServiceTask>> GetAllServiceTasksAsync()
    {
        return await _context.ServiceTasks
            .Include(t => t.UsedParts)
            .ThenInclude(up => up.Part)
            .ToListAsync();
    }

    public async Task<ServiceTask> GetServiceTaskByIdAsync(int id)
    {
        var task = await _context.ServiceTasks
            .Include(t => t.UsedParts)
            .ThenInclude(up => up.Part)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task == null)
        {
            _logger.LogWarning("Service task with ID {Id} not found", id);
            throw new KeyNotFoundException($"Service task with ID {id} not found");
        }

        return task;
    }

    public async Task<ServiceTask> CreateServiceTaskAsync(ServiceTask task)
    {
        _context.ServiceTasks.Add(task);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created new service task with ID {Id}", task.Id);
        return task;
    }

    public async Task<ServiceTask> UpdateServiceTaskAsync(ServiceTask task)
    {
        var existingTask = await GetServiceTaskByIdAsync(task.Id);
        
        _context.Entry(existingTask).CurrentValues.SetValues(task);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated service task with ID {Id}", task.Id);
        return task;
    }

    public async Task DeleteServiceTaskAsync(int id)
    {
        var task = await GetServiceTaskByIdAsync(id);
        
        _context.ServiceTasks.Remove(task);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted service task with ID {Id}", id);
    }

    public async Task<UsedPart> AddPartToTaskAsync(int taskId, UsedPart usedPart)
    {
        var task = await GetServiceTaskByIdAsync(taskId);
        
        usedPart.ServiceTaskId = taskId;
        _context.UsedParts.Add(usedPart);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Added part {PartId} to task {TaskId}", usedPart.PartId, taskId);
        return usedPart;
    }

    public async Task<IEnumerable<UsedPart>> GetTaskPartsAsync(int taskId)
    {
        return await _context.UsedParts
            .Include(up => up.Part)
            .Where(up => up.ServiceTaskId == taskId)
            .ToListAsync();
    }

    public async Task CompleteTaskAsync(int taskId)
    {
        var task = await GetServiceTaskByIdAsync(taskId);
        
        task.IsCompleted = true;
        task.CompletedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Marked task {TaskId} as completed", taskId);
    }

    public async Task<IEnumerable<ServiceTask>> GetMechanicTasksAsync(int mechanicId)
    {
        return await _context.ServiceTasks
            .Include(t => t.UsedParts)
            .ThenInclude(up => up.Part)
            .Where(t => t.AssignedMechanicId == mechanicId)
            .ToListAsync();
    }
} 