// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using ArcaneVault.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Categories
{
    [Authorize(Roles = "Staff")]
    public class DetailsModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DetailsModel> _logger;

        public CategoryDto? Category { get; set; }
        public string? ErrorMessage { get; set; }

        public DetailsModel(IHttpClientFactory httpClientFactory, ILogger<DetailsModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string? categoryCode)
        {
            if (string.IsNullOrWhiteSpace(categoryCode))
            {
                ErrorMessage = "Category Code is required.";
                return RedirectToPage("./Index");
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.GetAsync($"/api/categories/{categoryCode}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Category = System.Text.Json.JsonSerializer.Deserialize<CategoryDto>(jsonContent,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    _logger.LogInformation($"Retrieved category details: {categoryCode}");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    ErrorMessage = $"Category '{categoryCode}' not found.";
                    return RedirectToPage("./Index");
                }
                else
                {
                    ErrorMessage = $"Failed to retrieve category. Status: {response.StatusCode}";
                    _logger.LogError(ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while loading the category.";
                _logger.LogError($"Error loading category {categoryCode}: {ex.Message}");
            }

            return Page();
        }
    }
}
