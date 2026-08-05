// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

namespace ArcaneVault.Web.Models
{
    public class CollectionItemDto
    {
        public int ItemId { get; set; }
        public string ItemName { get; set; } = null!;
        public int StartingQuantity { get; set; }
        public int CurrentQuantity { get; set; }
        public string UserName { get; set; } = null!;
        public List<string> CategoryCodes { get; set; } = new List<string>();
    }
}
