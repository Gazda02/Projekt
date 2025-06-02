using System.ComponentModel.DataAnnotations;

namespace WorkshopManager.API.Models;

public class UsedPart
{
    public int Id { get; set; }

    [Required]
    [Range(1, 1000)]
    public int Quantity { get; set; }

    // Foreign keys
    public int ServiceTaskId { get; set; }
    public int PartId { get; set; }

    // Navigation properties
    public ServiceTask? ServiceTask { get; set; }
    public Part? Part { get; set; }
} 