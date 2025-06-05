using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Pages.Vehicles;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

    public async Task OnGetAsync(string? search = null)
    {
        if (!Request.Cookies.ContainsKey("jwt_token"))
        {
            Response.Redirect("/Login");
            return;
        }
        var query = _context.Vehicles.Include(v => v.Customer).AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(v => v.VIN.Contains(search) || v.RegistrationNumber.Contains(search));
        }
        Vehicles = await query
            .OrderBy(v => v.Customer.LastName)
            .ThenBy(v => v.Customer.FirstName)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.ServiceOrders)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle == null)
        {
            return NotFound();
        }

        if (vehicle.ServiceOrders.Any())
        {
            ModelState.AddModelError(string.Empty, "Cannot delete vehicle with existing service orders.");
            await OnGetAsync();
            return Page();
        }

        _context.Vehicles.Remove(vehicle);
        await _context.SaveChangesAsync();

        return RedirectToPage();
    }
} 