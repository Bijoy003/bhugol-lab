using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using BhugolLab.Web.Options;
using BhugolLab.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;

namespace BhugolLab.Web.Pages.Admin;

[EnableRateLimiting("login")]
public class LoginModel(IOptions<AdminOptions> admin, ILogger<LoginModel> logger) : PageModel
{
    private readonly AdminOptions _admin = admin.Value;

    [BindProperty]
    [Microsoft.AspNetCore.Mvc.ModelBinding.BindNever]
    public string ReturnUrl { get; set; } = "/admin/leads";

    [BindProperty(Name = "username")]
    public string Username { get; set; } = string.Empty;

    [BindProperty(Name = "password")]
    public string Password { get; set; } = string.Empty;

    public bool NotConfigured { get; private set; }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password))
        {
            ModelState.AddModelError(string.Empty, "Enter your username and password.");
            return Page();
        }

        if (!_admin.IsConfigured)
        {
            logger.LogWarning("Admin login attempted but Admin:Username / Admin:PasswordHash are not configured");
            NotConfigured = true;
            ModelState.AddModelError(string.Empty, "Admin access is not configured on this deployment.");
            return Page();
        }

        var userOk = FixedTimeEquals(Username, _admin.Username);
        var passwordOk = PasswordHasher.Verify(Password, _admin.PasswordHash);
        if (!userOk || !passwordOk)
        {
            logger.LogWarning("Failed admin login attempt for {Username}", Username);
            ModelState.AddModelError(string.Empty, "Invalid username or password.");
            return Page();
        }

        var identity = new ClaimsIdentity(
            [new Claim(ClaimTypes.Name, _admin.Username)],
            CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties { IsPersistent = false });

        logger.LogInformation("Admin {Username} signed in", _admin.Username);
        return RedirectToPage("/Admin/Leads");
    }

    private static bool FixedTimeEquals(string left, string right) =>
        CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(left), Encoding.UTF8.GetBytes(right));
}
