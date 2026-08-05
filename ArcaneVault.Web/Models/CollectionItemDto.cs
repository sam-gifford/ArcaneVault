// Name: [Your Name] | Admin No: [Your Admin No] | Tutorial Group: [Your Group]

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
