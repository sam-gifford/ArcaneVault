// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.Web.Models
{
    public class CollectionItemDto
    {
        public int ItemId { get; set; }

        [Required(ErrorMessage = "Item Name is required")]
        [StringLength(200)]
        public string ItemName { get; set; } = null!;

        [Range(0, int.MaxValue, ErrorMessage = "Starting Quantity must be 0 or greater")]
        public int StartingQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Current Quantity must be 0 or greater")]
        public int CurrentQuantity { get; set; }
        public string UserName { get; set; } = null!;
        public List<string> CategoryCodes { get; set; } = new List<string>();
    }
}
