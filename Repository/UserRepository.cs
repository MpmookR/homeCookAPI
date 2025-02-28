using homeCookAPI.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

public class UserRepository : IUserRepository
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UserRepository(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // Methods returning ApplicationUser (needed for Identity)
    public async Task<ApplicationUser> GetByIdEntityAsync(string userId) =>
        await _userManager.FindByIdAsync(userId);

    public async Task<ApplicationUser> GetByEmailEntityAsync(string email) =>
        await _userManager.FindByEmailAsync(email);

    // Methods returning UserDTO (API responses)
    public async Task<UserDTO> GetByIdAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            JoinDate = user.JoinDate,
            Roles = roles.ToList()
        };
    }

    public async Task<IEnumerable<ApplicationUser>> GetAllEntitiesAsync()
    {
        return await _userManager.Users.ToListAsync();
    }


    public async Task<UserDTO> GetByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return new UserDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            JoinDate = user.JoinDate,
            Roles = roles.ToList()
        };
    }

    public async Task<IEnumerable<UserDTO>> GetAllUsersWithRolesAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        var userDTOs = new List<UserDTO>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDTOs.Add(new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                JoinDate = user.JoinDate,
                Roles = roles.ToList()
            });
        }

        return userDTOs;
    }

    public async Task<IdentityResult> CreateAsync(ApplicationUser user, string password) =>
        await _userManager.CreateAsync(user, password);

    public async Task<IdentityResult> DeleteAsync(ApplicationUser user) =>
        await _userManager.DeleteAsync(user);

    public async Task<IList<string>> GetRolesAsync(ApplicationUser user) =>
        await _userManager.GetRolesAsync(user);

    public async Task<bool> ExistsAsync(string userId)
    {
        return await _userManager.FindByIdAsync(userId) != null;
    }
}

// abstraction layer between the database and the service