// Name: [Your Name] | Admin No: [Your Admin No] | Tutorial Group: [Your Group]

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.API.Models
{
    public class ArcaneVaultUserRole
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        public string RoleName { get; set; } = null!;

        public ICollection<ArcaneVaultUser> Users { get; set; } = new List<ArcaneVaultUser>();
    }
}
