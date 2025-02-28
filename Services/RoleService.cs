using Microsoft.AspNetCore.Identity;

public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;

    public RoleService(IRoleRepository roleRepository)
    {
        _roleRepository = roleRepository;
    }

    public async Task<List<IdentityRole>> GetAllRolesAsync()
    {
        return await _roleRepository.GetAllRolesAsync();
    }

    public async Task<bool> RoleExistsAsync(string roleName)
    {
        return await _roleRepository.RoleExistsAsync(roleName);
    }

    public async Task<IdentityResult> CreateRoleAsync(string roleName)
    {
        if (await _roleRepository.RoleExistsAsync(roleName))
        {
            return IdentityResult.Failed(new IdentityError { Description = "Role already exists." });
        }

        var role = new IdentityRole(roleName);
        return await _roleRepository.CreateRoleAsync(role);
    }

    public async Task<bool> AssignRoleToUserAsync(string userId, string roleName)
    {
        return await _roleRepository.AssignRoleToUserAsync(userId, roleName);
    }

    public async Task<bool> DeleteRoleAsync(string roleName) 
    {
        return await _roleRepository.DeleteRoleAsync(roleName);
    }
}
