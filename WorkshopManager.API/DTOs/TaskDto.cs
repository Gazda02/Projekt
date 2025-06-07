using WorkshopManager.API.Models;

namespace WorkshopManager.API.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public decimal LaborCost { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? AssignedMechanicId { get; set; }
    public int ServiceOrderId { get; set; }
    public ServiceOrder ServiceOrder { get; set; }
    public List<UsedPart> UsedParts { get; set; } = new();
}