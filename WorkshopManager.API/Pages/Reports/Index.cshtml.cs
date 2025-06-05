using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Rotativa.AspNetCore;
using WorkshopManager.API.Data;
using WorkshopManager.API.Models;

namespace WorkshopManager.API.Pages.Reports
{
    public class ClientReportPdfViewModel
    {
        public List<Customer> Clients { get; set; } = new();
        public int SelectedCustomerId { get; set; }
        public List<ServiceOrder> OrdersForClient { get; set; } = new();
        public decimal TotalCostSum { get; set; }
    }

    public class ClientReportModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public ClientReportModel(ApplicationDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lista klientów wyświetlana w dropdownie, aby móc wybrać klienta, dla którego generujemy raport.
        /// </summary>
        public List<Customer> Clients { get; set; } = new();

        /// <summary>
        /// Wybrany klient (po kliknięciu „Generuj raport”).
        /// </summary>
        [BindProperty]
        public int SelectedCustomerId { get; set; }

        /// <summary>
        /// Wszystkie ServiceOrder dla wybranego klienta (załadowane wraz z pojazdami).
        /// </summary>
        public List<ServiceOrder> OrdersForClient { get; set; } = new();

        /// <summary>
        /// Łączny koszt (+VAT, jeśli to potrzebne) dla wszystkich zleceń.
        /// </summary>
        public decimal TotalCostSum { get; set; }

        /// <summary>
        /// Wywoływane na GET – ładujemy listę klientów do dropdownu.
        /// </summary>
        public async Task OnGetAsync()
        {
            Clients = await _context.Customers
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();
        }

        /// <summary>
        /// Po kliknięciu przycisku „Generuj raport (HTML)” – pobieramy zlecenia i obliczamy sumę.
        /// </summary>
        public async Task<IActionResult> OnPostGenerateAsync()
        {
            // 1. Walidacja: czy wybrano klienta?
            if (SelectedCustomerId == 0)
            {
                ModelState.AddModelError(nameof(SelectedCustomerId), "Proszę wybrać klienta.");
                Clients = await _context.Customers
                    .OrderBy(c => c.LastName)
                    .ThenBy(c => c.FirstName)
                    .ToListAsync();
                return Page();
            }

            // 2. Pobranie zleceń serwisowych dla wskazanego klienta
            OrdersForClient = await _context.ServiceOrders
                .Include(so => so.Vehicle)
                .ThenInclude(v => v.Customer)
                .Include(so => so.Tasks)
                .ThenInclude(t => t.UsedParts)
                .ThenInclude(up => up.Part)
                .Where(so => so.Vehicle.CustomerId == SelectedCustomerId)
                .OrderByDescending(so => so.CreatedAt)
                .ToListAsync();

            // 3. Obliczenie sumy łącznej (uwzględniamy po prostu TotalCost z tabeli)
            TotalCostSum = OrdersForClient.Sum(o => o.TotalCost);

            // 4. Przygotuj listę klientów ponownie (żeby dropdown nie był pusty po postback)
            Clients = await _context.Customers
                .OrderBy(c => c.LastName)
                .ThenBy(c => c.FirstName)
                .ToListAsync();

            return Page(); // Renderujemy ten sam widok, z wypełnionymi OrdersForClient i TotalCostSum
        }

        /// <summary>
        /// Metoda, która zwraca PDF (widok z tymi samymi danymi co w HTML).
        /// Rotativa wygeneruje PDF z widoku ClientReportPdf.cshtml.
        /// </summary>
        public async Task<IActionResult> OnPostGeneratePdfAsync()
        {
            if (SelectedCustomerId == 0)
            {
                ModelState.AddModelError(nameof(SelectedCustomerId), "Proszę wybrać klienta.");
                Clients = await _context.Customers.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToListAsync();
                return Page();
            }

            var orders = await _context.ServiceOrders
                .Include(so => so.Vehicle)
                .ThenInclude(v => v.Customer)
                .Include(so => so.Tasks)
                .ThenInclude(t => t.UsedParts)
                .ThenInclude(up => up.Part)
                .Where(so => so.Vehicle.CustomerId == SelectedCustomerId)
                .OrderByDescending(so => so.CreatedAt)
                .ToListAsync();

            var total = orders.Sum(o => o.TotalCost);

            var clients = await _context.Customers.OrderBy(c => c.LastName).ThenBy(c => c.FirstName).ToListAsync();

            var viewModel = new ClientReportPdfViewModel
            {
                Clients = clients,
                SelectedCustomerId = SelectedCustomerId,
                OrdersForClient = orders,
                TotalCostSum = total
            };

            return new ViewAsPdf("Reports/ClientReportPdf", viewModel)
            {
                FileName = $"Raport_Klienta_{SelectedCustomerId}.pdf",
                PageSize = Rotativa.AspNetCore.Options.Size.A4,
                PageOrientation = Rotativa.AspNetCore.Options.Orientation.Portrait,
                CustomSwitches = "--disable-smart-shrinking"
            };
        }
    }
}
