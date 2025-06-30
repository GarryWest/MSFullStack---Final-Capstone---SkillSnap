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
    public class PortfolioUserController : ControllerBase
    {
        private readonly SkillSnapContext _context;
        private readonly IMemoryCache _cache;

        public PortfolioUserController(SkillSnapContext context, IMemoryCache cache)
        {
            _cache = cache;
            _context = context;
        }

        [Authorize]
        [HttpPost("update")]
        public async Task<IActionResult> UpdatePortfolioUser(PortfolioUserDto portfolioUserDto)
        {
            Console.WriteLine($"Updating portfolio user: {portfolioUserDto.Name}");
            Stopwatch stopwatch = Stopwatch.StartNew();

            // Validate the input
            // Only allow updates if the name is provided
            // and the user is authenticated and matches the portfolio user name
            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                stopwatch.Stop();
                Console.WriteLine("Unauthorized: User ID claim not found.");
                return Unauthorized();
            }

            var existingPortfolioUser = await _context.PortfolioUsers
                .FirstOrDefaultAsync(pu => pu.Name == portfolioUserDto.Name);

            if (existingPortfolioUser == null)
            {
                // If the portfoliouser does not exist, create a new one
                existingPortfolioUser = new PortfolioUser
                {
                    Name = portfolioUserDto.Name,
                    Bio = portfolioUserDto.Bio,
                    ProfileImageUrl = portfolioUserDto.ProfileImageUrl,
                    ApplicationUserId = userIdClaim
                };
                Console.WriteLine($"Creating new portfolio user: {portfolioUserDto.Name} : {userIdClaim}");
                _context.PortfolioUsers.Add(existingPortfolioUser);
                await _context.SaveChangesAsync();
            }
            else if (existingPortfolioUser.ApplicationUserId != userIdClaim)
            {
                // If the existing portfolio user belongs to a different user, return unauthorized
                stopwatch.Stop();
                Console.WriteLine("Unauthorized: User does not own this portfolio user.");
                return Unauthorized();
            }
            else
            {

                existingPortfolioUser.Bio = portfolioUserDto.Bio;
                existingPortfolioUser.ProfileImageUrl = portfolioUserDto.ProfileImageUrl;

                Console.WriteLine($"Updating existing portfolio user: {portfolioUserDto.Name} ");
                // Update the existing portfolio user
                _context.PortfolioUsers.Update(existingPortfolioUser);
                await _context.SaveChangesAsync();
            }

            // Invalidate cache for this user
            string cacheKey = $"portfolio_user_{portfolioUserDto.Name}";
            _cache.Remove(cacheKey);

            stopwatch.Stop();
            Console.WriteLine($"Updated portfolio user in {stopwatch.ElapsedMilliseconds} ms");

            return Ok(portfolioUserDto);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetPortfolioUsers()
        {
            Console.WriteLine("Retrieving all portfolio users");
            Stopwatch stopwatch = Stopwatch.StartNew();

            const string cacheKey = "portfolio_users";

            if (_cache.TryGetValue(cacheKey, out List<PortfolioUserDto>? cachedPortfolioUsers))
            {
                stopwatch.Stop();
                Console.WriteLine($"Retrieved portfolio users from cache in {stopwatch.ElapsedMilliseconds} ms");
                return Ok(cachedPortfolioUsers);
            }

            Console.WriteLine("🔄 Cache miss, querying database...");

            var portfolioUsers = await _context.PortfolioUsers
                .AsNoTracking()
                .Include(pu => pu.Projects)
                .ToListAsync();

            var portfolioUserDtos = portfolioUsers.Select(pu => new PortfolioUserDto
            {
                Name = pu.Name,
                Bio = pu.Bio,
                ProfileImageUrl = pu.ProfileImageUrl
            }).ToList();

            // Cache the result for 5 minutes
            _cache.Set(cacheKey, portfolioUserDtos, TimeSpan.FromMinutes(5));

            stopwatch.Stop();
            Console.WriteLine($"Retrieved portfolio users in {stopwatch.ElapsedMilliseconds} ms");

            return Ok(portfolioUserDtos);
        }

        [Authorize]
        [HttpGet("mine")]
        public async Task<IActionResult> GetMyPortfolioUser()
        {
            Console.WriteLine("Retrieving current user's portfolio user...");
            Stopwatch stopwatch = Stopwatch.StartNew();

            var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized();
            }

            var portfolioUser = await _context.PortfolioUsers
                .AsNoTracking()
                .Include(pu => pu.Projects)
                .FirstOrDefaultAsync(pu => pu.ApplicationUserId == userIdClaim);

            if (portfolioUser == null)
            {
                return NotFound("No portfolio found for this user.");
            }

            var portfolioUserDto = new PortfolioUserDto
            {
                Name = portfolioUser.Name,
                Bio = portfolioUser.Bio,
                ProfileImageUrl = portfolioUser.ProfileImageUrl
            };

            stopwatch.Stop();
            Console.WriteLine($"Retrieved my portfolio user in {stopwatch.ElapsedMilliseconds} ms");

            return Ok(portfolioUserDto);
        }

        [Authorize]
        [HttpGet("user/{username}")]
        public async Task<IActionResult> GetPortfolioUser(string username)
        {
            Console.WriteLine($"Retrieving portfolio user for: {username}");
            Stopwatch stopwatch = Stopwatch.StartNew();

            const string cacheKeyPrefix = "portfolio_user_";
            string cacheKey = $"{cacheKeyPrefix}{username}";

            if (_cache.TryGetValue(cacheKey, out PortfolioUserDto? cachedPortfolioUser))
            {
                stopwatch.Stop();
                Console.WriteLine($"Retrieved portfolio user from cache in {stopwatch.ElapsedMilliseconds} ms");
                return Ok(cachedPortfolioUser);
            }

            Console.WriteLine("🔄 Cache miss, querying database...");

            var portfolioUser = await _context.PortfolioUsers
                .AsNoTracking()
                .Include(pu => pu.Projects)
                .FirstOrDefaultAsync(pu => pu.Name == username);

            if (portfolioUser == null)
            {
                return NotFound();
            }

            var portfolioUserDto = new PortfolioUserDto
            {
                Name = portfolioUser.Name,
                Bio = portfolioUser.Bio,
                ProfileImageUrl = portfolioUser.ProfileImageUrl
            };

            // Cache the result for 5 minutes
            _cache.Set(cacheKey, portfolioUserDto, TimeSpan.FromMinutes(5));

            stopwatch.Stop();
            Console.WriteLine($"Retrieved portfolio user in {stopwatch.ElapsedMilliseconds} ms");

            return Ok(portfolioUserDto);
        }
    }
}