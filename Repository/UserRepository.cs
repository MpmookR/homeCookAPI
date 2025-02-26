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
        return user == null ? null : MapToDTO(user);
    }

    public async Task<UserDTO> GetByEmailAsync(string email)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user == null ? null : MapToDTO(user);
    }

    public async Task<IEnumerable<UserDTO>> GetAllAsync()
    {
        var users = await _userManager.Users.ToListAsync();
        return users.Select(user => MapToDTO(user));
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

    private static UserDTO MapToDTO(ApplicationUser user)
    {
        return new UserDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            JoinDate = user.JoinDate
        };
    }
}



// abstraction layer between the database and the service