// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using ArcaneVault.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;
using System.Security.Claims;

namespace ArcaneVault.Web.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public LoginModel(
            IHttpClientFactory httpClientFactory,
            ILogger<LoginModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public IActionResult OnGet()
        {
            return User.Identity?.IsAuthenticated == true
                ? RedirectToPage("/Index")
                : Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.PostAsJsonAsync(
                    "/api/auth/login",
                    new { userName = UserName, password = Password });

                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = "Invalid username or password.";
                    _logger.LogWarning("Login failed for {UserName}", UserName);
                    return Page();
                }

                var loginResponse = await response.Content.ReadFromJsonAsync<LoginDto>();
                if (loginResponse == null ||
                    !loginResponse.Success ||
                    string.IsNullOrWhiteSpace(loginResponse.Token))
                {
                    ErrorMessage = "The API returned an invalid login response.";
                    return Page();
                }

                HttpContext.Session.SetString("UserName", loginResponse.UserName);
                HttpContext.Session.SetString("Email", loginResponse.Email);
                HttpContext.Session.SetString("Role", loginResponse.Role);
                HttpContext.Session.SetString("JwtToken", loginResponse.Token);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, loginResponse.UserName),
                    new Claim(ClaimTypes.Email, loginResponse.Email),
                    new Claim(ClaimTypes.Role, loginResponse.Role)
                };

                await HttpContext.SignInAsync(
                    "CookieAuth",
                    new ClaimsPrincipal(new ClaimsIdentity(claims, "CookieAuth")),
                    new AuthenticationProperties
                    {
                        IsPersistent = false,
                        ExpiresUtc = loginResponse.TokenExpiration
                    });

                _logger.LogInformation(
                    "User {UserName} logged in as {Role}",
                    loginResponse.UserName,
                    loginResponse.Role);

                return RedirectToPage("/CollectionItems/Index");
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = "The ArcaneVault API is unavailable. Start both projects and try again.";
                _logger.LogError(ex, "API connection failed during login");
                return Page();
            }
        }
    }
}
