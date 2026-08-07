// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using ArcaneVault.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Categories
{
    [Authorize(Roles = "Staff")]
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DeleteModel> _logger;

        public CategoryDto? Category { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? CategoryCode { get; set; }

        public string? ErrorMessage { get; set; }

        public DeleteModel(IHttpClientFactory httpClientFactory, ILogger<DeleteModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync(string? categoryCode)
        {
            if (string.IsNullOrWhiteSpace(categoryCode))
            {
                return RedirectToPage("./Index");
            }

            CategoryCode = categoryCode;

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.GetAsync($"/api/categories/{categoryCode}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Category = System.Text.Json.JsonSerializer.Deserialize<CategoryDto>(jsonContent,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    _logger.LogInformation($"Retrieved category for deletion: {categoryCode}");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (string.IsNullOrWhiteSpace(CategoryCode))
            {
                return RedirectToPage("./Index");
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.DeleteAsync($"/api/categories/{CategoryCode}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Category deleted successfully: {CategoryCode}");
                    return RedirectToPage("./Index");
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to delete category: {content}";
                    _logger.LogError(ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while deleting the category.";
                _logger.LogError($"Error deleting category: {ex.Message}");
            }

            return Page();
        }
    }
}
