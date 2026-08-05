// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using ArcaneVault.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.CollectionItems
{
    [Authorize]
    public class EditModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<EditModel> _logger;

        [BindProperty]
        public CollectionItemDto Item { get; set; } = new();

        [BindProperty(SupportsGet = true)]
        public int? ItemId { get; set; }

        [BindProperty]
        public string? SelectedCategories { get; set; }

        public List<CategoryDto> AvailableCategories { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string UserName { get; set; } = string.Empty;

        public EditModel(IHttpClientFactory httpClientFactory, ILogger<EditModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            UserName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? string.Empty;

            if (!ItemId.HasValue || ItemId <= 0)
            {
                return RedirectToPage("./Index");
            }

            await LoadCategories();

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.GetAsync($"/api/collectionitems/{ItemId}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Item = System.Text.Json.JsonSerializer.Deserialize<CollectionItemDto>(jsonContent,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new CollectionItemDto();

                    // Verify ownership
                    if (Item.UserName != UserName)
                    {
                        ErrorMessage = "You don't have permission to edit this item.";
                        return RedirectToPage("./Index");
                    }

                    SelectedCategories = string.Join(",", Item.CategoryCodes);
                    _logger.LogInformation($"Retrieved item for edit: {ItemId}");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return RedirectToPage("./Index");
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while loading the item.";
                _logger.LogError($"Error loading collection item {ItemId}: {ex.Message}");
            }

            return Page();
        }

        private async Task LoadCategories()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.GetAsync("/api/categories");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    AvailableCategories = System.Text.Json.JsonSerializer.Deserialize<List<CategoryDto>>(jsonContent,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                        ?? new List<CategoryDto>();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error loading categories: {ex.Message}");
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ItemId.HasValue || ItemId <= 0)
            {
                return RedirectToPage("./Index");
            }

            if (string.IsNullOrWhiteSpace(Item.ItemName))
            {
                ErrorMessage = "Item Name is required.";
                await LoadCategories();
                return Page();
            }

            if (Item.CurrentQuantity > Item.StartingQuantity)
            {
                ErrorMessage = "Current Quantity cannot exceed Starting Quantity.";
                await LoadCategories();
                return Page();
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var categoryCodes = SelectedCategories?.Split(',')
                    .Select(c => c.Trim())
                    .Where(c => !string.IsNullOrEmpty(c))
                    .ToList() ?? new List<string>();

                var updateRequest = new
                {
                    itemName = Item.ItemName,
                    startingQuantity = Item.StartingQuantity,
                    currentQuantity = Item.CurrentQuantity,
                    categoryCodes = categoryCodes
                };

                var jsonContent = new System.Net.Http.StringContent(
                    System.Text.Json.JsonSerializer.Serialize(updateRequest),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PutAsync($"/api/collectionitems/{ItemId}", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Collection item updated: {ItemId}");
                    return RedirectToPage("./Index");
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to update item: {content}";
                    _logger.LogError(ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while updating the item.";
                _logger.LogError($"Error updating collection item: {ex.Message}");
            }

            await LoadCategories();
            return Page();
        }
    }
}
