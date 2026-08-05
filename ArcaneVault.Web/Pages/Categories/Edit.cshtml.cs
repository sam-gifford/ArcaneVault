using ArcaneVault.Web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.Categories
{
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EditModel> _logger;

        [BindProperty]
        public CategoryDto Category { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public string? CategoryCode { get; set; }

        public string? ErrorMessage { get; set; }

        public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger)
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
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new CategoryDto();

                    _logger.LogInformation($"Retrieved category for edit: {categoryCode}");
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
            if (!ModelState.IsValid || string.IsNullOrWhiteSpace(CategoryCode))
            {
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var updateRequest = new { categoryName = Category.CategoryName };
                var jsonContent = new System.Net.Http.StringContent(
                    System.Text.Json.JsonSerializer.Serialize(updateRequest),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PutAsync($"/api/categories/{CategoryCode}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Category updated successfully: {CategoryCode}");
                    return RedirectToPage("./Index");
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to update category: {content}";
                    _logger.LogError(ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while updating the category.";
                _logger.LogError($"Error updating category: {ex.Message}");
            }

            return Page();
        }
    }
}
