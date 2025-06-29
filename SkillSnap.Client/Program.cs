using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SkillSnap.Client;
using SkillSnap.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register HttpClient for API calls
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<CustomAuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(provider =>
    provider.GetRequiredService<CustomAuthStateProvider>());

builder.Services.AddTransient<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient<AuthService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5072/");
});

builder.Services.AddHttpClient<ProjectService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5072/");
})
.AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

builder.Services.AddHttpClient<SkillService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:5072/");
})
.AddHttpMessageHandler<JwtAuthorizationMessageHandler>();

// Register UserStateService to manage user state
builder.Services.AddScoped<UserStateService>();

// Register ProjectService as a scoped service
builder.Services.AddScoped<IProjectService, ProjectService>();


await builder.Build().RunAsync();
