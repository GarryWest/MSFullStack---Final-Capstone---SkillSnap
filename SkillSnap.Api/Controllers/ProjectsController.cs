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


            Console.WriteLine("Retrieving projects...");
            Stopwatch stopwatch = Stopwatch.StartNew();

            const string cacheKey = "all_projects";
            var projects = new List<ProjectDto>();

            if (_cache.TryGetValue(cacheKey, out List<ProjectDto>? cachedProjects))
            {
                Console.WriteLine("✅ Returning projects from cache.");
                projects = cachedProjects ?? new List<ProjectDto>();
                stopwatch.Stop();
                Console.WriteLine($"Retrieved {projects!.Count} projects from cache in {stopwatch.ElapsedMilliseconds} ms");
                return Ok(projects);
            }

            Console.WriteLine("🔄 Cache miss, querying database...");

            // Retrieve projects with related PortfolioUser
            // Use AsNoTracking for read-only queries to improve performance
            // Use Include to eagerly load related PortfolioUser data
            // Use Select to project into a DTO for better performance and reduced data transfer
            projects = await _context.Projects
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
            if (projects.Count == 0)
            {
                Console.WriteLine("⚠️ No projects found.");
                return Ok(projects);
            }
            // Cache the projects for 5 minutes
            var cacheOptions = new MemoryCacheEntryOptions()
                .SetAbsoluteExpiration(TimeSpan.FromMinutes(5))
                .SetSlidingExpiration(TimeSpan.FromMinutes(2))
                .SetSize(1); // optional if using size-based eviction

            _cache.Set(cacheKey, projects, cacheOptions);

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