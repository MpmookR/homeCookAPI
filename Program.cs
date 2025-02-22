using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using homeCookAPI.Models;

var builder = WebApplication.CreateBuilder(args);

// Retrieve connection string from appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Connection"))); 

// Configure SQLite Database
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=homeCook.db")); // SQLite database file

// Configure Identity
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.AddAuthentication();
builder.Services.AddAuthorization();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = null; // Remove circular reference tracking
        options.JsonSerializerOptions.PropertyNamingPolicy = null; // Keep property names as written
        options.JsonSerializerOptions.WriteIndented = true;
    });


var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

// Map controllers (REST API routes)
app.MapControllers();

app.Run();
