using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;
using SkillSnap.Shared.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing"); // 👈 This skips SQLite registration


        builder.ConfigureServices(services =>
        {
            // Remove the existing authentication scheme
            var authSchemeDescriptor = services
                .FirstOrDefault(d => d.ServiceType == typeof(AuthenticationSchemeOptions) &&
                                     d.ImplementationType == typeof(TestAuthHandler));
            if (authSchemeDescriptor != null)
            {
                services.Remove(authSchemeDescriptor);
            }
            // Register the test authentication handler
            // This is a custom handler that simulates authentication for testing purposes
            services.AddAuthentication(TestAuthHandler.Scheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.Scheme, options => { });

            services.AddAuthorization(options =>
            {
                options.DefaultPolicy = new AuthorizationPolicyBuilder()
                    .AddAuthenticationSchemes(TestAuthHandler.Scheme)
                    .RequireAuthenticatedUser()
                    .Build();
            });


            // Remove all SkillSnapContext registrations
            var dbContextDescriptors = services
                .Where(d => d.ServiceType == typeof(DbContextOptions<SkillSnapContext>) ||
                            d.ServiceType == typeof(SkillSnapContext))
                .ToList();

            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            // Register in-memory database
            services.AddDbContext<SkillSnapContext>(options =>
            {
                options.UseInMemoryDatabase("TestDb");
            });

            // Ensure the service provider is rebuilt
            var sp = services.BuildServiceProvider();

            // Seed test data
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<SkillSnapContext>();
            db.Database.EnsureCreated();
            SeedTestData(db);
        });
    }

    private void SeedTestData(SkillSnapContext context)
    {
        var testUser = new PortfolioUser
        {
            Id = 1,
            Name = "Test User",
            ApplicationUserId = "test-user-id" // must match the claim in TestAuthHandler
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