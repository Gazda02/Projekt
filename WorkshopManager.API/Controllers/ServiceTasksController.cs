using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkshopManager.API.Models;
using WorkshopManager.API.Services.Interfaces;

namespace WorkshopManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceTasksController : ControllerBase
{
    private readonly IServiceTaskService _serviceTaskService;
    private readonly ILogger<ServiceTasksController> _logger;

    public ServiceTasksController(IServiceTaskService serviceTaskService, ILogger<ServiceTasksController> logger)
    {
        _serviceTaskService = serviceTaskService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceTask>>> GetServiceTasks()
    {
        try
        {
            var tasks = await _serviceTaskService.GetAllServiceTasksAsync();
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all service tasks");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceTask>> GetServiceTask(int id)
    {
        try
        {
            var task = await _serviceTaskService.GetServiceTaskByIdAsync(id);
            if (task == null)
            {
                return NotFound();
            }
            return Ok(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting service task with ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<ActionResult<ServiceTask>> CreateServiceTask(ServiceTask task)
    {
        try
        {
            var createdTask = await _serviceTaskService.CreateServiceTaskAsync(task);
            return CreatedAtAction(nameof(GetServiceTask), new { id = createdTask.Id }, createdTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating service task");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Receptionist,Mechanic")]
    public async Task<IActionResult> UpdateServiceTask(int id, ServiceTask task)
    {
        if (id != task.Id)
        {
            return BadRequest();
        }

        try
        {
            await _serviceTaskService.UpdateServiceTaskAsync(task);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating service task with ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeleteServiceTask(int id)
    {
        try
        {
            await _serviceTaskService.DeleteServiceTaskAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting service task with ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("{id}/add-part")]
    [Authorize(Roles = "Admin,Receptionist,Mechanic")]
    public async Task<ActionResult<UsedPart>> AddPartToTask(int id, [FromBody] UsedPart usedPart)
    {
        try
        {
            var addedPart = await _serviceTaskService.AddPartToTaskAsync(id, usedPart);
            return Ok(addedPart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding part to task ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("{id}/parts")]
    public async Task<ActionResult<IEnumerable<UsedPart>>> GetTaskParts(int id)
    {
        try
        {
            var parts = await _serviceTaskService.GetTaskPartsAsync(id);
            return Ok(parts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting parts for task ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("{id}/complete")]
    [Authorize(Roles = "Admin,Mechanic")]
    public async Task<IActionResult> CompleteTask(int id)
    {
        try
        {
            await _serviceTaskService.CompleteTaskAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while completing task ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("mechanic/{mechanicId}")]
    public async Task<ActionResult<IEnumerable<ServiceTask>>> GetMechanicTasks(int mechanicId)
    {
        try
        {
            var tasks = await _serviceTaskService.GetMechanicTasksAsync(mechanicId);
            return Ok(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting tasks for mechanic ID: {Id}", mechanicId);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
} 