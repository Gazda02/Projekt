using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkshopManager.API.Models;
using WorkshopManager.API.Services.Interfaces;

namespace WorkshopManager.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PartsController : ControllerBase
{
    private readonly IPartService _partService;
    private readonly ILogger<PartsController> _logger;

    public PartsController(IPartService partService, ILogger<PartsController> logger)
    {
        _partService = partService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Part>>> GetParts()
    {
        try
        {
            var parts = await _partService.GetAllPartsAsync();
            return Ok(parts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all parts");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Part>> GetPart(int id)
    {
        try
        {
            var part = await _partService.GetPartByIdAsync(id);
            if (part == null)
            {
                return NotFound();
            }
            return Ok(part);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting part with ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<ActionResult<Part>> CreatePart(Part part)
    {
        try
        {
            var createdPart = await _partService.CreatePartAsync(part);
            return CreatedAtAction(nameof(GetPart), new { id = createdPart.Id }, createdPart);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating part");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> UpdatePart(int id, Part part)
    {
        if (id != part.Id)
        {
            return BadRequest();
        }

        try
        {
            await _partService.UpdatePartAsync(part);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating part with ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> DeletePart(int id)
    {
        try
        {
            await _partService.DeletePartAsync(id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting part with ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<Part>>> SearchParts([FromQuery] string query)
    {
        try
        {
            var parts = await _partService.SearchPartsAsync(query);
            return Ok(parts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while searching parts with query: {Query}", query);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpGet("low-stock")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<ActionResult<IEnumerable<Part>>> GetLowStockParts()
    {
        try
        {
            var parts = await _partService.GetLowStockPartsAsync();
            return Ok(parts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting low stock parts");
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }

    [HttpPost("{id}/adjust-stock")]
    [Authorize(Roles = "Admin,Receptionist")]
    public async Task<IActionResult> AdjustStock(int id, [FromBody] int quantity)
    {
        try
        {
            await _partService.AdjustStockAsync(id, quantity);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while adjusting stock for part ID: {Id}", id);
            return StatusCode(500, "An error occurred while processing your request.");
        }
    }
} 