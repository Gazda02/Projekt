using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;
using Microsoft.AspNetCore.Identity;

namespace WorkshopManager.API.Pages.ServiceOrders
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public EditModel(ApplicationDbContext context, UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [BindProperty]
        public ServiceOrder ServiceOrder { get; set; } = default!;

        [BindProperty]
        public string Content { get; set; }

        [BindProperty]
        public List<ServiceTask> Tasks { get; set; }
        [BindProperty]
        public string NewTaskDescription { get; set; }
        [BindProperty]
        public decimal NewTaskLaborCost { get; set; }

        // Jeśli chcemy pozwolić na zmianę pojazdu, przygotowujemy SelectList
        public SelectList? VehicleList { get; set; }
        public SelectList? MechanicList { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Pobierz zlecenie wraz z powiązanym pojazdem (oraz klientem), zadaniami i komentarzami
            ServiceOrder = await _context.ServiceOrders
                .Include(so => so.Vehicle)
                    .ThenInclude(v => v.Customer)
                .Include(so => so.Tasks)
                .Include(so => so.Comments)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (ServiceOrder == null)
            {
                return NotFound();
            }

            // Synchronizuj Tasks do bindowania
            Tasks = ServiceOrder.Tasks.OrderBy(t => t.Id).ToList();

            // Przygotowanie listy pojazdów do wyboru
            var vehicles = await _context.Vehicles
                .Include(v => v.Customer)
                .OrderBy(v => v.Customer.LastName)
                .ThenBy(v => v.Customer.FirstName)
                .Select(v => new
                {
                    v.Id,
                    DisplayName = $"{v.Year} {v.Make} {v.Model} - {(v.Customer != null ? $"{v.Customer.LastName}, {v.Customer.FirstName}" : "Brak klienta")}"
                })
                .ToListAsync();
            VehicleList = new SelectList(vehicles, "Id", "DisplayName", ServiceOrder.VehicleId);

            // Przygotowanie listy mechaników
            var allUsers = _userManager.Users.ToList();
            var mechanics = new List<IdentityUser>();
            foreach (var user in allUsers)
            {
                var roles = await _userManager.GetRolesAsync(user);
                if (roles.Contains("Mechanic"))
                {
                    mechanics.Add(user);
                }
            }
            MechanicList = new SelectList(mechanics, "Id", "Email", ServiceOrder.AssignedMechanicId);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                // Jeśli walidacja nie przeszła, ponownie wczytujemy listę pojazdów i wracamy do strony
                var vehicles = await _context.Vehicles
                    .Include(v => v.Customer)
                    .OrderBy(v => v.Customer.LastName)
                    .ThenBy(v => v.Customer.FirstName)
                    .Select(v => new
                    {
                        v.Id,
                        DisplayName = $"{v.Year} {v.Make} {v.Model} - {(v.Customer != null ? $"{v.Customer.LastName}, {v.Customer.FirstName}" : "Brak klienta")}"
                    })
                    .ToListAsync();
                VehicleList = new SelectList(vehicles, "Id", "DisplayName", ServiceOrder.VehicleId);

                // Przygotowanie listy mechaników
                var allUsers = _userManager.Users.ToList();
                var mechanics = new List<IdentityUser>();
                foreach (var user in allUsers)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    if (roles.Contains("Mechanic"))
                    {
                        mechanics.Add(user);
                    }
                }
                MechanicList = new SelectList(mechanics, "Id", "Email", ServiceOrder.AssignedMechanicId);

                return Page();
            }

            // Załaduj istniejący encję z bazy (żeby EF Core śledził zmiany)
            var existingOrder = await _context.ServiceOrders.FindAsync(ServiceOrder.Id);
            if (existingOrder == null)
            {
                return NotFound();
            }

            // Zaktualizuj pola, które pozwalamy edytować
            existingOrder.Description = ServiceOrder.Description;
            existingOrder.VehicleId = ServiceOrder.VehicleId;
            existingOrder.Status = ServiceOrder.Status;
            existingOrder.AssignedMechanicId = ServiceOrder.AssignedMechanicId;
            // (opcjonalnie: jeśli chcemy edytować CompletedAt lub IsCompleted)

            // Jeśli chcemy aktualizować tylko główne pola zlecenia, nie musimy ingerować w ServiceOrder.Tasks
            // Jeśli planujemy też edycję zadań, trzeba by dodatkowo synchronizować listę ServiceOrder.Tasks

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ServiceOrderExists(ServiceOrder.Id))
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

        public async Task<IActionResult> OnPostAddCommentAsync(int orderId)
        {
            if (string.IsNullOrWhiteSpace(Content))
            {
                // Optionally, add a ModelState error and reload the page
                return await OnGetAsync(orderId);
            }

            var order = await _context.ServiceOrders
                .Include(o => o.Comments)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
            {
                return NotFound();
            }

            var user = await _userManager.GetUserAsync(User);
            var authorId = user?.Email ?? "Anonim";

            var comment = new Comment
            {
                Content = Content,
                AuthorId = authorId,
                CreatedAt = DateTime.UtcNow,
                ServiceOrderId = orderId
            };
            order.Comments.Add(comment);
            await _context.SaveChangesAsync();

            // Clear the textarea after submit
            Content = string.Empty;

            return RedirectToPage(new { id = orderId });
        }

        public async Task<IActionResult> OnPostEditTasksAsync(int orderId)
        {
            var order = await _context.ServiceOrders
                .Include(o => o.Tasks)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                return NotFound();

            // Aktualizuj istniejące taski
            foreach (var postedTask in Tasks)
            {
                var dbTask = order.Tasks.FirstOrDefault(t => t.Id == postedTask.Id);
                if (dbTask != null)
                {
                    dbTask.Description = postedTask.Description;
                    dbTask.LaborCost = postedTask.LaborCost;
                    dbTask.IsCompleted = postedTask.IsCompleted;
                }
            }
            await _context.SaveChangesAsync();
            return RedirectToPage(new { id = orderId });
        }

        public async Task<IActionResult> OnPostAddTaskAsync(int orderId)
        {
            if (string.IsNullOrWhiteSpace(NewTaskDescription))
                return await OnGetAsync(orderId);
            var order = await _context.ServiceOrders
                .Include(o => o.Tasks)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                return NotFound();
            var newTask = new ServiceTask
            {
                Description = NewTaskDescription,
                LaborCost = NewTaskLaborCost,
                ServiceOrderId = orderId,
                IsCompleted = false
            };
            order.Tasks.Add(newTask);
            await _context.SaveChangesAsync();
            NewTaskDescription = string.Empty;
            NewTaskLaborCost = 0;
            return RedirectToPage(new { id = orderId });
        }

        public async Task<IActionResult> OnPostDeleteTaskAsync(int orderId, int taskId)
        {
            var order = await _context.ServiceOrders
                .Include(o => o.Tasks)
                .FirstOrDefaultAsync(o => o.Id == orderId);
            if (order == null)
                return NotFound();
            var task = order.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                _context.ServiceTasks.Remove(task);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage(new { id = orderId });
        }

        private bool ServiceOrderExists(int id)
        {
            return _context.ServiceOrders.Any(e => e.Id == id);
        }
    }
}
