using Microsoft.AspNetCore.Identity;

public interface IRoleRepository
{
    Task<List<IdentityRole>> GetAllRolesAsync();
    Task<bool> RoleExistsAsync(string roleName);
    Task<IdentityResult> CreateRoleAsync(IdentityRole role);
    Task<bool> AssignRoleToUserAsync(string userId, string roleName);
    Task<bool> DeleteRoleAsync(string newRole); 

}
