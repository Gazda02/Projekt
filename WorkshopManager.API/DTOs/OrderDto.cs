using WorkshopManager.API.Models;

namespace WorkshopManager.API.DTOs;

public class OrderDto
{
    public int Id { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Created;
    public string? AssignedMechanicId { get; set; }
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; }
    public List<ServiceTask> Tasks { get; set; } = new();
    public List<Comment> Comments { get; set; } = new();
    public ICollection<UsedPart> UsedParts { get; set; } = new List<UsedPart>();
    public decimal TotalLaborCost => Tasks.Sum(t => t.LaborCost);
    public decimal TotalPartsCost => Tasks.Sum(t => t.UsedParts.Sum(p => p.Quantity * p.Part.UnitPrice));
    public decimal TotalCost => Tasks?.Sum(t => t.LaborCost + t.UsedParts.Sum(up => up.Part.UnitPrice * up.Quantity)) ?? 0;

}