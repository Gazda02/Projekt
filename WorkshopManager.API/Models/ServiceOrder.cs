using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace WorkshopManager.API.Models;

public class ServiceOrder
{
    public int Id { get; set; }
    
    [Required]
    public string Description { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    public string? AssignedMechanicId { get; set; }
    
    // Foreign keys
    public int VehicleId { get; set; }
    
    // Navigation properties
    [ValidateNever]
    public Vehicle Vehicle { get; set; }
    
    [BindProperty]
    public List<ServiceTask> Tasks { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
    public ICollection<UsedPart> UsedParts { get; set; } = new List<UsedPart>();


    public decimal TotalLaborCost => Tasks.Sum(t => t.LaborCost);
    public decimal TotalPartsCost => Tasks.Sum(t => t.UsedParts.Sum(p => p.Quantity * p.Part.UnitPrice));
    public decimal TotalCost => Tasks?.Sum(t => t.LaborCost + t.UsedParts.Sum(up => up.Part.UnitPrice * up.Quantity)) ?? 0;
} 