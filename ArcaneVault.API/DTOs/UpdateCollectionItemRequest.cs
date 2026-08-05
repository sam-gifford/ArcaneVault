// Name: [Your Name] | Admin No: [Your Admin No] | Tutorial Group: [Your Group]

using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.API.DTOs
{
    public class UpdateCollectionItemRequest
    {
        [Required(ErrorMessage = "Item Name is required")]
        [StringLength(200, MinimumLength = 1, ErrorMessage = "Item Name must be between 1 and 200 characters")]
        public string ItemName { get; set; } = null!;

        [Range(0, int.MaxValue, ErrorMessage = "Starting Quantity must be 0 or greater")]
        public int StartingQuantity { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Current Quantity must be 0 or greater")]
        public int CurrentQuantity { get; set; }

        public List<string> CategoryCodes { get; set; } = new List<string>();
    }
}
