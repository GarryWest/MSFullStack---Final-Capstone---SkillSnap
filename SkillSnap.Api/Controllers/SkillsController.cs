using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;
using SkillSnap.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Caching.Memory;

namespace SkillSnap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SkillsController : ControllerBase
    {
        private readonly SkillSnapContext _context;
        private readonly IMemoryCache _cache;

        public SkillsController(SkillSnapContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        [Authorize]
        [HttpGet]
        public IActionResult GetAll()
        {
            const string cacheKey = "all_skills";

            if (_cache.TryGetValue(cacheKey, out List<SkillDto> cachedSkills))
            {
                Console.WriteLine("✅ Returning skills from cache.");
                return Ok(cachedSkills);
            }

            Console.WriteLine("🔄 Cache miss, querying database...");

            var skills = _context.Skills
                .AsNoTracking()
                .Include(s => s.PortfolioUser)
                .Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Level = s.Level,
                    PortfolioUserName = s.PortfolioUser != null ? s.PortfolioUser.Name : "Unassigned"
                })
                .ToList();

            Console.WriteLine($"📦 Retrieved {skills.Count} skills from the database.");

            _cache.Set(cacheKey, skills, GetDefaultCacheOptions());

            return Ok(skills);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Add(Skill skill)
        {
            _context.Skills.Add(skill);
            _context.SaveChanges();

            _cache.Remove("all_skills"); // 👈 match the DTO cache key

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userIdClaim))
            {
                // Find the PortfolioUser associated with the current ApplicationUser
                var portfolioUserId = _context.PortfolioUsers
                    .Where(p => p.ApplicationUserId == userIdClaim)
                    .Select(p => p.Id)
                    .FirstOrDefault();

                if (portfolioUserId != 0)
                    _cache.Remove($"skills_user_{portfolioUserId}");
            }

            return CreatedAtAction(nameof(GetAll), new { id = skill.Id }, skill);
        }

        [Authorize]
        [HttpGet("mine")]
        public IActionResult GetMySkills()
        {
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized();

            // Find the PortfolioUser associated with the current ApplicationUser
            var portfolioUserId = _context.PortfolioUsers
                .Where(p => p.ApplicationUserId == userIdClaim)
                .Select(p => p.Id)
                .FirstOrDefault();

            if (portfolioUserId == 0)
                return NotFound("No portfolio found for this user.");

            var cacheKey = $"skills_user_{portfolioUserId}";

            if (_cache.TryGetValue(cacheKey, out List<SkillDto> cachedSkills))
            {
                Console.WriteLine("✅ Returning user's skills from cache.");
                return Ok(cachedSkills);
            }
            Console.WriteLine("🔄 Cache miss, querying database for user's skills...");

            var skills = _context.Skills
                .AsNoTracking()
                .Where(s => s.PortfolioUserId == portfolioUserId)
                .Select(s => new SkillDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    Level = s.Level,
                    PortfolioUserName = s.PortfolioUser.Name
                })
                .ToList();

            Console.WriteLine($"📦 Retrieved {skills.Count} skills for user {portfolioUserId} from the database.");

            _cache.Set(cacheKey, skills, GetDefaultCacheOptions());

            return Ok(skills);
        }

        private MemoryCacheEntryOptions GetDefaultCacheOptions() =>
            new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                .SetSize(1);
    }
}