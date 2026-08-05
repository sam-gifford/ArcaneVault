// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

// Name: [Your Name] | Admin No: [Your Admin No] | Tutorial Group: [Your Group]

using System.ComponentModel.DataAnnotations;

namespace ArcaneVault.API.DTOs
{
    public class RegisterRequest
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 100 characters")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Email must be a valid email address")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be between 6 and 100 characters")]
        public string Password { get; set; } = null!;
    }
}
