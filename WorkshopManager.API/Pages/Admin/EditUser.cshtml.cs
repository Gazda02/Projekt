using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;

public class EditUserModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public EditUserModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public string Email { get; set; }
    [BindProperty]
    public string SelectedRole { get; set; }
    public SelectList RoleList { get; set; }
    public string ErrorMessage { get; set; }
    private IdentityUser _user;
    [BindProperty]
    public string NewPassword { get; set; }

    public async Task<IActionResult> OnGetAsync(string id)
    {
        if (!Request.Cookies.ContainsKey("jwt_token"))
        {
            Response.Redirect("/Login");
            return Page();
        }
        _user = await _userManager.FindByIdAsync(id);
        if (_user == null)
        {
            return RedirectToPage("Index");
        }
        Email = _user.Email;
        var roles = await _userManager.GetRolesAsync(_user);
        SelectedRole = roles.FirstOrDefault();
        RoleList = new SelectList(_roleManager.Roles.Select(r => r.Name).ToList());
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(string id)
    {
        _user = await _userManager.FindByIdAsync(id);
        if (_user == null)
        {
            return RedirectToPage("Index");
        }
        var currentRoles = await _userManager.GetRolesAsync(_user);
        var removeResult = await _userManager.RemoveFromRolesAsync(_user, currentRoles);
        if (!removeResult.Succeeded)
        {
            ErrorMessage = string.Join(", ", removeResult.Errors.Select(e => e.Description));
            RoleList = new SelectList(_roleManager.Roles.Select(r => r.Name).ToList());
            Email = _user.Email;
            return Page();
        }
        var addResult = await _userManager.AddToRoleAsync(_user, SelectedRole);
        if (!addResult.Succeeded)
        {
            ErrorMessage = string.Join(", ", addResult.Errors.Select(e => e.Description));
            RoleList = new SelectList(_roleManager.Roles.Select(r => r.Name).ToList());
            Email = _user.Email;
            return Page();
        }
        if (!string.IsNullOrWhiteSpace(NewPassword))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(_user);
            var passResult = await _userManager.ResetPasswordAsync(_user, token, NewPassword);
            if (!passResult.Succeeded)
            {
                ErrorMessage = string.Join(", ", passResult.Errors.Select(e => e.Description));
                RoleList = new SelectList(_roleManager.Roles.Select(r => r.Name).ToList());
                Email = _user.Email;
                return Page();
            }
        }
        return RedirectToPage("Index");
    }
} 