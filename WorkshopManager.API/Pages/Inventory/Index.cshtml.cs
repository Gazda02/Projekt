using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components.Forms;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

public class InventoryIndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public InventoryIndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Part> Parts { get; set; } = new List<Part>();
    public string UserRole { get; set; }

    public async Task OnGetAsync(string? search)
    {
        if (!Request.Cookies.ContainsKey("jwt_token"))
        {
            Response.Redirect("/Login");
            return;
        }
        var jwtCookie = Request.Cookies["jwt_token"];
        if (!string.IsNullOrEmpty(jwtCookie))
        {
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(jwtCookie);
            UserRole = jwt.Claims.FirstOrDefault(c => c.Type == "role" || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value;
        }
        var query = _context.Parts.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => p.Name.Contains(search) || p.PartNumber.Contains(search));
        }
        Parts = await query.OrderBy(p => p.Name).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
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
        var part = await _context.Parts.FindAsync(id);
        if (part != null)
        {
            _context.Parts.Remove(part);
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return Forbid();
            }
            
        }
        return RedirectToPage();
    }
} 