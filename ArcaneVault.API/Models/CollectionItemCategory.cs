// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault.API.Models
{
    // Join entity for many-to-many between CollectionItem and Category
    public class CollectionItemCategory
    {
        public int ItemId { get; set; }

        public string CategoryCode { get; set; } = null!;

        [ForeignKey(nameof(ItemId))]
        public CollectionItem Item { get; set; } = null!;

        [ForeignKey(nameof(CategoryCode))]
        public Category Category { get; set; } = null!;
    }
}
