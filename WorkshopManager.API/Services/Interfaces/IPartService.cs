using WorkshopManager.API.Models;

namespace WorkshopManager.API.Services.Interfaces;

public interface IPartService
{
    Task<IEnumerable<Part>> GetAllPartsAsync();
    Task<Part> GetPartByIdAsync(int id);
    Task<IEnumerable<Part>> SearchPartsAsync(string query);
    Task<Part> CreatePartAsync(Part part);
    Task<Part> UpdatePartAsync(Part part);
    Task DeletePartAsync(int id);
    Task<IEnumerable<Part>> GetLowStockPartsAsync();
    Task AdjustStockAsync(int id, int quantity);
} 