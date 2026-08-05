// Name: [Your Name] | Admin No: [Your Admin No] | Tutorial Group: [Your Group]

using ArcaneVault.Web.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Security.Claims;

namespace ArcaneVault.Web.Pages.Auth
{
    public class LoginModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<LoginModel> _logger;

        [BindProperty]
        public string UserName { get; set; } = string.Empty;

        public string? ErrorMessage { get; set; }

        public LoginModel(IHttpClientFactory httpClientFactory, ILogger<LoginModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public void OnGet()
        {
            // Check if already logged in
            if (User.Identity?.IsAuthenticated == true)
            {
                RedirectToPage("/Index");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(UserName))
            {
                ErrorMessage = "Username is required.";
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var loginRequest = new { userName = UserName };
                var jsonContent = new System.Net.Http.StringContent(
                    System.Text.Json.JsonSerializer.Serialize(loginRequest),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync("/api/auth/login", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var loginResponse = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(jsonResponse,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (loginResponse != null && loginResponse.ContainsKey("userName"))
                    {
                        var userName = loginResponse["userName"]?.ToString() ?? UserName;
                        var email = loginResponse["email"]?.ToString() ?? string.Empty;
                        var role = loginResponse["role"]?.ToString() ?? "User";

                        // Store user info in session
                        HttpContext.Session.SetString("UserName", userName);
                        HttpContext.Session.SetString("Email", email);
                        HttpContext.Session.SetString("Role", role);

                        // Create claims for cookie authentication
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Name, userName),
                            new Claim(ClaimTypes.Email, email),
                            new Claim(ClaimTypes.Role, role)
                        };

                        var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
                        var authProperties = new AuthenticationProperties
                        {
                            IsPersistent = false,
                            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(1)
                        };

                        await HttpContext.SignInAsync("CookieAuth",
                            new ClaimsPrincipal(claimsIdentity),
                            authProperties);

                        _logger.LogInformation($"User logged in successfully: {userName}");
                        return RedirectToPage("/Index");
                    }
                }
                else
                {
                    ErrorMessage = "Invalid username. Please check and try again.";
                    _logger.LogWarning($"Login attempt failed for user: {UserName}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during login. Please try again.";
                _logger.LogError($"Error during login: {ex.Message}");
            }

            return Page();
        }
    }
}
