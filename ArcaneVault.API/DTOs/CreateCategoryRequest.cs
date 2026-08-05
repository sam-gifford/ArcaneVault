// Name: [Your Name] | Admin No: [Your Admin No] | Tutorial Group: [Your Group]

using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.API.DTOs
{
    public class CreateCategoryRequest
    {
        [Required(ErrorMessage = "Category Code is required")]
        [StringLength(50, MinimumLength = 1, ErrorMessage = "Category Code must be between 1 and 50 characters")]
        public string CategoryCode { get; set; } = null!;

        [Required(ErrorMessage = "Category Name is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Category Name must be between 1 and 200 characters")]
        public string CategoryName { get; set; } = null!;
    }
}
