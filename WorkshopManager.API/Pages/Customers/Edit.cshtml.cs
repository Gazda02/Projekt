using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

public class EditCustomerModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public EditCustomerModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Customer Customer { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(int id)
    {
        if (!Request.Cookies.ContainsKey("jwt_token"))
        {
            Response.Redirect("/Login");
            return Page();
        }
        Customer = await _context.Customers.FindAsync(id);
        if (Customer == null)
        {
            return RedirectToPage("Index");
        }
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        _context.Attach(Customer).State = EntityState.Modified;
        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await _context.Customers.AnyAsync(e => e.Id == Customer.Id))
            {
                return RedirectToPage("Index");
            }
            else
            {
                throw;
            }
        }
        return RedirectToPage("Index");
    }
} 