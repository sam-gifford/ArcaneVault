// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using ArcaneVault.API.Data;
using ArcaneVault.API.DTOs;
using ArcaneVault.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ArcaneVault.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ArcaneVaultDbContext _context;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ArcaneVaultDbContext context, ILogger<CategoriesController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Get all categories
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CategoryResponse>>> GetAll()
        {
            try
            {
                var categories = await _context.Categories
                    .Select(c => new CategoryResponse
                    {
                        CategoryCode = c.CategoryCode,
                        CategoryName = c.CategoryName
                    })
                    .ToListAsync();

                _logger.LogInformation($"Retrieved {categories.Count} categories");
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving categories: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving categories" });
            }
        }

        /// <summary>
        /// Get a specific category by code
        /// </summary>
        [HttpGet("{categoryCode}")]
        public async Task<ActionResult<CategoryResponse>> GetById(string categoryCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoryCode))
                {
                    return BadRequest(new { message = "Category Code is required" });
                }

                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryCode == categoryCode);

                if (category == null)
                {
                    _logger.LogWarning($"Category not found: {categoryCode}");
                    return NotFound(new { message = $"Category with code '{categoryCode}' not found" });
                }

                var response = new CategoryResponse
                {
                    CategoryCode = category.CategoryCode,
                    CategoryName = category.CategoryName
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving category {categoryCode}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while retrieving the category" });
            }
        }

        /// <summary>
        /// Create a new category
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "Staff")]
        public async Task<ActionResult<CategoryResponse>> Create([FromBody] CreateCategoryRequest request)
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
                // Check if category code already exists
                var existingCategory = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryCode == request.CategoryCode);

                if (existingCategory != null)
                {
                    _logger.LogWarning($"Duplicate category code: {request.CategoryCode}");
                    return BadRequest(new { message = $"Category code '{request.CategoryCode}' already exists" });
                }

                var category = new Category
                {
                    CategoryCode = request.CategoryCode,
                    CategoryName = request.CategoryName
                };

                _context.Categories.Add(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Category created successfully: {request.CategoryCode}");

                var response = new CategoryResponse
                {
                    CategoryCode = category.CategoryCode,
                    CategoryName = category.CategoryName
                };

                return CreatedAtAction(nameof(GetById), new { categoryCode = category.CategoryCode }, response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error creating category: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while creating the category" });
            }
        }

        /// <summary>
        /// Update an existing category
        /// </summary>
        [HttpPut("{categoryCode}")]
        [Authorize(Roles = "Staff")]
        public async Task<ActionResult<CategoryResponse>> Update(string categoryCode, [FromBody] UpdateCategoryRequest request)
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
                if (string.IsNullOrWhiteSpace(categoryCode))
                {
                    return BadRequest(new { message = "Category Code is required" });
                }

                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryCode == categoryCode);

                if (category == null)
                {
                    _logger.LogWarning($"Category not found for update: {categoryCode}");
                    return NotFound(new { message = $"Category with code '{categoryCode}' not found" });
                }

                category.CategoryName = request.CategoryName;
                _context.Categories.Update(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Category updated successfully: {categoryCode}");

                var response = new CategoryResponse
                {
                    CategoryCode = category.CategoryCode,
                    CategoryName = category.CategoryName
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error updating category {categoryCode}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while updating the category" });
            }
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        [HttpDelete("{categoryCode}")]
        [Authorize(Roles = "Staff")]
        public async Task<IActionResult> Delete(string categoryCode)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(categoryCode))
                {
                    return BadRequest(new { message = "Category Code is required" });
                }

                var category = await _context.Categories
                    .FirstOrDefaultAsync(c => c.CategoryCode == categoryCode);

                if (category == null)
                {
                    _logger.LogWarning($"Category not found for deletion: {categoryCode}");
                    return NotFound(new { message = $"Category with code '{categoryCode}' not found" });
                }

                // Check if category is in use
                var isInUse = await _context.CollectionItemCategories
                    .AnyAsync(cic => cic.CategoryCode == categoryCode);

                if (isInUse)
                {
                    _logger.LogWarning($"Cannot delete category in use: {categoryCode}");
                    return BadRequest(new { message = $"Cannot delete category '{categoryCode}' as it is associated with collection items" });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Category deleted successfully: {categoryCode}");

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error deleting category {categoryCode}: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new { message = "An error occurred while deleting the category" });
            }
        }
    }
}
