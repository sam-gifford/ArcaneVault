// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.API.Models
{
    public class Category
    {
        [Key]
        [Required]
        public string CategoryCode { get; set; } = null!;

        [Required]
        public string CategoryName { get; set; } = null!;

        public ICollection<CollectionItemCategory> CollectionItemCategories { get; set; } = new List<CollectionItemCategory>();
    }
}
