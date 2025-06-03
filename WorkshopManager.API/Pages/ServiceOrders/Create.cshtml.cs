using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Pages.ServiceOrders;

public class CreateModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public CreateModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public ServiceOrder ServiceOrder { get; set; } = new();

    [BindProperty]
    public List<ServiceTask> Tasks { get; set; } = new();

    public SelectList? VehicleList { get; set; }

    public async Task OnGetAsync(int? vehicleId = null)
    {
        // Get vehicles with customer information for the dropdown
        var vehicles = await _context.Vehicles
            .Include(v => v.Customer)
            .OrderBy(v => v.Customer.LastName)
            .ThenBy(v => v.Customer.FirstName)
            .Select(v => new
            {
                Id = v.Id,
                DisplayName = $"{v.Year} {v.Make} {v.Model} - {(v.Customer != null ? $"{v.Customer.LastName}, {v.Customer.FirstName}" : "No Customer")}"
            })
            .ToListAsync();

        VehicleList = new SelectList(vehicles, "Id", "DisplayName");

        if (vehicleId.HasValue)
        {
            ServiceOrder.VehicleId = vehicleId.Value;
        }

        // Initialize with one empty task
        if (!Tasks.Any())
        {
            Tasks.Add(new ServiceTask());
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await OnGetAsync();
            return Page();
        }

        ServiceOrder.CreatedAt = DateTime.UtcNow;
        ServiceOrder.Status = OrderStatus.Created;

        // Filter out empty tasks and set their service order
        var validTasks = Tasks.Where(t => !string.IsNullOrWhiteSpace(t.Description)).ToList();
        foreach (var task in validTasks)
        {
            task.ServiceOrder = ServiceOrder;
        }
        ServiceOrder.Tasks = validTasks;

        _context.ServiceOrders.Add(ServiceOrder);
        
        Console.WriteLine("Attempting to save order:");
        Console.WriteLine($"VehicleId: {ServiceOrder.VehicleId}");
        Console.WriteLine($"Tasks count: {ServiceOrder.Tasks?.Count}");
        await _context.SaveChangesAsync();
        return RedirectToPage("./Index");
    }
} 