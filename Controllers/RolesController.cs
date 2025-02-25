using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using homeCookAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging; 

namespace homeCookAPI.Controllers
{
    [Route("api/roles")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")] 
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ILogger<RolesController> _logger; 

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager, ILogger<RolesController> logger)
        {
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult GetRoles()
        {
            _logger.LogInformation("Fetching all roles...");

            var roles = _roleManager.Roles.ToList();

            _logger.LogInformation("Successfully retrieved {Count} roles.", roles.Count);
            return Ok(roles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            _logger.LogInformation("Attempting to create role: {RoleName}", roleName);

            if (await _roleManager.RoleExistsAsync(roleName))
            {
                _logger.LogWarning("Role '{RoleName}' already exists.", roleName);
                return BadRequest(new { message = "Role already exists." });
            }

            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
            {
                _logger.LogInformation("Role '{RoleName}' created successfully.", roleName);
                return Ok(new { message = "Role created successfully." });
            }

            _logger.LogError("Failed to create role '{RoleName}'. Errors: {Errors}", roleName, result.Errors);
            return BadRequest(result.Errors);
        }

        [HttpPost("assign-role-to-user")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleDTO assignRoleDTO)
        {
            _logger.LogInformation("Assigning role '{RoleName}' to User ID {UserId}.", assignRoleDTO.RoleName, assignRoleDTO.UserId);

            var user = await _userManager.FindByIdAsync(assignRoleDTO.UserId);
            if (user == null)
            {
                _logger.LogWarning("Failed to assign role - User ID {UserId} not found.", assignRoleDTO.UserId);
                return NotFound(new { message = "User not found." });
            }

            var roleExists = await _roleManager.RoleExistsAsync(assignRoleDTO.RoleName);
            if (!roleExists)
            {
                _logger.LogWarning("Failed to assign role - Role '{RoleName}' not found.", assignRoleDTO.RoleName);
                return NotFound(new { message = "Role not found." });
            }

            var result = await _userManager.AddToRoleAsync(user, assignRoleDTO.RoleName);
            if (!result.Succeeded)
            {
                _logger.LogError("Failed to assign role '{RoleName}' to User ID {UserId}. Errors: {Errors}", assignRoleDTO.RoleName, assignRoleDTO.UserId, result.Errors);
                return BadRequest(new { message = "Failed to assign role.", errors = result.Errors });
            }

            _logger.LogInformation("Successfully assigned role '{RoleName}' to User ID {UserId}.", assignRoleDTO.RoleName, assignRoleDTO.UserId);
            return Ok(new AssignRoleDTO
            {
                UserId = assignRoleDTO.UserId,
                RoleName = assignRoleDTO.RoleName
            });
        }
    }
}
