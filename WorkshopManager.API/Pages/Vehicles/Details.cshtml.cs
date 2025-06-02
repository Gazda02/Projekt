using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Pages.Vehicles;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public Vehicle Vehicle { get; set; } = default!;

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Customer)
            .Include(v => v.ServiceOrders)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle == null)
        {
            return NotFound();
        }

        Vehicle = vehicle;
        return Page();
    }
} 