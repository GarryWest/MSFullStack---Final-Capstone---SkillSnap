using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;
using SkillSnap.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using SkillSnap.Api.Services;
using System.Security.Claims;

#region Builder setup
var builder = WebApplication.CreateBuilder(args);
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

var config = builder.Configuration;


var jwtSettings = config.GetSection("JwtSettings");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]);

// Add services to the container.

// Register the MemoryCache service
// This is useful for caching data in memory, such as user sessions or frequently accessed data
builder.Services.AddMemoryCache();

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme; // 👈 this is key
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(key),
        RoleClaimType = ClaimTypes.Role
    };
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context =>
        {
            Console.WriteLine($"JWT auth failed: {context.Exception.Message}");
            return Task.CompletedTask;
        },
        OnTokenValidated = context =>
        {
            Console.WriteLine("JWT token validated successfully.");
            return Task.CompletedTask;
        }
    };
});

// Register the JwtTokenService for generating tokens
builder.Services.AddScoped<JwtTokenService>();

// Register Identity services
builder.Services.AddIdentityCore<ApplicationUser>()
    .AddRoles<IdentityRole>()
    .AddSignInManager<SignInManager<ApplicationUser>>() // 👈 Add this
    .AddEntityFrameworkStores<SkillSnapContext>()
    .AddDefaultTokenProviders();

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true; // optional, for readability
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register the DbContext with SQLite
// Ensure you have the Microsoft.EntityFrameworkCore.Sqlite package installed
builder.Services.AddSkillSnapDb(builder.Configuration, builder.Environment);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins("http://localhost:5097")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// For automatic seeding of sample data
builder.Services.AddScoped<DatabaseSeeder>();
#endregion

#region App pipeline
var app = builder.Build();

// Run the database seeder
// This should be done before any other middleware that might depend on the database being seeded
using (var scope = app.Services.CreateScope())
{
    var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
    await seeder.SeedAsync();
    //await seeder.SeedUsersAsync();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowClient");
app.UseAuthentication(); // 👈 before UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.Run();
#endregion

#region Tests support
// This partial class allows the Program class to be accessible in tests
public partial class Program { } // 👈 This makes Program accessible to tests

// This extension method is used to configure the database context for SkillSnap
// It allows for different configurations based on the environment
public static class DatabaseConfig
{
    public static void AddSkillSnapDb(this IServiceCollection services, IConfiguration config, IWebHostEnvironment env)
    {
        if (env.IsEnvironment("Testing"))
        {
            // Skip SQLite in tests — in-memory will be injected by the test factory
            return;
        }

        services.AddDbContext<SkillSnapContext>(options =>
        {
            options.UseSqlite(config.GetConnectionString("DefaultConnection"));
        });
    }
}
#endregion