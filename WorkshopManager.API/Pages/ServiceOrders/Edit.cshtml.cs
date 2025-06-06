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

        // Jeśli chcemy pozwolić na zmianę pojazdu, przygotowujemy SelectList
        public SelectList? VehicleList { get; set; }
        public SelectList? MechanicList { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Pobierz zlecenie wraz z powiązanym pojazdem (oraz klientem) oraz zadaniami
            ServiceOrder = await _context.ServiceOrders
                .Include(so => so.Vehicle)
                    .ThenInclude(v => v.Customer)
                .Include(so => so.Tasks)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (ServiceOrder == null)
            {
                return NotFound();
            }

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

        private bool ServiceOrderExists(int id)
        {
            return _context.ServiceOrders.Any(e => e.Id == id);
        }
    }
}
