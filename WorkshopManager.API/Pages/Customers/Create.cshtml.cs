using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

public class CreateCustomerModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public CreateCustomerModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Customer Customer { get; set; } = new();

    public void OnGet()
    {
        if (!Request.Cookies.ContainsKey("jwt_token"))
        {
            Response.Redirect("/Login");
            return;
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }
        _context.Customers.Add(Customer);
        await _context.SaveChangesAsync();
        return RedirectToPage("Index");
    }
} 