using Microsoft.AspNetCore.Mvc;
using homeCookAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace homeCookAPI.Controllers
{
    [Route("api/roles")]
    [ApiController]
    // [Authorize(Roles = "SuperAdmin")]  //only SuperAdmin can perform role feature
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IRoleService roleService, ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _logger = logger;
        }

        /// <summary>
        /// Retrieves all roles.
        /// </summary>
        /// <returns>A list of roles available in the system</returns>
        /// <response code="200">Returns the list of roles</response>
        [HttpGet]
        public async Task<IActionResult> GetRoles()
        {
            _logger.LogInformation("Fetching all roles...");
            var roles = await _roleService.GetAllRolesAsync();
            _logger.LogInformation("Successfully retrieved {Count} roles.", roles.Count);
            return Ok(roles);
        }

        /// <summary>
        /// Creates a new role.
        /// </summary>
        /// <param name="roleName">The name of the role to create</param>
        /// <returns>A confirmation message if the role is created</returns>
        /// <response code="200">Role created successfully</response>
        /// <response code="400">Invalid request or role creation failed</response>
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            _logger.LogInformation("Attempting to create role: {RoleName}", roleName);
            var result = await _roleService.CreateRoleAsync(roleName);

            if (result.Succeeded)
            {
                _logger.LogInformation("Role '{RoleName}' created successfully.", roleName);
                return Ok(new { message = "Role created successfully." });
            }

            _logger.LogError("Failed to create role '{RoleName}'. Errors: {Errors}", roleName, result.Errors);
            return BadRequest(result.Errors);
        }

        /// <summary>
        /// Assigns a role to a user.
        /// </summary>
        /// <param name="assignRoleDTO">The user ID and role name to assign</param>
        /// <returns>A confirmation message if the role is assigned</returns>
        /// <response code="200">Role assigned successfully</response>
        /// <response code="400">Invalid request or role assignment failed</response>
        [HttpPost("assign-role-to-user")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRole assignRoleDTO)
        {
            _logger.LogInformation("Assigning role '{RoleName}' to User ID {UserId}.", assignRoleDTO.RoleName, assignRoleDTO.UserId);

            var success = await _roleService.AssignRoleToUserAsync(assignRoleDTO.UserId, assignRoleDTO.RoleName);
            if (!success)
            {
                _logger.LogWarning("Failed to assign role '{RoleName}' to User ID {UserId}.", assignRoleDTO.RoleName, assignRoleDTO.UserId);
                return BadRequest(new { message = "Failed to assign role." });
            }

            _logger.LogInformation("Successfully assigned role '{RoleName}' to User ID {UserId}.", assignRoleDTO.RoleName, assignRoleDTO.UserId);
            return Ok(new { message = $"Role '{assignRoleDTO.RoleName}' assigned successfully." });
        }
        
        /// <summary>
        /// Deletes a role by name.
        /// </summary>
        /// <param name="roleName">The name of the role to delete.</param>
        /// <returns>A confirmation message if the role is deleted.</returns>
        /// <response code="200">Role deleted successfully</response>
        /// <response code="400">Invalid request or deletion failed</response>
        [HttpDelete("{roleName}")]
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            _logger.LogInformation("Attempting to delete role: {RoleName}", roleName);

            var success = await _roleService.DeleteRoleAsync(roleName);
            if (!success)
            {
                _logger.LogWarning("Failed to delete role '{RoleName}'.", roleName);
                return BadRequest(new { message = "Failed to delete role." });
            }

            _logger.LogInformation("Successfully deleted role '{RoleName}'.", roleName);
            return Ok(new { message = $"Role '{roleName}' deleted successfully." });
        }
    }
}
