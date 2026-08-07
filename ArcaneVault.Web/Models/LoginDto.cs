// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

namespace ArcaneVault.Web.Models
{
    public class LoginDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Role { get; set; } = null!;
        public string Token { get; set; } = null!;
        public DateTime TokenExpiration { get; set; }
    }
}
