using Microsoft.AspNetCore.Mvc;
using WorkshopManager.API.Mapperly;
using WorkshopManager.API.Models;
using WorkshopManager.API.Services.Interfaces;

namespace WorkshopManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ServiceOrdersController : ControllerBase
{
    private readonly IServiceOrderService _serviceOrderService;
    private readonly ILogger<ServiceOrdersController> _logger;
    private readonly OrderMapper _mapper;
    private readonly CommentMapper _commentMapper;

    public ServiceOrdersController(IServiceOrderService serviceOrderService, ILogger<ServiceOrdersController> logger, OrderMapper mapper, CommentMapper commentMapper)
    {
        _serviceOrderService = serviceOrderService;
        _logger = logger;
        _mapper = mapper;
        _commentMapper = commentMapper;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ServiceOrder>>> GetServiceOrders()
    {
        try
        {
            var orders = await _serviceOrderService.GetAllServiceOrdersAsync();
            return Ok(orders.Select(x => _mapper.Map(x)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all service orders");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("active")]
    public async Task<ActionResult<IEnumerable<ServiceOrder>>> GetActiveServiceOrders()
    {
        try
        {
            var orders = await _serviceOrderService.GetActiveServiceOrdersAsync();
            return Ok(orders.Select(x => _mapper.Map(x)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting active service orders");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ServiceOrder>> GetServiceOrder(int id)
    {
        try
        {
            var order = await _serviceOrderService.GetServiceOrderByIdAsync(id);
            if (order == null)
            {
                return NotFound();
            }
            return Ok(_mapper.Map(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting service order with ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost]
    public async Task<ActionResult<ServiceOrder>> CreateServiceOrder(ServiceOrder order)
    {
        try
        {
            var createdOrder = await _serviceOrderService.CreateServiceOrderAsync(order);
            return CreatedAtAction(nameof(GetServiceOrder), new { id = createdOrder.Id }, createdOrder);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating service order");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateServiceOrder(int id, ServiceOrder order)
    {
        if (id != order.Id)
        {
            return BadRequest();
        }

        try
        {
            await _serviceOrderService.UpdateServiceOrderAsync(order);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating service order with ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteServiceOrder(int id)
    {
        try
        {
            await _serviceOrderService.DeleteServiceOrderAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting service order with ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("{id}/assign-mechanic")]
    public async Task<IActionResult> AssignMechanic(int id, [FromBody] string mechanicId)
    {
        try
        {
            await _serviceOrderService.AssignMechanicAsync(id, mechanicId);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while assigning mechanic {MechanicId} to order {OrderId}", mechanicId, id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("{id}/add-comment")]
    public async Task<ActionResult<Comment>> AddComment(int id, [FromBody] Comment comment)
    {
        try
        {
            var addedComment = await _serviceOrderService.AddCommentAsync(id, comment);
            return Ok(addedComment);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding comment to order {OrderId}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("{id}/comments")]
    public async Task<ActionResult<IEnumerable<Comment>>> GetComments(int id)
    {
        try
        {
            var comments = await _serviceOrderService.GetOrderCommentsAsync(id);
            return Ok(comments.Select(c => _commentMapper.Map(c)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting comments for order {OrderId}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("{id}/add-task")]
    public async Task<ActionResult<ServiceTask>> AddServiceTask(int id, [FromBody] ServiceTask task)
    {
        try
        {
            var addedTask = await _serviceOrderService.AddServiceTaskAsync(id, task);
            return Ok(addedTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adding task to order {OrderId}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("report")]
    public async Task<ActionResult> GenerateReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        try
        {
            var reportStream = await _serviceOrderService.GenerateReportAsync(startDate, endDate);
            return File(reportStream, "application/pdf", "service-orders-report.pdf");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while generating service orders report");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
} 