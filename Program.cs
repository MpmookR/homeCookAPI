using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;
using homeCookAPI.Services;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using AspNetCoreRateLimit;

var builder = WebApplication.CreateBuilder(args);

// Logger 
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();


// Retrieve Environment Variables
string databaseConnection = Environment.GetEnvironmentVariable("DATABASE_CONNECTION") ?? builder.Configuration.GetConnectionString("Connection");
string smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") ?? builder.Configuration["EmailSettings:SmtpServer"];
string smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? builder.Configuration["EmailSettings:SmtpUsername"];
string smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? builder.Configuration["EmailSettings:SmtpPassword"];
string jwtKey = Environment.GetEnvironmentVariable("JWT_SECRET") ?? builder.Configuration["Jwt:Key"];
// var jwtKey = "ThisIsMySuperSecureKeyWithAtLeast32Chars!";
string jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"];

// Debugging Logs
Console.WriteLine($"🔹 DATABASE_CONNECTION = {databaseConnection}");
Console.WriteLine($"🔹 SMTP_SERVER = {smtpServer}");
Console.WriteLine($"🔹 JWT Key Length = {jwtKey.Length}");
Console.WriteLine($"🔹 JWT Issuer = {jwtIssuer}");

// Database Connection
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(databaseConnection));

// Identity Service - User Management
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Register Email Settings & Service
builder.Services.Configure<EmailSettings>(options =>
{
    options.SmtpServer = smtpServer;
    options.SmtpUsername = smtpUsername;
    options.SmtpPassword = smtpPassword;
});
builder.Services.AddScoped<EmailService>();

// Dependency Injection: Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<ISavedRecipeRepository, SavedRecipeRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<ILikeRepository, LikeRepository>();
builder.Services.AddScoped<IRecipeRatingRepository, RecipeRatingRepository>();
builder.Services.AddScoped<ICommentRepository, CommentRepository>();

//Dependency Injection: Services (Business Logic)
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<ISavedRecipeService, SavedRecipeService>();
builder.Services.AddScoped<IRoleService, RoleService>();
builder.Services.AddScoped<ILikeService, LikeService>();
builder.Services.AddScoped<IRecipeRatingService, RecipeRatingService>();
builder.Services.AddScoped<ICommentService, CommentService>();

// Configure JSON Serialization Options
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.WriteIndented = true;
        options.JsonSerializerOptions.PropertyNamingPolicy = null;
    });

// JWT Authentication Setup 
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var keyBytes = Encoding.UTF8.GetBytes(jwtKey);

    Console.WriteLine($"JWT Key Length Used in Signing: {keyBytes.Length} characters");

    if (keyBytes.Length < 32)
    {
        throw new Exception("JWT Key is too short! It must be at least 32 characters.");
    }

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtIssuer,
        IssuerSigningKey = new SymmetricSecurityKey(keyBytes) 
    };
});


// Swagger Configuration (API Documentation)
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HomeCookAPI",
        Version = "v1",
        Description = "API Documentation for HomeCookAPI",
        Contact = new OpenApiContact
        {
            Name = "Mook Rattana",
            Email = "mpmookr@gmail.com",
            Url = new Uri("https://github.com/MpmookR/homeCookAPI")
        }
    });
});

// CORS Configuration - Allow Specific Frontend Access
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins", policy =>
    {
        policy.WithOrigins("https://yourfrontend.com") //frontend URL
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Rate Limiting Prevent - API Abuse
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*", // Applies to all endpoints
            Limit = 100, // 100 requests max
            Period = "10m" // Every 10 minutes
        }
    };
});
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

var app = builder.Build();

// Ensure HTTPS in Production
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// Middleware 
app.UseCors("AllowSpecificOrigins");

// Enable Rate Limiting
app.UseIpRateLimiting();

// Enable Authentication & Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

// Enable Swagger in Development Mode
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HomeCookAPI v1");
        c.RoutePrefix = "api-docs"; // Accessible at /api-docs
    });
}

// Map API Endpoints
app.MapControllers();

app.Run();
