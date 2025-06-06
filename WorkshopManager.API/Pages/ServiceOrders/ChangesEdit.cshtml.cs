using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Pages.ServiceOrders
{
    public class ChangesEditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ChangesEditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty(SupportsGet = true)]
        public int Id { get; set; }

        public ServiceOrder ServiceOrder { get; set; } = default!;

        public IList<ServiceTask> AllTasks { get; set; } = new List<ServiceTask>();

        public IList<Part> AllParts { get; set; } = new List<Part>();

        // Słownik: klucz = Id zadania, wartość = lista UsedPart dla tego zadania
        public Dictionary<int, List<UsedPart>> TaskParts { get; set; } = new Dictionary<int, List<UsedPart>>();

        // Nowe zadanie – bindujemy tylko przy dodawaniu
        [BindProperty]
        [Display(Name = "Opis zadania")]
        [Required(ErrorMessage = "Opis zadania jest wymagany.")]
        public string NewTaskDescription { get; set; } = string.Empty;

        [BindProperty]
        [Display(Name = "Koszt laboru")]
        [Range(0, 100000, ErrorMessage = "Koszt musi być nieujemny.")]
        public decimal NewTaskLaborCost { get; set; }

        // Dodawanie części – pola bindowane przy handlerze AddPart
        [BindProperty]
        public int TaskIdToAddPart { get; set; }

        [BindProperty]
        public int SelectedPartId { get; set; }

        [BindProperty]
        [Range(1, 1000, ErrorMessage = "Ilość musi być co najmniej 1.")]
        public int PartQuantity { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Id = id;
            ServiceOrder = await _context.ServiceOrders
                .Include(so => so.Tasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(up => up.Part)
                .FirstOrDefaultAsync(so => so.Id == id);

            if (ServiceOrder == null)
            {
                return NotFound();
            }

            AllTasks = ServiceOrder.Tasks;
            AllParts = await _context.Parts.ToListAsync();

            // Przygotuj słownik TaskParts
            foreach (var t in AllTasks)
            {
                var partsForTask = t.UsedParts.ToList();
                TaskParts[t.Id] = partsForTask;
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAddTaskAsync()
        {
            if (!ModelState.IsValid)
            {
                await ReloadDataAsync();
                return Page();
            }

            var order = await _context.ServiceOrders
                .Include(so => so.Tasks)
                .FirstOrDefaultAsync(so => so.Id == Id);

            if (order == null)
            {
                return NotFound();
            }

            var newTask = new ServiceTask
            {
                ServiceOrderId = Id,
                Description = NewTaskDescription,
                LaborCost = NewTaskLaborCost,
                IsCompleted = false
            };

            _context.ServiceTasks.Add(newTask);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Dodano nowe zadanie.";
            return RedirectToPage(new { id = Id });
        }

public async Task<IActionResult> OnPostAddPartAsync()
{
    // 1. Usuń walidację dla pól związanych z dodawaniem nowego zadania,
    //    bo przy dodawaniu części nie podajesz opisu zadania ani kosztu.
    ModelState.Remove(nameof(NewTaskDescription));
    ModelState.Remove(nameof(NewTaskLaborCost));

    // 2. Teraz ModelState sprawdza już tylko SelectedPartId, PartQuantity, TaskIdToAddPart.
    if (!ModelState.IsValid)
    {
        await ReloadDataAsync();
        return Page();
    }

    // 3. Pobierz zadanie, aby sprawdzić, czy należy do tego zlecenia
    var task = await _context.ServiceTasks
        .Include(t => t.UsedParts).ThenInclude(up => up.Part)
        .FirstOrDefaultAsync(t => t.Id == TaskIdToAddPart && t.ServiceOrderId == Id);

    if (task == null)
    {
        ModelState.AddModelError("", "Zadanie nie istnieje lub nie należy do tego zlecenia.");
        await ReloadDataAsync();
        return Page();
    }

    // 4. Pobierz część z magazynu
    var part = await _context.Parts.FindAsync(SelectedPartId);
    if (part == null)
    {
        ModelState.AddModelError("", "Wybrana część nie istnieje.");
        await ReloadDataAsync();
        return Page();
    }

    // 5. Sprawdź, czy jest wystarczająco dużo sztuk w magazynie
    if (PartQuantity > part.StockQuantity)
    {
        ModelState.AddModelError("", $"Brak wystarczającej liczby sztuk. W magazynie: {part.StockQuantity}.");
        await ReloadDataAsync();
        return Page();
    }

    // 6. Zmniejsz stan magazynowy
    part.StockQuantity -= PartQuantity;
    // part jest śledzony przez kontekst, więc wystarczy po zmianie wywołać SaveChanges

    // 7. Utwórz wpis w UsedParts
    var usedPart = new UsedPart
    {
        ServiceTaskId   = TaskIdToAddPart,
        PartId          = SelectedPartId,
        Quantity        = PartQuantity,
        ServiceOrderId  = Id
    };
    _context.UsedParts.Add(usedPart);

    // 8. Zapisz zmiany: i stan magazynu, i wpis w UsedParts w jednym SaveChanges
    await _context.SaveChangesAsync();

    TempData["SuccessMessage"] = 
        $"Dodano część '{part.Name}' (x{PartQuantity}) do zadania #{task.Id}. " +
        $"Pozostało w magazynie: {part.StockQuantity} szt.";

    return RedirectToPage(new { id = Id });
}
        
        private async Task ReloadDataAsync()
        {
            ServiceOrder = await _context.ServiceOrders
                .Include(so => so.Tasks)
                    .ThenInclude(t => t.UsedParts)
                        .ThenInclude(up => up.Part)
                .FirstOrDefaultAsync(so => so.Id == Id) ?? new ServiceOrder();

            AllTasks = ServiceOrder.Tasks;
            AllParts = await _context.Parts.ToListAsync();

            TaskParts.Clear();
            foreach (var t in AllTasks)
            {
                TaskParts[t.Id] = t.UsedParts.ToList();
            }
        }
    }
}
