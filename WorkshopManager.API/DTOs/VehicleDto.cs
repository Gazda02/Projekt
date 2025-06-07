using WorkshopManager.API.Models;

namespace WorkshopManager.API.DTOs;

public class VehicleDto
{
    public int Id { get; set; }
    public string VIN { get; set; } = string.Empty;
    public string RegistrationNumber { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int Year { get; set; }
    public string? ImageUrl { get; set; }
    public int CustomerId { get; set; }
    public Customer? Customer { get; set; }
    public ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
    public string DisplayName => $"{Year} {Make} {Model} - {Customer?.FirstName} {Customer?.LastName}";

}