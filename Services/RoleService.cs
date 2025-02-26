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

    public async Task<bool> ChangeUserRoleAsync(string userId, string newRole)
    {
        if (!await _roleRepository.RoleExistsAsync(newRole))
            throw new KeyNotFoundException($"Role '{newRole}' does not exist.");

        return await _roleRepository.ChangeUserRoleAsync(userId, newRole);
    }


    public async Task<bool> AssignRoleToUserAsync(string userId, string roleName)
    {
        return await _roleRepository.AssignRoleToUserAsync(userId, roleName);
    }
}
