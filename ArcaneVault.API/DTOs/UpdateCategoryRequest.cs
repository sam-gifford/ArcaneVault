using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.API.DTOs
{
    public class UpdateCategoryRequest
    {
        [Required(ErrorMessage = "Category Name is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Category Name must be between 1 and 200 characters")]
        public string CategoryName { get; set; } = null!;
    }
}
