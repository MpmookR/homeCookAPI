using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using homeCookAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace homeCookAPI.Controllers
{
    [Route("api/roles")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")] // Only SuperAdmin can manage roles
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public RolesController(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        // Get all roles (Super Admin only)
        [HttpGet]
        public IActionResult GetRoles()
        {
            var roles = _roleManager.Roles.ToList();
            return Ok(roles);
        }

        // Create a new role (Super Admin only)
        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            if (await _roleManager.RoleExistsAsync(roleName))
                return BadRequest(new { message = "Role already exists." });

            var role = new IdentityRole(roleName);
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
                return Ok(new { message = "Role created successfully." });

            return BadRequest(result.Errors);
        }

        // Assign a role to a user (Super Admin only)
        [HttpPost("assign-role-to-user")]
        public async Task<IActionResult> AssignRoleToUser([FromBody] AssignRoleDTO assignRoleDTO)
        {
            var user = await _userManager.FindByIdAsync(assignRoleDTO.UserId);
            if (user == null)
                return NotFound(new { message = "User not found." });

            var roleExists = await _roleManager.RoleExistsAsync(assignRoleDTO.RoleName);
            if (!roleExists)
                return NotFound(new { message = "Role not found." });

            var result = await _userManager.AddToRoleAsync(user, assignRoleDTO.RoleName);
            if (!result.Succeeded)
                return BadRequest(new { message = "Failed to assign role.", errors = result.Errors });

            return Ok(new AssignRoleDTO
            {
                UserId = assignRoleDTO.UserId,
                RoleName = assignRoleDTO.RoleName
            });
        }

    }
}
