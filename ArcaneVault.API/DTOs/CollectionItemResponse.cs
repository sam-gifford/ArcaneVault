// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

// Name: [Your Name] | Admin No: [Your Admin No] | Tutorial Group: [Your Group]

namespace ArcaneVault.API.DTOs
{
    public class CollectionItemResponse
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = null!;
        public int StartingQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public string UserName { get; set; } = null!;
        public List<string> CategoryCodes { get; set; } = new List<string>();
    }
}
