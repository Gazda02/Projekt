using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WorkshopManager.API.Models;

public class ServiceTask
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Description { get; set; } = string.Empty;

    [Required]
    [Range(0, 10000)]
    public decimal LaborCost { get; set; }

    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? AssignedMechanicId { get; set; }

    // Foreign key
    public int ServiceOrderId { get; set; }

    // Navigation properties
    [ValidateNever]
    public ServiceOrder ServiceOrder { get; set; }
    public List<UsedPart> UsedParts { get; set; } = new();
} 