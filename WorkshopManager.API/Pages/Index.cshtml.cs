using Microsoft.AspNetCore.Mvc.RazorPages;

namespace WorkshopManager.API.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ILogger<IndexModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        if (!Request.Cookies.ContainsKey("jwt_token"))
        {
            Response.Redirect("/Login");
            return;
        }
        _logger.LogInformation("Dashboard page accessed at: {Time}", DateTime.UtcNow);
    }
} 