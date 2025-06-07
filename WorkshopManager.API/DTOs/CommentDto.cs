using WorkshopManager.API.Models;

namespace WorkshopManager.API.DTOs;

public class CommentDto
{
    public int Id { get; set; }
    public string Content { get; set; }
    public string AuthorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public int ServiceOrderId { get; set; }
    public ServiceOrder ServiceOrder { get; set; }
}