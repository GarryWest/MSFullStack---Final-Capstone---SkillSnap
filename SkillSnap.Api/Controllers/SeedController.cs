using Microsoft.AspNetCore.Mvc;
using SkillSnap.Shared.Models;
using SkillSnap.Api.Data;
namespace SkillSnap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeedController : ControllerBase
    {
        private readonly SkillSnapContext _context;
        public SeedController(SkillSnapContext context)
        {
            _context = context;
        }
        [HttpPost]
        public IActionResult Seed()
        {
            if (_context.PortfolioUsers.Any())
            {
                return BadRequest("Sample data already exists.");
            }

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
            _context.SaveChanges();

            return Ok("Sample data inserted.");
        }
    }
}
