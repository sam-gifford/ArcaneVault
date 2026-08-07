// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Json;

namespace ArcaneVault.Web.Pages.Auth
{
    public class RegisterModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<RegisterModel> _logger;

        [BindProperty]
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3)]
        [RegularExpression(@"^[A-Za-z0-9._-]+$", ErrorMessage = "Use only letters, numbers, dots, underscores, and hyphens")]
        public string UserName { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Enter a valid email address")]
        public string Email { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [BindProperty]
        [Required(ErrorMessage = "Confirm Password is required")]
        [Compare(nameof(Password), ErrorMessage = "Passwords do not match")]
        [DataType(DataType.Password)]
        public string ConfirmPassword { get; set; } = string.Empty;

        public string? SuccessMessage { get; set; }
        public string? ErrorMessage { get; set; }

        public RegisterModel(
            IHttpClientFactory httpClientFactory,
            ILogger<RegisterModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
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
                    "/api/auth/register",
                    new { userName = UserName, email = Email, password = Password });

                var apiResponse = await response.Content.ReadFromJsonAsync<RegisterApiResponse>();
                if (!response.IsSuccessStatusCode)
                {
                    ErrorMessage = apiResponse?.Message ?? "Registration failed.";
                    _logger.LogWarning(
                        "Registration failed for {UserName}: {Message}",
                        UserName,
                        ErrorMessage);
                    return Page();
                }

                TempData["SuccessMessage"] = "Registration successful. You can now log in.";
                _logger.LogInformation("User {UserName} registered successfully", UserName);
                return RedirectToPage("./Login");
            }
            catch (HttpRequestException ex)
            {
                ErrorMessage = "The ArcaneVault API is unavailable. Start both projects and try again.";
                _logger.LogError(ex, "API connection failed during registration");
                return Page();
            }
        }

        private sealed class RegisterApiResponse
        {
            public bool Success { get; set; }
            public string Message { get; set; } = string.Empty;
        }
    }
}
