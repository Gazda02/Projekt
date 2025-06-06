using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.IdentityModel.Tokens.Jwt;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

public class CreatePartModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public CreatePartModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Part Part { get; set; } = new();

    public IActionResult OnGet()
    {
        if (!Request.Cookies.ContainsKey("jwt_token"))
        {
            return RedirectToPage("/Login");
        }
        var jwtCookie = Request.Cookies["jwt_token"];
        string role = null;
        if (!string.IsNullOrEmpty(jwtCookie))
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(jwtCookie);
            role = jwt.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
        }
        if (role != "Admin" && role != "Receptionist")
        {
            return Forbid();
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var jwtCookie = Request.Cookies["jwt_token"];
        string role = null;
        if (!string.IsNullOrEmpty(jwtCookie))
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(jwtCookie);
            role = jwt.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
        }
        if (role != "Admin" && role != "Receptionist")
        {
            return Forbid();
        }
        if (!ModelState.IsValid)
        {
            return Page();
        }
        _context.Parts.Add(Part);
        await _context.SaveChangesAsync();
        return RedirectToPage("Index");
    }
} 