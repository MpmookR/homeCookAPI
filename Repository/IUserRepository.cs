using homeCookAPI.Models;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

public interface IUserRepository
{
    // Returns Full User Entities (For Internal Identity Operations)
    Task<ApplicationUser> GetByIdEntityAsync(string userId);
    Task<ApplicationUser> GetByEmailEntityAsync(string email);
    Task<IEnumerable<ApplicationUser>> GetAllEntitiesAsync();

    // Returns UserDTOs (For API Responses)
    Task<UserDTO> GetByIdAsync(string userId);
    Task<UserDTO> GetByEmailAsync(string email);
    Task<IEnumerable<UserDTO>> GetAllUsersWithRolesAsync(); 

    // Create & Delete Users
    Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
    Task<IdentityResult> DeleteAsync(ApplicationUser user);

    // Get User Roles
    Task<IList<string>> GetRolesAsync(ApplicationUser user);

    // Check If User Exists
    Task<bool> ExistsAsync(string userId);
}
