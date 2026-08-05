// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Auth
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel> _logger;

        public LogoutModel(ILogger<LogoutModel> logger)
        {
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var userName = HttpContext.Session.GetString("UserName");

            // Clear session
            HttpContext.Session.Clear();

            // Sign out from cookie authentication
            await HttpContext.SignOutAsync("CookieAuth");

            if (!string.IsNullOrEmpty(userName))
            {
                _logger.LogInformation($"User logged out: {userName}");
            }

            return RedirectToPage("/Index");
        }
    }
}
