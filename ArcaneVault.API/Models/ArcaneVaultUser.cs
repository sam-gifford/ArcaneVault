using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArcaneVault.API.Models
{
    public class ArcaneVaultUser
    {
        [Key]
        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        public bool IsDeleted { get; set; }

        public int RoleId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public ArcaneVaultUserRole Role { get; set; } = null!;

        public ICollection<CollectionItem> CollectionItems { get; set; } = new List<CollectionItem>();
    }
}
