using Microsoft.AspNetCore.Mvc;
using SkillSnap.Api.Data;
using SkillSnap.Shared.Models;
using Microsoft.AspNetCore.Authorization;


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