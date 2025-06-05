using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;
using System.IdentityModel.Tokens.Jwt;

public class EditServiceOrderModel : PageModel
{
    private readonly ApplicationDbContext _context;
    public EditServiceOrderModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public ServiceOrder ServiceOrder { get; set; } = new();
    [BindProperty]
    public List<ServiceTask> Tasks { get; set; } = new();
    [BindProperty]
    public string NewComment { get; set; }
    public List<Comment> Comments { get; set; } = new();
    public SelectList VehicleList { get; set; }

    public async Task<IActionResult> OnGetAsync(int id)
    {
        ServiceOrder = await _context.ServiceOrders
            .Include(o => o.Tasks)
            .FirstOrDefaultAsync(o => o.Id == id);
        if (ServiceOrder == null)
        {
            return RedirectToPage("Index");
        }
        Tasks = ServiceOrder.Tasks?.ToList() ?? new List<ServiceTask>();
        Comments = await _context.Set<Comment>().Where(c => c.ServiceOrderId == id).OrderByDescending(c => c.CreatedAt).ToListAsync();
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
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
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
            return Page();
        }
        var order = await _context.ServiceOrders
            .Include(o => o.Tasks)
            .FirstOrDefaultAsync(o => o.Id == ServiceOrder.Id);
        if (order == null)
        {
            return RedirectToPage("Index");
        }
        order.VehicleId = ServiceOrder.VehicleId;
        order.Status = ServiceOrder.Status;
        order.Description = ServiceOrder.Description;
        // Aktualizuj zadania
        order.Tasks.Clear();
        foreach (var task in Tasks)
        {
            if (!string.IsNullOrWhiteSpace(task.Description))
            {
                order.Tasks.Add(new ServiceTask { Description = task.Description, ServiceOrderId = order.Id });
            }
        }
        await _context.SaveChangesAsync();
        return RedirectToPage("Index");
    }

    public async Task<IActionResult> OnPostAddCommentAsync(int id)
    {
        ServiceOrder = await _context.ServiceOrders.Include(o => o.Tasks).FirstOrDefaultAsync(o => o.Id == id);
        if (ServiceOrder == null)
        {
            return RedirectToPage("Index");
        }
        if (!string.IsNullOrWhiteSpace(NewComment))
        {
            var jwtCookie = Request.Cookies["jwt_token"];
            string authorId = "Unknown";
            if (!string.IsNullOrEmpty(jwtCookie))
            {
                var handler = new JwtSecurityTokenHandler();
                var jwt = handler.ReadJwtToken(jwtCookie);
                authorId = jwt.Claims.FirstOrDefault(c => c.Type == "email" || c.Type == "unique_name" || c.Type == "name")?.Value ?? "Unknown";
            }
            var comment = new Comment
            {
                Content = NewComment,
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow,
                ServiceOrderId = id
            };
            _context.Add(comment);
            await _context.SaveChangesAsync();
        }
        return RedirectToPage(new { id });
    }
} 