using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;
using WorkshopManager.API.Services.Interfaces;

namespace WorkshopManager.API.Services;

public class PartService : IPartService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PartService> _logger;
    private const int LowStockThreshold = 5;

    public PartService(ApplicationDbContext context, ILogger<PartService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Part>> GetAllPartsAsync()
    {
        return await _context.Parts.ToListAsync();
    }

    public async Task<Part> GetPartByIdAsync(int id)
    {
        var part = await _context.Parts.FindAsync(id);
        if (part == null)
        {
            _logger.LogWarning("Part with ID {Id} not found", id);
            throw new KeyNotFoundException($"Part with ID {id} not found");
        }
        return part;
    }

    public async Task<Part> CreatePartAsync(Part part)
    {
        _context.Parts.Add(part);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created new part with ID {Id}", part.Id);
        return part;
    }

    public async Task<Part> UpdatePartAsync(Part part)
    {
        var existingPart = await GetPartByIdAsync(part.Id);
        
        _context.Entry(existingPart).CurrentValues.SetValues(part);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Updated part with ID {Id}", part.Id);
        return part;
    }

    public async Task DeletePartAsync(int id)
    {
        var part = await GetPartByIdAsync(id);
        
        _context.Parts.Remove(part);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Deleted part with ID {Id}", id);
    }

    public async Task<IEnumerable<Part>> SearchPartsAsync(string query)
    {
        return await _context.Parts
            .Where(p => p.Name.Contains(query) || p.Description.Contains(query))
            .ToListAsync();
    }

    public async Task<IEnumerable<Part>> GetLowStockPartsAsync()
    {
        return await _context.Parts
            .Where(p => p.StockQuantity <= LowStockThreshold)
            .ToListAsync();
    }

    public async Task AdjustStockAsync(int id, int quantity)
    {
        var part = await GetPartByIdAsync(id);
        
        part.StockQuantity += quantity;
        if (part.StockQuantity < 0)
        {
            throw new InvalidOperationException("Stock quantity cannot be negative");
        }
        
        await _context.SaveChangesAsync();
        _logger.LogInformation("Adjusted stock for part {Id} by {Quantity}", id, quantity);
    }

    public async Task<Part> UpdateStockQuantityAsync(int partId, int quantity)
    {
        try
        {
            var part = await _context.Parts.FindAsync(partId);
            if (part == null)
                throw new KeyNotFoundException($"Part with ID {partId} not found");

            part.StockQuantity = quantity;
            await _context.SaveChangesAsync();
            return part;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating stock quantity for part ID: {Id}", partId);
            throw;
        }
    }

    public async Task<bool> IsPartAvailableAsync(int partId, int requestedQuantity)
    {
        try
        {
            var part = await _context.Parts.FindAsync(partId);
            if (part == null)
                throw new KeyNotFoundException($"Part with ID {partId} not found");

            return part.StockQuantity >= requestedQuantity;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while checking availability for part ID: {Id}", partId);
            throw;
        }
    }
} 