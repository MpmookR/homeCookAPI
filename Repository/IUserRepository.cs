using homeCookAPI.Models;
using Microsoft.AspNetCore.Identity;

public interface IUserRepository
{
    // return ApplicationUser because Identity operations require the full entity
    Task<ApplicationUser> GetByIdEntityAsync(string userId);
    Task<ApplicationUser> GetByEmailEntityAsync(string email);

    // return UserDTO for API responses
    Task<UserDTO> GetByIdAsync(string userId);
    Task<UserDTO> GetByEmailAsync(string email);
    Task<IEnumerable<UserDTO>> GetAllAsync();
    
    Task<IdentityResult> CreateAsync(ApplicationUser user, string password);
    Task<IdentityResult> DeleteAsync(ApplicationUser user);
    Task<IList<string>> GetRolesAsync(ApplicationUser user);
    Task<bool> ExistsAsync(String userId);
}



//data access logic for ApplicationUser