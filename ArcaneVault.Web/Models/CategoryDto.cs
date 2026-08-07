// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.Web.Models
{
    public class CategoryDto
    {
        [Required(ErrorMessage = "Category Code is required")]
        [StringLength(50)]
        public string CategoryCode { get; set; } = null!;

        [Required(ErrorMessage = "Category Name is required")]
        [StringLength(200)]
        public string CategoryName { get; set; } = null!;
    }
}
