using Microsoft.AspNetCore.Mvc;
using SkillSnap.Api.Data;
using SkillSnap.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Microsoft.Extensions.Caching.Memory;


namespace SkillSnap.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly SkillSnapContext _context;
        private readonly IMemoryCache _cache;

        public ProjectsController(SkillSnapContext context, IMemoryCache cache)
        {
            _cache = cache;
            _context = context;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            // Uncomment these lines to log user informatio
            // Console.WriteLine($"👤 User authenticated: {User.Identity?.IsAuthenticated}");
            // Console.WriteLine($"👤 User name: {User.Identity?.Name}");
            // Console.WriteLine($"👤 Roles: {string.Join(", ", User.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value))}");


            Console.WriteLine("Retrieving all projects...");
            Stopwatch stopwatch = Stopwatch.StartNew();

            const string cacheKey = "all_projects";

            if (_cache.TryGetValue(cacheKey, out List<ProjectDto>? cachedProjects))
            {
                stopwatch.Stop();
                Console.WriteLine($"Retrieved {cachedProjects!.Count} projects from cache in {stopwatch.ElapsedMilliseconds} ms");
                return Ok(cachedProjects);
            }

            Console.WriteLine("🔄 Cache miss, querying database...");

            // Retrieve projects with related PortfolioUser
            // Use AsNoTracking for read-only queries to improve performance
            // Use Include to eagerly load related PortfolioUser data
            // Use Select to project into a DTO for better performance and reduced data transfer
            var projects = await _context.Projects
                .AsNoTracking()
                .Include(p => p.PortfolioUser)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    PortfolioUserName = p.PortfolioUser != null ? p.PortfolioUser.Name : "Unassigned"
                })
                .ToListAsync();

            stopwatch.Stop();
            Console.WriteLine($"Retrieved {projects.Count} projects in {stopwatch.ElapsedMilliseconds} ms");

            _cache.Set(cacheKey, projects, GetDefaultCacheOptions());

            return Ok(projects);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public IActionResult Add(Project project)
        {
            _context.Projects.Add(project);
            _context.SaveChanges();
            _cache.Remove("all_projects"); // Invalidate cache after adding a new project

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (!string.IsNullOrEmpty(userIdClaim))
            {
                // Find the PortfolioUser associated with the current ApplicationUser
                var portfolioUserId = _context.PortfolioUsers
                    .Where(p => p.ApplicationUserId == userIdClaim)
                    .Select(p => p.Id)
                    .FirstOrDefault();

                if (portfolioUserId != 0)
                    _cache.Remove($"projects_user_{portfolioUserId}");
            }

            return CreatedAtAction(nameof(GetAll), new { id = project.Id }, project);
        }

        [Authorize]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyProjects()
        {

            Console.WriteLine("Retrieving current user's projects...");
            Stopwatch stopwatch = Stopwatch.StartNew();

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

            var cacheKey = $"projects_user_{portfolioUserId}";

            if (_cache.TryGetValue(cacheKey, out List<ProjectDto>? cachedProjects))
            {
                stopwatch.Stop();
                Console.WriteLine($"Retrieved {cachedProjects!.Count} projects from cache in {stopwatch.ElapsedMilliseconds} ms");
                return Ok(cachedProjects);
            }

            Console.WriteLine("🔄 Cache miss, querying database...");

            // Retrieve projects with related PortfolioUser
            // Use AsNoTracking for read-only queries to improve performance
            // Use Include to eagerly load related PortfolioUser data
            // Use Select to project into a DTO for better performance and reduced data transfer
            var projects = await _context.Projects
                .AsNoTracking()
                .Where(p => p.PortfolioUserId == portfolioUserId)
                .Include(p => p.PortfolioUser)
                .Select(p => new ProjectDto
                {
                    Id = p.Id,
                    Title = p.Title,
                    Description = p.Description,
                    ImageUrl = p.ImageUrl,
                    PortfolioUserName = p.PortfolioUser != null ? p.PortfolioUser.Name : "Unassigned"
                })
                .ToListAsync();

            stopwatch.Stop();
            Console.WriteLine($"Retrieved {projects.Count} projects in {stopwatch.ElapsedMilliseconds} ms");
            if (projects.Count == 0)
            {
                Console.WriteLine("⚠️ No projects found.");
                return Ok(projects);
            }

            _cache.Set(cacheKey, projects, GetDefaultCacheOptions());

            return Ok(projects);
        }

        private MemoryCacheEntryOptions GetDefaultCacheOptions() =>
            new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                .SetSize(1);
    }
}