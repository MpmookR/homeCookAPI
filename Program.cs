using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;
using homeCookAPI.Services;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi.Models;
using AspNetCoreRateLimit;
using DotNetEnv;

Env.Load(); // Load .env file automatically

var builder = WebApplication.CreateBuilder(args);

// Load environment variables
builder.Configuration.AddEnvironmentVariables();

//Ilogger
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Environment Variables
var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? "https://localhost:5057";
var emailSender = Environment.GetEnvironmentVariable("EMAIL_SENDER") ?? "";
var smtpServer = Environment.GetEnvironmentVariable("SMTP_SERVER") ?? "";
var smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME") ?? "";
var smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD") ?? "";
var dbConnection = Environment.GetEnvironmentVariable("DATABASE_CONNECTION") ?? "Data Source=homeCook.db";
var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY") ?? "";

Console.WriteLine($"🔍 JWT_ISSUER: {Environment.GetEnvironmentVariable("JWT_ISSUER")}");
Console.WriteLine($"🔍 DATABASE_CONNECTION: {Environment.GetEnvironmentVariable("DATABASE_CONNECTION")}");
Console.WriteLine($"🔍 JWT_KEY: {Environment.GetEnvironmentVariable("JWT_KEY")}");

// Override config with environment variables
builder.Configuration["Jwt:Issuer"] = jwtIssuer;
builder.Configuration["EmailSettings:SmtpServer"] = smtpServer;
builder.Configuration["EmailSettings:SmtpUsername"] = smtpUsername;
builder.Configuration["EmailSettings:SmtpPassword"] = smtpPassword;
builder.Configuration["ConnectionStrings:Connection"] = dbConnection;
builder.Configuration["Jwt:Key"] = jwtKey;

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
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
    };
});

//  Swagger service
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "HomeCookAPI",
        Version = "v1",
        Description = "API Documentation for HomeCookAPI",
        Contact = new OpenApiContact
        {
            Name = "Your Name",
            Email = "your.email@example.com",
            Url = new Uri("https://github.com/MpmookR/homeCookAPI")
        }
    });

    // Protect Swagger in production
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Enter 'Bearer {your JWT token}'",
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

// CORS: prevents unauthorized frontend apps from calling API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigins",
        policy =>
        {
            policy.WithOrigins("https://yourfrontend.com") // replace later
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
});

// Add Rate Limiting
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(options =>
{
    options.GeneralRules = new List<RateLimitRule>
    {
        new RateLimitRule
        {
            Endpoint = "*", // Applies to all endpoints
            Limit = 100, // 100 requests
            Period = "10m" // Every 10 minutes
        }
    };
});
builder.Services.AddInMemoryRateLimiting();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();

var app = builder.Build();

//Global Exception Handling Middleware
app.UseMiddleware<GlobalExceptionMiddleware>();

// Configure the HTTP request pipeline
app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigins");

app.UseIpRateLimiting();

// Enable Authentication & Authorization Middleware
app.UseAuthentication();
app.UseAuthorization();

// Configure Swagger UI (Restrict in production)
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HomeCookAPI v1");
        c.RoutePrefix = "api-docs"; // Accessible at /api-docs
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "HomeCookAPI v1");
        c.RoutePrefix = "api-docs"; // Accessible at /api-docs
    });
}

// Map Controllers (REST API routes)
app.MapControllers();
app.Run();
