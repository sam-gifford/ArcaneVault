// Name: [Your Name] | Admin No: [Your Admin No] | Tutorial Group: [Your Group]

using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.API.DTOs
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username is required")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; } = null!;
    }
}
