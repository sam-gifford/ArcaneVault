using ArcaneVault.API.Data;
using ArcaneVault.API.DTOs;
using ArcaneVault.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CollectionItemsController : ControllerBase
    {
        private readonly ArcaneVaultDbContext _context;
        private readonly ILogger<CollectionItemsController> _logger;

        public CollectionItemsController(ArcaneVaultDbContext context, ILogger<CollectionItemsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all collection items for a specific user
        /// </summary>
        [HttpGet("user/{userName}")]
        public async Task<ActionResult<IEnumerable<CollectionItemResponse>>> GetByUser(string userName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userName))
                {
                    return BadRequest(new { message = "UserName is required" });
                }

                var items = await _context.CollectionItems
                    .Where(i => i.UserName == userName && !i.IsDeleted)
                    .Include(i => i.CollectionItemCategories)
                    .Select(i => new CollectionItemResponse
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName,
                        StartingQuantity = i.StartingQuantity,
                        CurrentQuantity = i.CurrentQuantity,
                        UserName = i.UserName,
                        CategoryCodes = i.CollectionItemCategories.Select(c => c.CategoryCode).ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {items.Count} collection items for user: {userName}");
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving collection items for user {userName}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving collection items" });
            }
        }

        /// <summary>
        /// Get a specific collection item by ID
        /// </summary>
        [HttpGet("{itemId}")]
        public async Task<ActionResult<CollectionItemResponse>> GetById(int itemId)
        {
            try
            {
                var item = await _context.CollectionItems
                    .Where(i => i.ItemId == itemId && !i.IsDeleted)
                    .Include(i => i.CollectionItemCategories)
                    .FirstOrDefaultAsync();

                if (item == null)
                {
                    _logger.LogWarning($"Collection item not found: {itemId}");
                    return NotFound(new { message = $"Collection item with ID {itemId} not found" });
                }

                var response = new CollectionItemResponse
                {
                    ItemId = item.ItemId,
                    ItemName = item.ItemName,
                    StartingQuantity = item.StartingQuantity,
                    CurrentQuantity = item.CurrentQuantity,
                    UserName = item.UserName,
                    CategoryCodes = item.CollectionItemCategories.Select(c => c.CategoryCode).ToList()
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving collection item {itemId}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the collection item" });
            }
        }

        /// <summary>
        /// Search collection items by any field (ItemName, UserName, CategoryCode)
        /// </summary>
        [HttpGet("search/query")]
        public async Task<ActionResult<IEnumerable<CollectionItemResponse>>> Search(string? searchTerm = null, string? userName = null, string? categoryCode = null)
        {
            try
            {
                var query = _context.CollectionItems
                    .Where(i => !i.IsDeleted)
                    .AsQueryable();

                // Filter by userName if provided
                if (!string.IsNullOrWhiteSpace(userName))
                {
                    query = query.Where(i => i.UserName == userName);
                }

                // Search by term in ItemName if provided
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    var lowerSearchTerm = searchTerm.ToLower();
                    query = query.Where(i => i.ItemName.ToLower().Contains(lowerSearchTerm));
                }

                // Filter by categoryCode if provided
                if (!string.IsNullOrWhiteSpace(categoryCode))
                {
                    query = query.Where(i => i.CollectionItemCategories
                        .Any(c => c.CategoryCode == categoryCode));
                }

                var items = await query
                    .Include(i => i.CollectionItemCategories)
                    .Select(i => new CollectionItemResponse
                    {
                        ItemId = i.ItemId,
                        ItemName = i.ItemName,
                        StartingQuantity = i.StartingQuantity,
                        CurrentQuantity = i.CurrentQuantity,
                        UserName = i.UserName,
                        CategoryCodes = i.CollectionItemCategories.Select(c => c.CategoryCode).ToList()
                    })
                    .ToListAsync();

                _logger.LogInformation($"Search executed with term='{searchTerm}', userName='{userName}', categoryCode='{categoryCode}'. Found {items.Count} results");
                return Ok(items);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error searching collection items: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while searching collection items" });
            }
        }

        /// <summary>
        /// Create a new collection item
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<CollectionItemResponse>> Create([FromBody] CreateCollectionItemRequest request)
        {
            // Validate request
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = string.Join("; ", errors) });
            }

            try
            {
                // Verify user exists
                var userExists = await _context.ArcaneVaultUsers
                    .AnyAsync(u => u.UserName == request.UserName && !u.IsDeleted);

                if (!userExists)
                {
                    _logger.LogWarning($"Cannot create item: User '{request.UserName}' not found");
                    return BadRequest(new { message = $"User '{request.UserName}' not found" });
                }

                // Validate CurrentQuantity does not exceed StartingQuantity
                if (request.CurrentQuantity > request.StartingQuantity)
                {
                    return BadRequest(new { message = "Current Quantity cannot exceed Starting Quantity" });
                }

                // Verify all provided categories exist
                if (request.CategoryCodes.Any())
                {
                    var validCategories = await _context.Categories
                        .Where(c => request.CategoryCodes.Contains(c.CategoryCode))
                        .Select(c => c.CategoryCode)
                        .ToListAsync();

                    var invalidCategories = request.CategoryCodes.Except(validCategories).ToList();
                    if (invalidCategories.Any())
                    {
                        _logger.LogWarning($"Invalid category codes: {string.Join(", ", invalidCategories)}");
                        return BadRequest(new { message = $"Invalid category codes: {string.Join(", ", invalidCategories)}" });
                    }
                }

                var item = new CollectionItem
                {
                    ItemName = request.ItemName,
                    StartingQuantity = request.StartingQuantity,
                    CurrentQuantity = request.CurrentQuantity,
                    UserName = request.UserName,
                    IsDeleted = false
                };

                _context.CollectionItems.Add(item);
                await _context.SaveChangesAsync();

                // Add categories
                if (request.CategoryCodes.Any())
                {
                    foreach (var categoryCode in request.CategoryCodes)
                    {
                        var itemCategory = new CollectionItemCategory
                        {
                            ItemId = item.ItemId,
                            CategoryCode = categoryCode
                        };
                        _context.CollectionItemCategories.Add(itemCategory);
                    }
                    await _context.SaveChangesAsync();
                }

                _logger.LogInformation($"Collection item created successfully: {item.ItemId}");

                var response = new CollectionItemResponse
                {
                    ItemId = item.ItemId,
                    ItemName = item.ItemName,
                    StartingQuantity = item.StartingQuantity,
                    CurrentQuantity = item.CurrentQuantity,
                    UserName = item.UserName,
                    CategoryCodes = request.CategoryCodes
                };

                return CreatedAtAction(nameof(GetById), new { itemId = item.ItemId }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating collection item: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the collection item" });
            }
        }

        /// <summary>
        /// Update an existing collection item
        /// </summary>
        [HttpPut("{itemId}")]
        public async Task<ActionResult<CollectionItemResponse>> Update(int itemId, [FromBody] UpdateCollectionItemRequest request)
        {
            // Validate request
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new { message = string.Join("; ", errors) });
            }

            try
            {
                var item = await _context.CollectionItems
                    .Include(i => i.CollectionItemCategories)
                    .FirstOrDefaultAsync(i => i.ItemId == itemId && !i.IsDeleted);

                if (item == null)
                {
                    _logger.LogWarning($"Collection item not found for update: {itemId}");
                    return NotFound(new { message = $"Collection item with ID {itemId} not found" });
                }

                // Validate CurrentQuantity does not exceed StartingQuantity
                if (request.CurrentQuantity > request.StartingQuantity)
                {
                    return BadRequest(new { message = "Current Quantity cannot exceed Starting Quantity" });
                }

                // Verify all provided categories exist
                if (request.CategoryCodes.Any())
                {
                    var validCategories = await _context.Categories
                        .Where(c => request.CategoryCodes.Contains(c.CategoryCode))
                        .Select(c => c.CategoryCode)
                        .ToListAsync();

                    var invalidCategories = request.CategoryCodes.Except(validCategories).ToList();
                    if (invalidCategories.Any())
                    {
                        _logger.LogWarning($"Invalid category codes: {string.Join(", ", invalidCategories)}");
                        return BadRequest(new { message = $"Invalid category codes: {string.Join(", ", invalidCategories)}" });
                    }
                }

                // Update item properties
                item.ItemName = request.ItemName;
                item.StartingQuantity = request.StartingQuantity;
                item.CurrentQuantity = request.CurrentQuantity;

                // Update categories
                _context.CollectionItemCategories.RemoveRange(item.CollectionItemCategories);
                foreach (var categoryCode in request.CategoryCodes)
                {
                    var itemCategory = new CollectionItemCategory
                    {
                        ItemId = item.ItemId,
                        CategoryCode = categoryCode
                    };
                    _context.CollectionItemCategories.Add(itemCategory);
                }

                _context.CollectionItems.Update(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Collection item updated successfully: {itemId}");

                var response = new CollectionItemResponse
                {
                    ItemId = item.ItemId,
                    ItemName = item.ItemName,
                    StartingQuantity = item.StartingQuantity,
                    CurrentQuantity = item.CurrentQuantity,
                    UserName = item.UserName,
                    CategoryCodes = request.CategoryCodes
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating collection item {itemId}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the collection item" });
            }
        }

        /// <summary>
        /// Delete a collection item (soft delete)
        /// </summary>
        [HttpDelete("{itemId}")]
        public async Task<IActionResult> Delete(int itemId)
        {
            try
            {
                var item = await _context.CollectionItems
                    .FirstOrDefaultAsync(i => i.ItemId == itemId && !i.IsDeleted);

                if (item == null)
                {
                    _logger.LogWarning($"Collection item not found for deletion: {itemId}");
                    return NotFound(new { message = $"Collection item with ID {itemId} not found" });
                }

                // Soft delete
                item.IsDeleted = true;
                _context.CollectionItems.Update(item);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Collection item deleted successfully: {itemId}");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting collection item {itemId}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the collection item" });
            }
        }
    }
}
