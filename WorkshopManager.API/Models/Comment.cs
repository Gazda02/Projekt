using System.ComponentModel.DataAnnotations;

namespace WorkshopManager.API.Models;

public class Comment
{
    public int Id { get; set; }

    [Required]
    public string Content { get; set; }

    [Required]
    public string AuthorId { get; set; }

    public DateTime CreatedAt { get; set; }

    // Foreign key
    public int ServiceOrderId { get; set; }

    // Navigation property
    public ServiceOrder ServiceOrder { get; set; }
} 