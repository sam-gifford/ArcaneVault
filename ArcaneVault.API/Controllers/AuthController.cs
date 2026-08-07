// Name: Gifford | Admin No: 252266P | Tutorial Group: IT2814-06

using ArcaneVault.API.Data;
using ArcaneVault.API.DTOs;
using ArcaneVault.API.Models;
using ArcaneVault.API.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ArcaneVault.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ArcaneVaultDbContext _context;
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;

        public AuthController(
            ArcaneVaultDbContext context,
            ILogger<AuthController> logger,
            IConfiguration configuration)
        {
            _context = context;
            _logger = logger;
            _configuration = configuration;
        }

        /// <summary>
        /// Register a new user with the User role.
        /// Includes asynchronous validation to check for duplicate emails before creating account.
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
            var normalizedUserName = request.UserName.Trim();
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var existingUser = await _context.ArcaneVaultUsers
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.UserName.ToLower() == normalizedUserName.ToLower());
            if (existingUser != null)
            {
                _logger.LogWarning($"Registration attempt with duplicate username: {request.UserName}");
                return BadRequest(new RegisterResponse
                {
                    Success = false,
                    Message = "Username already exists"
                });
            }

            // Asynchronously check if email already exists (async validation)
            var emailExists = await _context.ArcaneVaultUsers
                .IgnoreQueryFilters()
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail);
            if (emailExists)
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
                // Create new user with User role (RoleId = 2)
                var newUser = new ArcaneVaultUser
                {
                    UserName = normalizedUserName,
                    Email = normalizedEmail,
                    PasswordHash = PasswordService.HashPassword(request.Password),
                    RoleId = 2, // User role
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
        /// Login a user. Validates username and password, returns user info with role.
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
                // Find user by username (soft-deleted users excluded via global query filter)
                var normalizedUserName = request.UserName.Trim().ToLowerInvariant();
                var user = await _context.ArcaneVaultUsers
                    .Include(u => u.Role)
                    .FirstOrDefaultAsync(u => u.UserName.ToLower() == normalizedUserName);

                if (user == null || !PasswordService.VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning($"Login attempt with invalid credentials: {request.UserName}");
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    });
                }

                var expiration = DateTime.UtcNow.AddHours(
                    _configuration.GetValue<int?>("Jwt:ExpiryHours") ?? 3);
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role.RoleName)
                };
                var signingKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: expiration,
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256));

                _logger.LogInformation($"User logged in successfully: {user.UserName}");

                return Ok(new LoginResponse
                {
                    Success = true,
                    Message = "Login successful",
                    UserName = user.UserName,
                    Email = user.Email,
                    Role = user.Role.RoleName,
                    Token = new JwtSecurityTokenHandler().WriteToken(token),
                    TokenExpiration = expiration
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
