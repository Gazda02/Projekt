using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Pages.Vehicles;

public class EditModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public EditModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public Vehicle Vehicle { get; set; } = default!;

    public SelectList? CustomerList { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Customer)
            .FirstOrDefaultAsync(v => v.Id == id);

        if (vehicle == null)
        {
            return NotFound();
        }

        Vehicle = vehicle;

        var customers = await _context.Customers
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Select(c => new
            {
                Id = c.Id,
                DisplayName = $"{c.LastName}, {c.FirstName} ({c.Email})"
            })
            .ToListAsync();

        CustomerList = new SelectList(customers, "Id", "DisplayName", Vehicle.CustomerId);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadCustomerList();
            return Page();
        }

        // Check if VIN is unique (excluding current vehicle)
        if (await _context.Vehicles.AnyAsync(v => v.VIN == Vehicle.VIN && v.Id != Vehicle.Id))
        {
            ModelState.AddModelError("Vehicle.VIN", "This VIN is already registered in the system.");
            await LoadCustomerList();
            return Page();
        }

        // Check if registration number is unique (excluding current vehicle)
        if (await _context.Vehicles.AnyAsync(v => v.RegistrationNumber == Vehicle.RegistrationNumber && v.Id != Vehicle.Id))
        {
            ModelState.AddModelError("Vehicle.RegistrationNumber", "This registration number is already registered in the system.");
            await LoadCustomerList();
            return Page();
        }

        _context.Attach(Vehicle).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await VehicleExists(Vehicle.Id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return RedirectToPage("./Index");
    }

    private async Task LoadCustomerList()
    {
        var customers = await _context.Customers
            .OrderBy(c => c.LastName)
            .ThenBy(c => c.FirstName)
            .Select(c => new
            {
                Id = c.Id,
                DisplayName = $"{c.LastName}, {c.FirstName} ({c.Email})"
            })
            .ToListAsync();

        CustomerList = new SelectList(customers, "Id", "DisplayName", Vehicle.CustomerId);
    }

    private async Task<bool> VehicleExists(int id)
    {
        return await _context.Vehicles.AnyAsync(v => v.Id == id);
    }
} 