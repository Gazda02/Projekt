using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Pages.ServiceOrders;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<ServiceOrder> ServiceOrders { get; set; } = new List<ServiceOrder>();

    public async Task OnGetAsync()
    {
        ServiceOrders = await _context.ServiceOrders
            .Include(so => so.Vehicle)
                .ThenInclude(v => v.Customer)
            .Include(so => so.Tasks)
            .OrderByDescending(so => so.CreatedAt)
            .ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var serviceOrder = await _context.ServiceOrders.FindAsync(id);

        if (serviceOrder != null)
        {
            _context.ServiceOrders.Remove(serviceOrder);
            await _context.SaveChangesAsync();
        }

        return RedirectToPage();
    }
} 