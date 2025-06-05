using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

public class CreateUserModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public CreateUserModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();
    public SelectList RoleList { get; set; }
    public string ErrorMessage { get; set; }

    public class InputModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
    }

    public void OnGet()
    {
        if (!Request.Cookies.ContainsKey("jwt_token"))
        {
            Response.Redirect("/Login");
            return;
        }
        RoleList = new SelectList(_roleManager.Roles.Select(r => r.Name).ToList());
    }

    public async Task<IActionResult> OnPostAsync()
    {
        RoleList = new SelectList(_roleManager.Roles.Select(r => r.Name).ToList());
        if (!ModelState.IsValid)
        {
            return Page();
        }
        var user = new IdentityUser { UserName = Input.Email, Email = Input.Email };
        var result = await _userManager.CreateAsync(user, Input.Password);
        if (!result.Succeeded)
        {
            ErrorMessage = string.Join(", ", result.Errors.Select(e => e.Description));
            return Page();
        }
        await _userManager.AddToRoleAsync(user, Input.Role);
        return RedirectToPage("Index");
    }
} 