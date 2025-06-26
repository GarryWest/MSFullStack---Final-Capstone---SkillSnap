using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;
using SkillSnap.Shared.Models;

namespace SkillSnap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SkillsController : ControllerBase
    {
        private readonly SkillSnapContext _context;

        public SkillsController(SkillSnapContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var skills = _context.Skills
            .Include(s => s.PortfolioUser)
            .ToList();
            Console.WriteLine($"Retrieved {skills.Count} skills from the database.");
            return Ok(skills);
        }

        [HttpPost]
        public IActionResult Add(Skill skill)
        {
            _context.Skills.Add(skill);
            _context.SaveChanges();
            return CreatedAtAction(nameof(GetAll), new { id = skill.Id }, skill);
        }
    }
}