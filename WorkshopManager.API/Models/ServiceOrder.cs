using System.ComponentModel.DataAnnotations;

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
    public Vehicle Vehicle { get; set; }
    public List<ServiceTask> Tasks { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();

    // Total calculations
    public decimal TotalLaborCost => Tasks.Sum(t => t.LaborCost);
    public decimal TotalPartsCost => Tasks.Sum(t => t.UsedParts.Sum(p => p.Quantity * p.Part.UnitPrice));
    public decimal TotalCost => Tasks?.Sum(t => t.LaborCost + t.UsedParts.Sum(up => up.Part.UnitPrice * up.Quantity)) ?? 0;
} 