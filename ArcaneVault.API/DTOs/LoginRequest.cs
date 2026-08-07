// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

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
