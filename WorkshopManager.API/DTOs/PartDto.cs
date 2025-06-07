using WorkshopManager.API.Models;

namespace WorkshopManager.API.DTOs;

public class PartDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal UnitPrice { get; set; }
    public string? Description { get; set; }
    public string? PartNumber { get; set; }
    public int? StockQuantity { get; set; }
    public ICollection<UsedPart> UsedInTasks { get; set; } = new List<UsedPart>();

}