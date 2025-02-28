using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;
using homeCookAPI.Services;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

//Ilogger
builder.Logging.ClearProviders();
builder.Logging.AddConsole();  
builder.Logging.AddDebug();    

// Retrieve connection string from appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Connection")));

// Identity service
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register EmailSettings & email service
builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddScoped<EmailService>();

//Repositories (Dependency Injection)
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<ISavedRecipeRepository, SavedRecipeRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<IRecipeRatingRepository, RecipeRatingRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

//Services: Business Logic Layer
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<ISavedRecipeService, SavedRecipeService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<IRecipeRatingService, RecipeRatingService>();
builder.Services.AddScoped<ICommentService, CommentService>();

//  Controllers & JSON Options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles; // ✅ Fixes self-referencing issues
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull; // ✅ Removes null values
        options.JsonSerializerOptions.WriteIndented = true; 
        options.JsonSerializerOptions.PropertyNamingPolicy = null; 
    });


// JWT Authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
    };
});

var app = builder.Build();

// Enable Authentication & Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

// Map Controllers (REST API routes)
app.MapControllers();

app.Run();
