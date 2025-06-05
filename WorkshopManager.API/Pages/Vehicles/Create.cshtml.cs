using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Pages.Vehicles;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Vehicle Vehicle { get; set; } = new();

    public SelectList? CustomerList { get; set; }

    public async Task OnGetAsync()
    {
        if (!Request.Cookies.ContainsKey("jwt_token"))
        {
            Response.Redirect("/Login");
            return;
        }

        var customers = await _context.Customers
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Select(c => new
            {
                Id = c.Id,
                DisplayName = $"{c.LastName}, {c.FirstName} ({c.Email})"
            })
            .ToListAsync();

        CustomerList = new SelectList(customers, "Id", "DisplayName");
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        // Check if VIN is unique
        if (await _context.Vehicles.AnyAsync(v => v.VIN == Vehicle.VIN))
        {
            ModelState.AddModelError("Vehicle.VIN", "This VIN is already registered in the system.");
            await OnGetAsync();
            return Page();
        }

        // Check if registration number is unique
        if (await _context.Vehicles.AnyAsync(v => v.RegistrationNumber == Vehicle.RegistrationNumber))
        {
            ModelState.AddModelError("Vehicle.RegistrationNumber", "This registration number is already registered in the system.");
            await OnGetAsync();
            return Page();
        }

        _context.Vehicles.Add(Vehicle);
        await _context.SaveChangesAsync();

        return RedirectToPage("./Index");
    }
} 