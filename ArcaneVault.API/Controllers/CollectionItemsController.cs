// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using ArcaneVault.API.Data;
using ArcaneVault.API.DTOs;
using ArcaneVault.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ArcaneVault.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CollectionItemsController : ControllerBase
    {
        private readonly ArcaneVaultDbContext _context;
        private readonly ILogger<CollectionItemsController> _logger;

        public CollectionItemsController(
            ArcaneVaultDbContext context,
            ILogger<CollectionItemsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        private string? CurrentUserName => User.FindFirstValue(ClaimTypes.Name);

        private IQueryable<CollectionItem> CurrentUserItems()
        {
            var userName = CurrentUserName;
            return _context.CollectionItems
                .Where(item => item.UserName == userName);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<CollectionItemResponse>>> Search(string? query = null)
        {
            var userName = CurrentUserName;
            if (string.IsNullOrWhiteSpace(userName))
            {
                return Unauthorized();
            }

            var itemsQuery = CurrentUserItems()
                .Include(item => item.CollectionItemCategories)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(query))
            {
                var searchText = query.Trim().ToLower();
                var isNumber = int.TryParse(searchText, out var number);

                itemsQuery = itemsQuery.Where(item =>
                    item.ItemName.ToLower().Contains(searchText) ||
                    item.UserName.ToLower().Contains(searchText) ||
                    item.CollectionItemCategories.Any(link =>
                        link.CategoryCode.ToLower().Contains(searchText)) ||
                    (isNumber && (
                        item.ItemId == number ||
                        item.StartingQuantity == number ||
                        item.CurrentQuantity == number)));
            }

            var items = await Project(itemsQuery)
                .OrderBy(item => item.ItemName)
                .ToListAsync();

            _logger.LogInformation(
                "Collection search for {UserName} with query {Query} returned {Count} items",
                userName,
                query,
                items.Count);

            return Ok(items);
        }

        [HttpGet("user/{userName}")]
        public async Task<ActionResult<IEnumerable<CollectionItemResponse>>> GetByUser(string userName)
        {
            if (!string.Equals(userName, CurrentUserName, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var items = await Project(
                    CurrentUserItems().Include(item => item.CollectionItemCategories))
                .OrderBy(item => item.ItemName)
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{itemId:int}")]
        public async Task<ActionResult<CollectionItemResponse>> GetById(int itemId)
        {
            var item = await Project(
                    CurrentUserItems()
                        .Where(item => item.ItemId == itemId)
                        .Include(item => item.CollectionItemCategories))
                .FirstOrDefaultAsync();

            return item == null
                ? NotFound(new { message = $"Collection item with ID {itemId} was not found." })
                : Ok(item);
        }

        [HttpPost]
        public async Task<ActionResult<CollectionItemResponse>> Create(
            [FromBody] CreateCollectionItemRequest request)
        {
            var userName = CurrentUserName;
            if (string.IsNullOrWhiteSpace(userName))
            {
                return Unauthorized();
            }

            if (!string.Equals(request.UserName, userName, StringComparison.OrdinalIgnoreCase))
            {
                return Forbid();
            }

            var validationError = await ValidateItemRequest(
                request.StartingQuantity,
                request.CurrentQuantity,
                request.CategoryCodes);
            if (validationError != null)
            {
                return BadRequest(new { message = validationError });
            }

            var categoryCodes = request.CategoryCodes
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var item = new CollectionItem
            {
                ItemName = request.ItemName.Trim(),
                StartingQuantity = request.StartingQuantity,
                CurrentQuantity = request.CurrentQuantity,
                UserName = userName,
                IsDeleted = false,
                CollectionItemCategories = categoryCodes
                    .Select(code => new CollectionItemCategory { CategoryCode = code })
                    .ToList()
            };

            _context.CollectionItems.Add(item);
            await _context.SaveChangesAsync();

            var response = ToResponse(item);
            return CreatedAtAction(nameof(GetById), new { itemId = item.ItemId }, response);
        }

        [HttpPut("{itemId:int}")]
        public async Task<ActionResult<CollectionItemResponse>> Update(
            int itemId,
            [FromBody] UpdateCollectionItemRequest request)
        {
            var item = await CurrentUserItems()
                .Include(existingItem => existingItem.CollectionItemCategories)
                .FirstOrDefaultAsync(existingItem => existingItem.ItemId == itemId);

            if (item == null)
            {
                return NotFound(new { message = $"Collection item with ID {itemId} was not found." });
            }

            var validationError = await ValidateItemRequest(
                request.StartingQuantity,
                request.CurrentQuantity,
                request.CategoryCodes);
            if (validationError != null)
            {
                return BadRequest(new { message = validationError });
            }

            var categoryCodes = request.CategoryCodes
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            item.ItemName = request.ItemName.Trim();
            item.StartingQuantity = request.StartingQuantity;
            item.CurrentQuantity = request.CurrentQuantity;

            _context.CollectionItemCategories.RemoveRange(item.CollectionItemCategories);
            item.CollectionItemCategories = categoryCodes
                .Select(code => new CollectionItemCategory
                {
                    ItemId = item.ItemId,
                    CategoryCode = code
                })
                .ToList();

            await _context.SaveChangesAsync();
            return Ok(ToResponse(item));
        }

        [HttpDelete("{itemId:int}")]
        public async Task<IActionResult> Delete(int itemId)
        {
            var item = await CurrentUserItems()
                .FirstOrDefaultAsync(existingItem => existingItem.ItemId == itemId);

            if (item == null)
            {
                return NotFound(new { message = $"Collection item with ID {itemId} was not found." });
            }

            item.IsDeleted = true;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        private async Task<string?> ValidateItemRequest(
            int startingQuantity,
            int currentQuantity,
            IEnumerable<string>? requestedCategoryCodes)
        {
            if (currentQuantity > startingQuantity)
            {
                return "Current Quantity cannot exceed Starting Quantity.";
            }

            var categoryCodes = (requestedCategoryCodes ?? Enumerable.Empty<string>())
                .Where(code => !string.IsNullOrWhiteSpace(code))
                .Select(code => code.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (categoryCodes.Count == 0)
            {
                return null;
            }

            var validCodes = await _context.Categories
                .Where(category => categoryCodes.Contains(category.CategoryCode))
                .Select(category => category.CategoryCode)
                .ToListAsync();

            var invalidCodes = categoryCodes
                .Except(validCodes, StringComparer.OrdinalIgnoreCase)
                .ToList();

            return invalidCodes.Count == 0
                ? null
                : $"Invalid category codes: {string.Join(", ", invalidCodes)}";
        }

        private static IQueryable<CollectionItemResponse> Project(
            IQueryable<CollectionItem> query)
        {
            return query.Select(item => new CollectionItemResponse
            {
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                StartingQuantity = item.StartingQuantity,
                CurrentQuantity = item.CurrentQuantity,
                UserName = item.UserName,
                CategoryCodes = item.CollectionItemCategories
                    .Select(link => link.CategoryCode)
                    .ToList()
            });
        }

        private static CollectionItemResponse ToResponse(CollectionItem item)
        {
            return new CollectionItemResponse
            {
                ItemId = item.ItemId,
                ItemName = item.ItemName,
                StartingQuantity = item.StartingQuantity,
                CurrentQuantity = item.CurrentQuantity,
                UserName = item.UserName,
                CategoryCodes = item.CollectionItemCategories
                    .Select(link => link.CategoryCode)
                    .ToList()
            };
        }
    }
}
