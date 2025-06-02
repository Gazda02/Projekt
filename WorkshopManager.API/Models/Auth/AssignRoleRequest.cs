using System.ComponentModel.DataAnnotations;

namespace WorkshopManager.API.Models.Auth;

public class AssignRoleRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [RegularExpression("^(Admin|Mechanic|Receptionist)$", ErrorMessage = "Invalid role. Must be Admin, Mechanic, or Receptionist.")]
    public string Role { get; set; }
} 