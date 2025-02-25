using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using homeCookAPI.Models;
using homeCookAPI.Services; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace homeCookAPI.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, EmailService emailService, IConfiguration configuration, ILogger<AccountController> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _configuration = configuration;
             _logger = logger; 
        }

        // Register a new user with email verification
        // api/account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            _logger.LogInformation("User registration attempt: {Email}", model.Email);

            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                _logger.LogError("User registration failed: {Email}", model.Email);
                return BadRequest(result.Errors);
            }

            // Generate Token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
            var verificationLink = $"http://localhost:5057/api/account/verify-email?userId={user.Id}&token={encodedToken}";

            // Send Email
            await _emailService.SendEmailAsync(user.Email, "Verify Your Email",
                $"Welcome! Please verify your email by clicking {verificationLink}");

            _logger.LogInformation("User registered successfully: {Email}", model.Email);

            return Ok(new { message = "User registered successfully! Please check your email for verification." });
        }

        // Verify Email
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string userId, string token)
        {
            _logger.LogInformation("Email verification attempt for User ID: {UserId}", userId);

            var decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                _logger.LogWarning("Email verification failed - User not found: {UserId}", userId);
                return NotFound(new { message = "User not found." });
            }

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
            {
                _logger.LogError("Email verification failed - Invalid Token for User ID: {UserId}", userId);

                return BadRequest(new { message = "Email verification failed.", errors = result.Errors });
            }
            
            _logger.LogInformation("Email verified successfully for User ID: {UserId}", userId);
            return Ok(new { message = "Email verified successfully! You can now log in." });
        }

        // Restricts login until email is verified
        // api/account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            _logger.LogInformation("Login attempt for email: {Email}", model.Email); //Log login attempt

            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)  
            {     
                _logger.LogWarning("Login failed for email: {Email} - User not found", model.Email);
                return Unauthorized(new { message = "Invalid credentials" });
            }

            if (!await _userManager.IsEmailConfirmedAsync(user))
            {
            
                _logger.LogWarning("Login attempt with unverified email: {Email}", model.Email);
                return Unauthorized(new { message = "Email not verified. Please check your email for the verification link." });
            }

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded)
            {
                _logger.LogWarning("Invalid login attempt for email: {Email}", model.Email);
                return Unauthorized(new { message = "Invalid login attempt" });
            }

            var userRoles = await _userManager.GetRolesAsync(user);
            var userRole = userRoles.FirstOrDefault() ?? "User";

            // Generate JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id),
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, userRole) // Includes user role in JWT
                }),
                Expires = DateTime.UtcNow.AddHours(Convert.ToInt32(_configuration["Jwt:ExpireHours"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Issuer"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation("User {Email} logged in successfully", model.Email);
            return Ok(new
            {
                message = "Login successful!",
                token = tokenString,
                user = new UserDTO
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    JoinDate = user.JoinDate
                }
            });
        }

        // Get all users (Uses DTO)
        // api/account/users  >> header: Authorization: Bearer {JWT_TOKEN}
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _userManager.Users
                .Select(u => new UserDTO
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    JoinDate = u.JoinDate
                })
                .ToListAsync();

            return Ok(users);
        }

        // Get user by ID (Uses DTO)
        // api/account/users/{USER_ID}
        [HttpGet("users/{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                JoinDate = user.JoinDate
            });
        }

        // Report User only admin and user
        // api/account/report-user
        [Authorize]
        [HttpPost("report-user")]
        public async Task<IActionResult> ReportUser([FromBody] ReportUserModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            return Ok(new { message = $"User {model.UserId} has been reported to the Super Admin." });
        }

        // api/account/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "User successfully signed out" });
        }

        // Delete user by ID (SuperAdmin only)
        // api/account/users/{USER_ID} >> header: Authorization: Bearer {JWT_TOKEN}
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to delete user", errors = result.Errors });

            return Ok(new { message = "User deleted successfully!" });
        }
    }
}
