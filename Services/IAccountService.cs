using homeCookAPI.Models;
using Microsoft.AspNetCore.Identity;

public interface IAccountService
{
    Task<IdentityResult> RegisterAsync(RegisterModel model);
    Task<bool> VerifyEmailAsync(string userId, string token);
    Task<string> LoginAsync(LoginModel model);
    Task<IEnumerable<UserDTO>> GetUsersAsync();
    Task<UserDTO> GetUserByIdAsync(string id);
    Task<string> ReportUserAsync(string userId);
    Task<bool> DeleteUserAsync(string id);
    Task LogoutAsync();
}

//business logic related to account management