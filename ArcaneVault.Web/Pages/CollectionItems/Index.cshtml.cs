// Name: [Your Name] | Admin No: [Your Admin No] | Tutorial Group: [Your Group]

using ArcaneVault.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.CollectionItems
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<IndexModel> _logger;

        public List<CollectionItemDto> CollectionItems { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string UserName { get; set; } = string.Empty;

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
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while loading your collection.";
                _logger.LogError($"Error loading collection items: {ex.Message}");
            }
        }
    }
}
