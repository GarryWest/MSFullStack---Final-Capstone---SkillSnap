using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;
using SkillSnap.Shared.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Options;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly Action<List<Claim>>? _mutateClaims;

    public CustomWebApplicationFactory(Action<List<Claim>>? mutateClaims = null)
    {
        _mutateClaims = mutateClaims;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Replace DbContext with in-memory
            var descriptors = services.Where(
                d => d.ServiceType == typeof(DbContextOptions<SkillSnapContext>) ||
                     d.ServiceType == typeof(SkillSnapContext)).ToList();

            foreach (var descriptor in descriptors)
                services.Remove(descriptor);

            services.AddDbContext<SkillSnapContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            services.AddSingleton(_mutateClaims ?? (_ => { }));

            services.AddAuthentication(options =>
            {
                options.DefaultScheme = TestAuthHandler.Scheme;
                options.DefaultAuthenticateScheme = TestAuthHandler.Scheme;
                options.DefaultChallengeScheme = TestAuthHandler.Scheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.Scheme, options => { });

            services.AddSingleton<IAuthenticationHandlerProvider, CustomHandlerProvider>();

            services.AddSingleton<IPostConfigureOptions<AuthenticationSchemeOptions>>(sp =>
            {
                return new PostConfigureOptions<AuthenticationSchemeOptions>(TestAuthHandler.Scheme, opts => { });
            });

            services.AddAuthorization(options =>
             {
                 options.DefaultPolicy = new AuthorizationPolicyBuilder()
                     .AddAuthenticationSchemes(TestAuthHandler.Scheme)
                     .RequireAuthenticatedUser()
                     .Build();
             });

            // Seed data
            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SkillSnapContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    private void SeedTestData(SkillSnapContext context)
    {
        context.Projects.RemoveRange(context.Projects);
        context.PortfolioUsers.RemoveRange(context.PortfolioUsers);
        context.SaveChanges();

        var testUser = new PortfolioUser
        {
            Id = 1,
            Name = "Test User",
            ApplicationUserId = "test-user-id"
        };

        context.PortfolioUsers.Add(testUser);
        context.Projects.Add(new Project
        {
            Title = "User Project",
            Description = "Owned by test user",
            PortfolioUserId = testUser.Id
        });

        context.SaveChanges();
    }
}