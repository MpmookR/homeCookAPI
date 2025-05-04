using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using homeCookAPI.Models;
using homeCookAPI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

public class AccountService : IAccountService
{
    private readonly IUserRepository _userRepository;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly EmailService _emailService;
    private readonly IConfiguration _configuration;

    public AccountService(IUserRepository userRepository, SignInManager<ApplicationUser> signInManager,
                          EmailService emailService, IConfiguration configuration)
    {
        _userRepository = userRepository;
        _signInManager = signInManager;
        _emailService = emailService;
        _configuration = configuration;
    }

    // Register a new user
    public async Task<IdentityResult> RegisterAsync(RegisterModel model)
    {

        var user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            FullName = model.FullName
        };

        var result = await _userRepository.CreateAsync(user, model.Password);
        if (!result.Succeeded) return result;

        // Generate Token
        var token = await _signInManager.UserManager.GenerateEmailConfirmationTokenAsync(user);
        var encodedToken = Convert.ToBase64String(Encoding.UTF8.GetBytes(token));
        //local test
        // var verificationLink = $"http://localhost:5057/api/account/verify-email?userId={user.Id}&token={encodedToken}";

        //deploy with render
        var baseUrl = Environment.GetEnvironmentVariable("APP_BASE_URL") ?? "http://localhost:5057";
        var verificationLink = $"{baseUrl}/api/account/verify-email?userId={user.Id}&token={Uri.EscapeDataString(encodedToken)}";

        // Send Email
        await _emailService.SendEmailAsync(user.Email, "Verify Your Email",
            $"Welcome to Recipe Sharing! Please verify your email by clicking {verificationLink}");
        return result;
    }

    // Verify email confirmation
    public async Task<bool> VerifyEmailAsync(string userId, string token)
    {

        var user = await _userRepository.GetByIdEntityAsync(userId);
        if (user == null)
            return false;

        var decodedToken = Encoding.UTF8.GetString(Convert.FromBase64String(token));
        var result = await _signInManager.UserManager.ConfirmEmailAsync(user, decodedToken);
        return result.Succeeded;
    }

    // Login and return JWT token
    public async Task<string> LoginAsync(LoginModel model)
    {
        var user = await _userRepository.GetByEmailEntityAsync(model.Email);
        if (user == null || !await _signInManager.UserManager.IsEmailConfirmedAsync(user)) return null;

        var result = await _signInManager.PasswordSignInAsync(user, model.Password, false, false);
        if (!result.Succeeded) return null;

        var userRoles = await _userRepository.GetRolesAsync(user);
        var userRole = userRoles.FirstOrDefault() ?? "User";

        var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]);
        var tokenHandler = new JwtSecurityTokenHandler();
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, userRole)
            }),
            Expires = DateTime.UtcNow.AddHours(Convert.ToInt32(_configuration["Jwt:ExpireHours"])),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Issuer"],
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // Get all users
    public async Task<IEnumerable<UserDTO>> GetUsersAsync()
    {
        var users = await _userRepository.GetAllEntitiesAsync();
        var userDTOs = new List<UserDTO>();

        foreach (var user in users)
        {
            var roles = await _userRepository.GetRolesAsync(user);
            userDTOs.Add(new UserDTO
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                JoinDate = user.JoinDate,
                Roles = roles.ToList(),
                ProfileImage = user.ProfileImage,
                TotalRecipes = user.Recipes.Count,
                TotalLikes = user.Likes.Count,
                TotalSavedRecipes = user.SavedRecipes?.Count ?? 0
            });
        }

        return userDTOs;
    }

    // Get a single user as UserDTO
    public async Task<UserDTO> GetUserByIdAsync(string id)
    {
        var user = await _userRepository.GetByIdEntityAsync(id);
        if (user == null) throw new KeyNotFoundException($"User with ID {id} not found.");

        var roles = await _userRepository.GetRolesAsync(user);

        return new UserDTO
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            JoinDate = user.JoinDate,
            Roles = roles.ToList(),
            ProfileImage = user.ProfileImage,
            TotalRecipes = user.Recipes.Count,         
            TotalLikes = user.Likes.Count,
            TotalSavedRecipes = user.SavedRecipes?.Count ?? 0            
        };
    }

    // Report a user (returns a string message)
    public async Task<string> ReportUserAsync(string userId)
    {
        var user = await _userRepository.GetByIdEntityAsync(userId);
        return user == null ? "User not found." : $"User {userId} has been reported to the Super Admin.";
    }

    // Delete a user
    public async Task<bool> DeleteUserAsync(string id)
    {
        var user = await _userRepository.GetByIdEntityAsync(id);
        if (user == null) return false;

        var result = await _userRepository.DeleteAsync(user);
        return result.Succeeded; // returning success status
    }

    //Logout
    public async Task LogoutAsync()
    {
        await _signInManager.SignOutAsync();
    }
}
