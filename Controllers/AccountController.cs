using Microsoft.AspNetCore.Mvc;
using homeCookAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace homeCookAPI.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly IAccountService _accountService;
        private readonly ILogger<AccountController> _logger;

        public AccountController(IAccountService accountService, ILogger<AccountController> logger)
        {
            _accountService = accountService;
            _logger = logger;
        }

        // Register a new user with email verification
        // api/account/register
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            _logger.LogInformation("User registration attempt: {Email}", model.Email);

            var result = await _accountService.RegisterAsync(model);
            if (!result.Succeeded)
            {
                _logger.LogError("User registration failed: {Email}", model.Email);
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "User registered successfully! Please check your email for verification." });
        }


        // Verify Email
        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string userId, string token)
        {
            _logger.LogInformation("Email verification attempt for User ID: {UserId}", userId);

            var success = await _accountService.VerifyEmailAsync(userId, token);
            if (!success)
            {
                _logger.LogError("Email verification failed for User ID: {UserId}", userId);
                return BadRequest(new { message = "Email verification failed." });
            }

            _logger.LogInformation("Email verified successfully for User ID: {UserId}", userId);
            return Ok(new { message = "Email verified successfully! You can now log in." });
        }

        // Restricts login until email is verified
        // api/account/login
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            _logger.LogInformation("Login attempt for email: {Email}", model.Email);

            var token = await _accountService.LoginAsync(model);
            if (token == null)
            {
                _logger.LogWarning("Login failed for email: {Email}", model.Email);
                return Unauthorized(new { message = "Invalid credentials or email not verified." });
            }

            _logger.LogInformation("User {Email} logged in successfully", model.Email);
            return Ok(new { message = "Login successful!", token });
        }

        // Get all users (Uses DTO)
        // api/account/users  >> header: Authorization: Bearer {JWT_TOKEN}
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _accountService.GetUsersAsync();
            return Ok(users);
        }

        // Get user by ID (Uses DTO)
        // api/account/users/{USER_ID}
        [HttpGet("users/{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(string id)
        {
            try
            {
                var user = await _accountService.GetUserByIdAsync(id);
                return Ok(user);
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogWarning(ex.Message);
                return NotFound(new { message = ex.Message });
            }
        }

        // Report User only admin and user
        // api/account/report-user
        [Authorize]
        [HttpPost("report-user")]
        public async Task<IActionResult> ReportUser([FromBody] ReportUserModel model)
        {
            var message = await _accountService.ReportUserAsync(model.UserId);
            if (message == null)
            {
                _logger.LogWarning("Report failed - User not found: {UserId}", model.UserId);
                return NotFound(new { message = "User not found." });
            }

            return Ok(new { message });
        }

        // api/account/logout
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("User logout requested.");
            await _accountService.LogoutAsync();
            _logger.LogInformation("User successfully logged out.");
            return Ok(new { message = "User successfully signed out" });
        }

        // Delete user by ID (SuperAdmin only)
        // api/account/users/{USER_ID} >> header: Authorization: Bearer {JWT_TOKEN}
        [Authorize(Roles = "SuperAdmin")]
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var success = await _accountService.DeleteUserAsync(id);
            if (!success)
            {
                _logger.LogWarning("Delete failed - User not found: {UserId}", id);
                return NotFound(new { message = "User not found." });
            }

            _logger.LogInformation("User deleted successfully: {UserId}", id);
            return Ok(new { message = "User deleted successfully!" });
        }
    }
}
