using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.API.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; } = null!;
    }
}
