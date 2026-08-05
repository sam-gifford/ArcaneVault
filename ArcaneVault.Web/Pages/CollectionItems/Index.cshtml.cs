// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using ArcaneVault.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.CollectionItems
{
    /// <summary>
    /// IndexModel - Displays user's collection items with search functionality.
    /// Uses API search endpoint to filter items dynamically.
    /// </summary>
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexModel> _logger;

        public List<CollectionItemDto> CollectionItems { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string UserName { get; set; } = string.Empty;

        [BindProperty(SupportsGet = true)]
        public string? SearchQuery { get; set; }

        public IndexModel(IHttpClientFactory httpClientFactory, ILogger<IndexModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            UserName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? string.Empty;

            try
            {
                var httpClient = _httpClientFactory.CreateClient("ApiClient");

                if (!string.IsNullOrWhiteSpace(SearchQuery))
                {
                    // Use API search endpoint with query term
                    var response = await httpClient.GetAsync($"/api/collectionitems/search?query={Uri.EscapeDataString(SearchQuery)}");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = await response.Content.ReadAsStringAsync();
                        var allSearchResults = System.Text.Json.JsonSerializer.Deserialize<List<CollectionItemDto>>(jsonContent,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                            ?? new List<CollectionItemDto>();

                        // Filter to current user's items only
                        CollectionItems = allSearchResults.Where(i => i.UserName == UserName).ToList();
                        _logger.LogInformation($"Search executed for user {UserName} with query '{SearchQuery}'. Found {CollectionItems.Count} results");
                    }
                    else
                    {
                        ErrorMessage = $"Failed to search items. Status: {response.StatusCode}";
                        _logger.LogError(ErrorMessage);
                    }
                }
                else
                {
                    // Get all user's items
                    var response = await httpClient.GetAsync($"/api/collectionitems/user/{UserName}");

                    if (response.IsSuccessStatusCode)
                    {
                        var jsonContent = await response.Content.ReadAsStringAsync();
                        CollectionItems = System.Text.Json.JsonSerializer.Deserialize<List<CollectionItemDto>>(jsonContent,
                            new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                            ?? new List<CollectionItemDto>();

                        _logger.LogInformation($"Retrieved {CollectionItems.Count} collection items for user: {UserName}");
                    }
                    else
                    {
                        ErrorMessage = $"Failed to retrieve items. Status: {response.StatusCode}";
                        _logger.LogError(ErrorMessage);
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while loading your collection.";
                _logger.LogError($"Error loading collection items: {ex.Message}");
            }
        }
    }
}
