using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

public class LoginModel : PageModel
{
    [BindProperty]
    public InputModel Input { get; set; }

    public string ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }

    public void OnGet()
    {
        ErrorMessage = null;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        using var httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5041") };
        var loginData = new { Email = Input.Email, Password = Input.Password };
        var content = new StringContent(JsonSerializer.Serialize(loginData), Encoding.UTF8, "application/json");
        try
        {
            var response = await httpClient.PostAsync("api/Auth/login", content);
            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(responseString);
                var token = doc.RootElement.GetProperty("token").GetString();
                if (!string.IsNullOrEmpty(token))
                {
                    Response.Cookies.Append("jwt_token", token, new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = DateTimeOffset.UtcNow.AddHours(3)
                    });
                }
                return RedirectToPage("/Index");
            }
            else
            {
                ErrorMessage = "Nieprawidłowy email lub hasło.";
                return Page();
            }
        }
        catch
        {
            ErrorMessage = "Błąd połączenia z serwerem.";
            return Page();
        }
    }
} 