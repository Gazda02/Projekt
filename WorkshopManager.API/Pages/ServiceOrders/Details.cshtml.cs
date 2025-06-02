using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Pages.ServiceOrders;

public class DetailsModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public DetailsModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public ServiceOrder ServiceOrder { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ServiceOrder = await _context.ServiceOrders
            .Include(so => so.Vehicle)
                .ThenInclude(v => v.Customer)
            .Include(so => so.Tasks)
                .ThenInclude(t => t.UsedParts)
                    .ThenInclude(up => up.Part)
            .FirstOrDefaultAsync(so => so.Id == id);

        if (ServiceOrder == null)
        {
            return NotFound();
        }

        return Page();
    }
} 