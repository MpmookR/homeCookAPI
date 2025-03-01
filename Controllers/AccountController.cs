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

        /// <summary>
        /// Registers a new user and sends an email verification
        /// </summary>
        /// <param name="model">User registration details</param>
        /// <returns>A success message if registration is completed</returns>
        /// <response code="200">User registered successfully</response>
        /// <response code="400">Invalid registration request</response>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            _logger.LogInformation("User registration attempt: {Email}", model.Email);

            var result = await _accountService.RegisterAsync(model); // Now it matches the class name
            if (!result.Succeeded)
            {
                _logger.LogError("User registration failed: {Email}", model.Email);
                return BadRequest(result.Errors);
            }

            return Ok(new { message = "User registered successfully! Please check your email for verification." });
        }

        /// <summary>
        /// Verifies a user's email address
        /// </summary>
        /// <param name="userId">The ID of the user</param>
        /// <param name="token">The verification token</param>
        /// <returns>A success message if the email is verified</returns>
        /// <response code="200">Email verified successfully</response>
        /// <response code="400">Invalid or expired token</response>
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

        /// <summary>
        /// Authenticates a user and returns a JWT token
        /// </summary>
        /// <param name="model">User login credentials</param>
        /// <returns>A JWT token if authentication is successful</returns>
        /// <response code="200">Login successful</response>
        /// <response code="401">Invalid credentials or email not verified</response>
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

        /// <summary>
        /// Retrieves all registered users
        /// </summary>
        /// <returns>A list of all users</returns>
        /// <response code="200">Returns the list of users</response>
        [Authorize]
        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserDTO>>> GetUsers()
        {
            var users = await _accountService.GetUsersAsync();
            return Ok(users);
        }

        /// <summary>
        /// Retrieves a specific user by ID
        /// </summary>
        /// <param name="id">The ID of the user</param>
        /// <returns>User details if found</returns>
        /// <response code="200">Returns the user</response>
        /// <response code="404">User not found</response>
        [Authorize]
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

        /// <summary>
        /// Reports a user for misconduct
        /// </summary>
        /// <param name="model">The user ID being reported</param>
        /// <returns>A confirmation message if the report is submitted</returns>
        /// <response code="200">User reported successfully</response>
        /// <response code="404">User not found</response>
        [Authorize]
        [HttpPost("report-user")]
        public async Task<IActionResult> ReportUser([FromBody] ReportUser model)
        {
            var message = await _accountService.ReportUserAsync(model.UserId);
            if (message == null)
            {
                _logger.LogWarning("Report failed - User not found: {UserId}", model.UserId);
                return NotFound(new { message = "User not found." });
            }

            return Ok(new { message });
        }

        /// <summary>
        /// Logs out the current user
        /// </summary>
        /// <returns>A success message if logout is completed</returns>
        /// <response code="200">User successfully logged out</response>
        [HttpPost("logout")]      
        public async Task<IActionResult> Logout()
        {
            _logger.LogInformation("User logout requested.");
            await _accountService.LogoutAsync();
            _logger.LogInformation("User successfully logged out.");
            return Ok(new { message = "User successfully signed out" });
        }

        /// <summary>
        /// Deletes a user account (SuperAdmin only)
        /// </summary>
        /// <param name="id">The ID of the user to delete</param>
        /// <returns>A success message if deletion is completed</returns>
        /// <response code="200">User deleted successfully</response>
        /// <response code="404">User not found</response>
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
