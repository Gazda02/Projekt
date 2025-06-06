using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

public class EditPartModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public EditPartModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Part Part { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
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
        Part = await _context.Parts.FindAsync(id);
        if (Part == null)
        {
            return RedirectToPage("Index");
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
        _context.Attach(Part).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return RedirectToPage("Index");
    }
} 