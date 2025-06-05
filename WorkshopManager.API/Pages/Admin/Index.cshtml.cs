using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;

public class AdminIndexModel : PageModel
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    public AdminIndexModel(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
    {
        _userManager = userManager;
        _roleManager = roleManager;
    }

    public List<UserWithRoles> Users { get; set; } = new();

    public class UserWithRoles
    {
        public string Id { get; set; }
        public string Email { get; set; }
        public IList<string> Roles { get; set; }
    }

    public async Task OnGetAsync()
    {
        if (!Request.Cookies.ContainsKey("jwt_token"))
        {
            Response.Redirect("/Login");
            return;
        }
        // (opcjonalnie: sprawdź czy rola to Admin)
        var users = _userManager.Users.ToList();
        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            Users.Add(new UserWithRoles { Id = user.Id, Email = user.Email, Roles = roles });
        }
    }
} 