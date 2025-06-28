using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using SkillSnap.Api.Data;
using SkillSnap.Shared.Models;

namespace SkillSnap.Api.Services;

public class DatabaseSeeder
{
    private readonly SkillSnapContext _context;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IWebHostEnvironment _env;

    public DatabaseSeeder(
        SkillSnapContext context,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IWebHostEnvironment env)
    {
        _context = context;
        _userManager = userManager;
        _roleManager = roleManager;
        _env = env;
    }

    public async Task SeedAsync()
    {
        // 1. Create User role if it doesn't exist
        if (!await _roleManager.RoleExistsAsync("User"))
        {
            await _roleManager.CreateAsync(new IdentityRole("User"));
        }

        if (!_env.IsDevelopment()) return;

        if (_context.PortfolioUsers.Any()) return;

        var users = new List<PortfolioUser>
            {
                new PortfolioUser
                {
                    Name = "Jordan Developer",
                    Bio = "Full-stack developer passionate about learning new tech.",
                    ProfileImageUrl = "https://example.com/images/jordan.png",
                    Projects = new List<Project>
                    {
                        new Project { Title = "Task Tracker", Description = "Manage tasks effectively", ImageUrl = "https://example.com/images/task.png" },
                        new Project { Title = "Weather App", Description = "Forecast weather using APIs", ImageUrl = "https://example.com/images/weather.png" }
                    },
                    Skills = new List<Skill>
                    {
                        new Skill { Name = "C#", Level = "Advanced" },
                        new Skill { Name = "Blazor", Level = "Intermediate" }
                    }
                },
                new PortfolioUser
                {
                    Name = "Avery Designer",
                    Bio = "UX/UI designer with a flair for clean interfaces.",
                    ProfileImageUrl = "https://example.com/images/avery.png",
                    Projects = new List<Project>
                    {
                        new Project { Title = "Design System", Description = "Reusable UI components for web apps", ImageUrl = "https://example.com/images/design.png" },
                        new Project { Title = "Mobile Mockups", Description = "Prototypes for a fitness app", ImageUrl = "https://example.com/images/mockup.png" }
                    },
                    Skills = new List<Skill>
                    {
                        new Skill { Name = "Figma", Level = "Advanced" },
                        new Skill { Name = "Adobe XD", Level = "Intermediate" }
                    }
                },
                new PortfolioUser
                {
                    Name = "Riley Engineer",
                    Bio = "Cloud-native engineer focused on scalable systems.",
                    ProfileImageUrl = "https://example.com/images/riley.png",
                    Projects = new List<Project>
                    {
                        new Project { Title = "Kubernetes Dashboard", Description = "Visualize cluster metrics", ImageUrl = "https://example.com/images/k8s.png" },
                        new Project { Title = "CI/CD Pipeline", Description = "Automated deployment with GitHub Actions", ImageUrl = "https://example.com/images/cicd.png" }
                    },
                    Skills = new List<Skill>
                    {
                        new Skill { Name = "Docker", Level = "Advanced" },
                        new Skill { Name = "Azure", Level = "Intermediate" }
                    }
                }
            };

        _context.PortfolioUsers.AddRange(users);

        // 1. Create Admin role if it doesn't exist
        if (!await _roleManager.RoleExistsAsync("Admin"))
        {
            await _roleManager.CreateAsync(new IdentityRole("Admin"));
        }

        // 2. Create Admin user
        var adminEmail = "admin@skillsnap.io";
        var adminUser = await _userManager.FindByEmailAsync(adminEmail);
        if (adminUser == null)
        {
            adminUser = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(adminUser, "Admin*123");
            if (!result.Succeeded)
                return; // BadRequest("Failed to create admin user.");
        }

        // 3. Assign Admin role
        if (!await _userManager.IsInRoleAsync(adminUser, "Admin"))
        {
            await _userManager.AddToRoleAsync(adminUser, "Admin");
        }
        // Save changes to the context so we can link the admin user to a PortfolioUser
        _context.SaveChanges();

        // 4. Link to a PortfolioUser
        var adminProfile = new PortfolioUser
        {
            Name = "Admin User",
            Bio = "System administrator",
            ProfileImageUrl = "https://example.com/images/admin.png",
            ApplicationUserId = adminUser.Id
        };

        _context.PortfolioUsers.Add(adminProfile);

        await _context.SaveChangesAsync();
        Console.WriteLine("Sample data inserted successfully.");

        return; // Ok("Sample data inserted.");
    }
}