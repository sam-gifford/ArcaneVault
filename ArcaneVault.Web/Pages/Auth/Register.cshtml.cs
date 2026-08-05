// Name: [Your Name] | Admin No: [Your Admin No] | Tutorial Group: [Your Group]

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RegisterModel> _logger;

        [BindProperty]
        public string UserName { get; set; } = string.Empty;

        [BindProperty]
        public string Email { get; set; } = string.Empty;

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public RegisterModel(IHttpClientFactory httpClientFactory, ILogger<RegisterModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(UserName) || string.IsNullOrWhiteSpace(Email))
            {
                ErrorMessage = "Username and Email are required.";
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var registerRequest = new { userName = UserName, email = Email };
                var jsonContent = new System.Net.Http.StringContent(
                    System.Text.Json.JsonSerializer.Serialize(registerRequest),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync("/api/auth/register", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"User registered successfully: {UserName}");
                    SuccessMessage = $"Registration successful! Welcome, {UserName}. You can now log in.";
                    UserName = string.Empty;
                    Email = string.Empty;
                    return Page();
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Registration failed: {content}";
                    _logger.LogWarning($"Registration failed for user: {UserName} - {content}");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred during registration. Please try again.";
                _logger.LogError($"Error during registration: {ex.Message}");
            }

            return Page();
        }
    }
}
