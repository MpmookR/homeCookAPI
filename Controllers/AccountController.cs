using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using homeCookAPI.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace homeCookAPI.Controllers
{
    [Route("api/account")]
    [ApiController]
    public class AccountController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // ✅ Register a new user
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            var user = new ApplicationUser
            {
                Id = ApplicationUser.GenerateUserId(), // Explicitly set the custom User ID
                UserName = model.Email,
                Email = model.Email,
                FullName = model.FullName
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "User registered successfully!" });
        }

        // Login a user
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
                return Unauthorized(new { message = "Invalid credentials" });

            var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);

            if (!result.Succeeded)
                return Unauthorized(new { message = "Invalid login attempt" });

            return Ok(new { message = "Login successful!" });
        }

        // Get all users
        [HttpGet("users")]
        public ActionResult<IEnumerable<object>> GetUsers()
        {
            var users = _userManager.Users.Select(u => new
            {
                u.Id,
                u.FullName,
                u.Email,
                u.JoinDate
            }).ToList();

            return Ok(users);
        }

        // ✅ Get user by ID
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

        // // Update user details (FullName and Email)
        // [HttpPut("users/{id}")]
        // public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserModel model)
        // {
        //     var user = await _userManager.FindByIdAsync(id);
        //     if (user == null)
        //         return NotFound(new { message = "User not found" });

        //     // Update user details
        //     user.FullName = model.FullName ?? user.FullName;
        //     user.Email = model.Email ?? user.Email;
        //     user.UserName = model.Email ?? user.UserName; // Important for Identity

        //     var result = await _userManager.UpdateAsync(user);
        //     if (!result.Succeeded)
        //         return BadRequest(result.Errors);

        //     return Ok(new { message = "User updated successfully!" });
        // }

        // Delete user by ID
        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null)
                return NotFound(new { message = "User not found" });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            return Ok(new { message = "User deleted successfully!" });
        }
    }
}
