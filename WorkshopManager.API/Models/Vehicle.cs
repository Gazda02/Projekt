using System.ComponentModel.DataAnnotations;

namespace WorkshopManager.API.Models;

public class Vehicle
{
    public int Id { get; set; }

    [Required]
    [StringLength(17)]
    public string VIN { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string RegistrationNumber { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Make { get; set; } = string.Empty;

    [Required]
    [StringLength(50)]
    public string Model { get; set; } = string.Empty;

    public int Year { get; set; }

    public string? ImageUrl { get; set; }

    // Foreign key
    public int CustomerId { get; set; }
    
    // Navigation properties
    public Customer? Customer { get; set; }
    public ICollection<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();
} 