// Name: [Your Name] | Admin No: [Your Admin No] | Tutorial Group: [Your Group]

using ArcaneVault.API.Data;
using ArcaneVault.API.DTOs;
using ArcaneVault.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;

namespace ArcaneVault.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ArcaneVaultDbContext _context;
        private readonly ILogger<AuthController> _logger;

        public AuthController(ArcaneVaultDbContext context, ILogger<AuthController> logger)
        {
            _context = context;
            _logger = logger;
        }

        /// <summary>
        /// Hash password using SHA256
        /// </summary>
        private string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var hashedBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
                return Convert.ToBase64String(hashedBytes);
            }
        }

        /// <summary>
        /// Verify password against stored hash
        /// </summary>
        private bool VerifyPassword(string password, string hash)
        {
            var hashOfInput = HashPassword(password);
            return hashOfInput == hash;
        }

        /// <summary>
        /// Register a new user with the User role
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponse>> Register([FromBody] RegisterRequest request)
        {
            // Validate request
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = string.Join("; ", errors)
                });
            }

            // Check if username already exists
            var existingUser = await _context.ArcaneVaultUsers
                .FirstOrDefaultAsync(u => u.UserName == request.UserName);
            if (existingUser != null)
            {
                _logger.LogWarning($"Registration attempt with duplicate username: {request.UserName}");
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Username already exists"
                });
            }

            // Check if email already exists
            var emailExists = await _context.ArcaneVaultUsers
                .FirstOrDefaultAsync(u => u.Email == request.Email);
            if (emailExists != null)
            {
                _logger.LogWarning($"Registration attempt with duplicate email: {request.Email}");
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Email already exists"
                });
            }

            try
            {
                // Create new user with User role (RoleId = 1)
                var newUser = new ArcaneVaultUser
                {
                    UserName = request.UserName,
                    Email = request.Email,
                    PasswordHash = HashPassword(request.Password),
                    RoleId = 1, // User role
                    IsDeleted = false
                };

                _context.ArcaneVaultUsers.Add(newUser);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"User registered successfully: {request.UserName}");

                return Ok(new RegisterResponse
                {
                    Success = true,
                    Message = "User registered successfully",
                    UserName = newUser.UserName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during registration: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new RegisterResponse
                    {
                        Success = false,
                        Message = "An error occurred during registration"
                    });
            }
        }

        /// <summary>
        /// Login a user
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
        {
            // Validate request
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                return BadRequest(new LoginResponse
                {
                    Success = false,
                    Message = string.Join("; ", errors)
                });
            }

            try
            {
                // Find user by username
                var user = await _context.ArcaneVaultUsers
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserName == request.UserName && !u.IsDeleted);

                if (user == null || !VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning($"Login attempt with invalid credentials: {request.UserName}");
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    });
                }

                _logger.LogInformation($"User logged in successfully: {request.UserName}");

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Login successful",
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = user.Role?.RoleName
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during login: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError,
                    new LoginResponse
                    {
                        Success = false,
                        Message = "An error occurred during login"
                    });
            }
        }
    }
}
