using WorkshopManager.API.Models;

namespace WorkshopManager.API.DTOs;

public class UsedPartDto
{
    public int Id { get; set; }
    public int Quantity { get; set; }
    public int ServiceOrderId { get; set; }
    public int ServiceTaskId { get; set; }
    public int PartId { get; set; }
    public ServiceTask? ServiceTask { get; set; }
    public Part? Part { get; set; }
}