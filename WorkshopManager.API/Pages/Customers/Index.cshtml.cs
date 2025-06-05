using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

public class CustomersIndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public CustomersIndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    public IList<Customer> Customers { get; set; } = new List<Customer>();

    public async Task OnGetAsync()
    {
        if (!Request.Cookies.ContainsKey("jwt_token"))
        {
            Response.Redirect("/Login");
            return;
        }
        Customers = await _context.Customers.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToListAsync();
    }

    public async Task<IActionResult> OnPostDeleteAsync(int id)
    {
        var customer = await _context.Customers.FindAsync(id);
        if (customer != null)
        {
            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage();
    }
} 