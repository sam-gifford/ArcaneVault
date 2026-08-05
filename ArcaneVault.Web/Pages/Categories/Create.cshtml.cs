// Name: [Your Name] | Admin No: [Your Admin No] | Tutorial Group: [Your Group]

using ArcaneVault.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Categories
{
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        public CategoryDto Category { get; set; } = new();

        public string? ErrorMessage { get; set; }

        public CreateModel(IHttpClientFactory httpClientFactory, ILogger<CreateModel> logger)
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
                var jsonContent = new System.Net.Http.StringContent(
                    System.Text.Json.JsonSerializer.Serialize(Category),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync("/api/categories", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Category created successfully: {Category.CategoryCode}");
                    return RedirectToPage("./Index");
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to create category: {content}";
                    _logger.LogError(ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while creating the category.";
                _logger.LogError($"Error creating category: {ex.Message}");
            }

            return Page();
        }
    }
}
