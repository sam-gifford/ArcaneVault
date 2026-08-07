// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault.API.Models
{
    public class CollectionItem
    {
        [Key]
        public int ItemId { get; set; }

        [Required]
        public string ItemName { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public int StartingQuantity { get; set; }

        public int CurrentQuantity { get; set; }

        [Required]
        public string UserName { get; set; } = null!;

        [ForeignKey(nameof(UserName))]
        public ArcaneVaultUser User { get; set; } = null!;

        public ICollection<CollectionItemCategory> CollectionItemCategories { get; set; } = new List<CollectionItemCategory>();
    }
}
