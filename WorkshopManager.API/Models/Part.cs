using System.ComponentModel.DataAnnotations;

namespace WorkshopManager.API.Models;

public class Part
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [Range(0, 100000)]
    public decimal UnitPrice { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    [StringLength(50)]
    public string? PartNumber { get; set; }

    public int? StockQuantity { get; set; }

    // Navigation property
    public ICollection<UsedPart> UsedInTasks { get; set; } = new List<UsedPart>();
} 