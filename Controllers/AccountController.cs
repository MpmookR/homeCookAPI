using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using homeCookAPI.Models;
using homeCookAPI.Services; // Added to use EmailService
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

namespace homeCookAPI.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly EmailService _emailService; //Injected Email Service
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, EmailService emailService, IConfiguration configuration)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailService = emailService;
            _configuration = configuration;
        }

        // Register a new user with email verification
        // api/account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new ApplicationUser
            {
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // Generate Token
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

            // Print Debug Info to Terminal
            Console.WriteLine($"Generated Token (Before Encoding): {token}");

            // encode Token Before Sending
            var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
            Console.WriteLine($"Encoded Token (Sent in Email): {encodedToken}");

            var verificationLink = $"http://localhost:5057/api/account/verify-email?userId={user.Id}&token={encodedToken}";

            // Send Email
            await _emailService.SendEmailAsync(user.Email, "Verify Your Email",
                $"Welcome! Please verify your email by clicking {verificationLink}");

            return Ok(new { message = "User registered successfully! Please check your email for verification." });
        }


        // Verify Email
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string userId, string token)
        {
            Console.WriteLine($"User ID from Request: {userId}");
            Console.WriteLine($"Received Token (From URL): {token}");

            var decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));
            Console.WriteLine($"Decoded Token (Before Verification): {decodedToken}");

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                Console.WriteLine("User not found!");
                return NotFound(new { message = "User not found." });
            }

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
            if (!result.Succeeded)
            {
                Console.WriteLine("Email Verification Failed - Invalid Token");
                return BadRequest(new { message = "Email verification failed.", errors = result.Errors });
            }

            return Ok(new { message = "Email verified successfully! You can now log in." });
        }



        //Login User (Restricts login until email is verified

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            // ✅ Check if the email is verified before allowing login
            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized(new { message = "Email not verified. Please check your email for the verification link." });

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid login attempt" });

            // ✅ Generate JWT Token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(ClaimTypes.Email, user.Email)
        }),
                Expires = DateTime.UtcNow.AddHours(Convert.ToInt32(_configuration["Jwt:ExpireHours"])),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Issuer"],

                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                message = "Login successful!",
                token = tokenString,
                user = new
                {
                    user.Id,
                    user.FullName,
                    user.Email,
                    user.JoinDate
                }
            });
        }


        // Get all users
        [HttpGet("users")]
        public async Task<ActionResult<object>> GetUsers()
        {
            var users = await _userManager.Users
                .Select(u => new
                {
                    Id = u.Id,
                    FullName = u.FullName,
                    Email = u.Email,
                    JoinDate = u.JoinDate
                })
                .ToListAsync();

            return Ok(new { message = "Users retrieved successfully", users = users });
        }


        //Get user by ID
        [HttpGet("users/{id}")]
        public async Task<ActionResult<object>> GetUserById(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            return Ok(new
            {
                user.Id,
                user.FullName,
                user.Email,
                user.JoinDate
            });
        }

        // Log out user  
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "User successfully signed out" });
        }

        //Delete user by ID with improved error handling
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to delete user", errors = result.Errors });

            return Ok(new { message = "User deleted successfully!" });
        }
    }
}
