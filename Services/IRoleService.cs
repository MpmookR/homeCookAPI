using Microsoft.AspNetCore.Identity;

public interface IRoleService
{
    Task<List<IdentityRole>> GetAllRolesAsync();
    Task<bool> RoleExistsAsync(string roleName);
    Task<IdentityResult> CreateRoleAsync(string roleName);
    Task<bool> AssignRoleToUserAsync(string userId, string roleName);
    Task<bool> DeleteRoleAsync(string roleName);

}
