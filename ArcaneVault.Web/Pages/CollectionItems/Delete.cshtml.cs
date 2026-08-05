// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using ArcaneVault.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.CollectionItems
{
    [Authorize]
    public class DeleteModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DeleteModel> _logger;

        public CollectionItemDto? Item { get; set; }

        [BindProperty(SupportsGet = true)]
        public int? ItemId { get; set; }

        public string? ErrorMessage { get; set; }

        public DeleteModel(IHttpClientFactory httpClientFactory, ILogger<DeleteModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            if (!ItemId.HasValue || ItemId <= 0)
            {
                return RedirectToPage("./Index");
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.GetAsync($"/api/collectionitems/{ItemId}");

                if (response.IsSuccessStatusCode)
                {
                    var jsonContent = await response.Content.ReadAsStringAsync();
                    Item = System.Text.Json.JsonSerializer.Deserialize<CollectionItemDto>(jsonContent,
                        new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    // Verify ownership
                    var userName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? string.Empty;
                    if (Item?.UserName != userName)
                    {
                        ErrorMessage = "You don't have permission to delete this item.";
                        return RedirectToPage("./Index");
                    }

                    _logger.LogInformation($"Retrieved item for deletion: {ItemId}");
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

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ItemId.HasValue || ItemId <= 0)
            {
                return RedirectToPage("./Index");
            }

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");
                var response = await httpClient.DeleteAsync($"/api/collectionitems/{ItemId}");

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Collection item deleted: {ItemId}");
                    return RedirectToPage("./Index");
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to delete item: {content}";
                    _logger.LogError(ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while deleting the item.";
                _logger.LogError($"Error deleting collection item: {ex.Message}");
            }

            return Page();
        }
    }
}
