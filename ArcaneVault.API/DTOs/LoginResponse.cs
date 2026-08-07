// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

namespace ArcaneVault.API.DTOs
{
    public class LoginResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? Role { get; set; }
        public string? Token { get; set; }
        public DateTime? TokenExpiration { get; set; }
    }
}
