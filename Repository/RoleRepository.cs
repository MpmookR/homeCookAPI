using Microsoft.AspNetCore.Identity;
using homeCookAPI.Models;

public class RoleRepository : IRoleRepository
{
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;

    public RoleRepository(RoleManager<IdentityRole> roleManager, UserManager<ApplicationUser> userManager)
    {
        _roleManager = roleManager;
        _userManager = userManager;
    }

    public async Task<List<IdentityRole>> GetAllRolesAsync()
    {
        return await Task.FromResult(_roleManager.Roles.ToList());
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await _roleManager.RoleExistsAsync(roleName);
    }

    public async Task<IdentityResult> CreateRoleAsync(IdentityRole role)
    {
        return await _roleManager.CreateAsync(role);
    }

    public async Task<bool> AssignRoleToUserAsync(string userId, string roleName)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return false;

        var roleExists = await _roleManager.RoleExistsAsync(roleName);
        if (!roleExists) return false;

        var result = await _userManager.AddToRoleAsync(user, roleName);
        return result.Succeeded;
    }

    public async Task<bool> DeleteRoleAsync(string roleName) 
    {
        var role = await _roleManager.FindByNameAsync(roleName);
        if (role == null) return false;

        var result = await _roleManager.DeleteAsync(role);
        return result.Succeeded;
    }

}
