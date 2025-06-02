using System.ComponentModel.DataAnnotations;

namespace WorkshopManager.API.Models.Auth;

public class RegisterRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Password { get; set; }

    [Required]
    [Compare("Password")]
    public string ConfirmPassword { get; set; }

    [Required]
    [RegularExpression("^(Admin|Mechanic|Receptionist)$", ErrorMessage = "Invalid role. Must be Admin, Mechanic, or Receptionist.")]
    public string Role { get; set; }
} 