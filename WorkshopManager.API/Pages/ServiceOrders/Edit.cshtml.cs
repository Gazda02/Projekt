using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Pages.ServiceOrders
{
    public class EditModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ta właściwość pozwala nam pokazać detale zlecenia na stronie
        public ServiceOrder ServiceOrder { get; set; } = default!;

        // Lista istniejących komentarzy (ładowana w OnGet)
        public IList<Comment> Comments { get; set; } = new List<Comment>();

        // Bindowane pole wyboru nowego statusu
        [BindProperty]
        [Display(Name = "Nowy status zlecenia")]
        public OrderStatus SelectedStatus { get; set; }

        // Bindowane pole dla treści nowego komentarza
        [BindProperty]
        [Display(Name = "Treść komentarza")]
        [Required(ErrorMessage = "Treść komentarza jest wymagana.")]
        public string NewCommentContent { get; set; } = string.Empty;


        // ======================== OnGetAsync ========================
        public async Task<IActionResult> OnGetAsync(int id)
        {
            // Ładujemy zlecenie
            ServiceOrder = await _context.ServiceOrders
                .Include(so => so.Comments.OrderByDescending(c => c.CreatedAt))
                .FirstOrDefaultAsync(so => so.Id == id);

            if (ServiceOrder == null)
            {
                return NotFound();
            }

            // Przekazujemy listę komentarzy (posortowaną malejąco po dacie utworzenia)
            Comments = ServiceOrder.Comments.OrderByDescending(c => c.CreatedAt).ToList();

            // Ustawiamy wstępnie wybrany status
            SelectedStatus = ServiceOrder.Status;

            return Page();
        }


        // ======================== OnPostUpdateStatusAsync ========================
        public async Task<IActionResult> OnPostUpdateStatusAsync(int id)
        {
            // Usuwamy ewentualny błąd walidacji dla komentarza,
            // bo przy zmianie statusu pole komentarza nie jest wymagane
            ModelState.Remove(nameof(NewCommentContent));

            if (!ModelState.IsValid)
            {
                // W razie błędu ponownie ładujemy dane do widoku
                ServiceOrder = await _context.ServiceOrders
                    .Include(so => so.Comments.OrderByDescending(c => c.CreatedAt))
                    .FirstOrDefaultAsync(so => so.Id == id)
                    ?? new ServiceOrder();
                Comments = ServiceOrder.Comments.OrderByDescending(c => c.CreatedAt).ToList();
                return Page();
            }

            // Znajdujemy zlecenie po id i aktualizujemy status
            var order = await _context.ServiceOrders.FindAsync(id);
            if (order == null)
            {
                return NotFound();
            }

            order.Status = SelectedStatus;
            if (SelectedStatus == OrderStatus.Completed)
            {
                order.CompletedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Status zlecenia został zaktualizowany.";
            return RedirectToPage(new { id = id });
        }


        // ======================== OnPostAddCommentAsync ========================
        public async Task<IActionResult> OnPostAddCommentAsync(int id)
        {
            if (!ModelState.IsValid)
            {
                // Ponownie ładujemy w przypadku walidacji treści komentarza
                ServiceOrder = await _context.ServiceOrders
                    .Include(so => so.Comments.OrderByDescending(c => c.CreatedAt))
                    .FirstOrDefaultAsync(so => so.Id == id)
                    ?? new ServiceOrder();
                Comments = ServiceOrder.Comments.OrderByDescending(c => c.CreatedAt).ToList();
                return Page();
            }

            // Sprawdzamy, czy zlecenie istnieje
            var orderExists = await _context.ServiceOrders.AnyAsync(so => so.Id == id);
            if (!orderExists)
            {
                return NotFound();
            }

            // Tworzymy komentarz
            var comment = new Comment
            {
                ServiceOrderId = id,
                Content = NewCommentContent,
                AuthorId = User?.Identity?.Name ?? "Unknown",
                CreatedAt = DateTime.UtcNow
            };

            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Dodano komentarz.";
            return RedirectToPage(new { id = id });
        }
    }
}
