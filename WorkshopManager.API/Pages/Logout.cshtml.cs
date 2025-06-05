using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class LogoutModel : PageModel
{
    public IActionResult OnGet()
    {
        Response.Cookies.Delete("jwt_token");
        return RedirectToPage("/Index");
    }
} 