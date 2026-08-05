// Name: [Your Name] | Admin No: [Your Admin No] | Tutorial Group: [Your Group]

using ArcaneVault.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArcaneVault.Web.Pages.CollectionItems
{
    [Authorize]
    public class CreateModel : PageModel
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<CreateModel> _logger;

        [BindProperty]
        public CollectionItemDto Item { get; set; } = new();

        [BindProperty]
        public string? SelectedCategories { get; set; }

        public List<CategoryDto> AvailableCategories { get; set; } = new();
        public string? ErrorMessage { get; set; }
        public string UserName { get; set; } = string.Empty;

        public CreateModel(IHttpClientFactory httpClientFactory, ILogger<CreateModel> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task OnGetAsync()
        {
            UserName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? string.Empty;
            await LoadCategories();
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
            UserName = User.FindFirst(System.Security.Claims.ClaimTypes.Name)?.Value ?? string.Empty;

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

                var createRequest = new
                {
                    itemName = Item.ItemName,
                    startingQuantity = Item.StartingQuantity,
                    currentQuantity = Item.CurrentQuantity,
                    userName = UserName,
                    categoryCodes = categoryCodes
                };

                var jsonContent = new System.Net.Http.StringContent(
                    System.Text.Json.JsonSerializer.Serialize(createRequest),
                    System.Text.Encoding.UTF8,
                    "application/json"
                );

                var response = await httpClient.PostAsync("/api/collectionitems", jsonContent);

                if (response.IsSuccessStatusCode)
                {
                    _logger.LogInformation($"Collection item created: {Item.ItemName}");
                    return RedirectToPage("./Index");
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    ErrorMessage = $"Failed to create item: {content}";
                    _logger.LogError(ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = "An error occurred while creating the item.";
                _logger.LogError($"Error creating collection item: {ex.Message}");
            }

            await LoadCategories();
            return Page();
        }
    }
}
