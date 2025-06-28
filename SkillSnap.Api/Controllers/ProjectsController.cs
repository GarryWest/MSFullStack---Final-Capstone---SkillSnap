using Microsoft.AspNetCore.Mvc;
using SkillSnap.Api.Data;
using SkillSnap.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;


namespace SkillSnap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly SkillSnapContext _context;

        public ProjectsController(SkillSnapContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            Console.WriteLine($"👤 User authenticated: {User.Identity?.IsAuthenticated}");
            Console.WriteLine($"👤 User name: {User.Identity?.Name}");
            Console.WriteLine($"👤 Roles: {string.Join(", ", User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");

            var projects = _context.Projects.ToList();
            Console.WriteLine($"Retrieved {projects.Count} projects from the database.");
            return Ok(projects);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Add(Project project)
        {
            _context.Projects.Add(project);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetAll), new { id = project.Id }, project);
        }
    }
}