using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using SkillSnap.Client;
using SkillSnap.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register HttpClient for API calls
builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri("http://localhost:5072") });

// Register your typed services
builder.Services.AddScoped<ProjectService>();
builder.Services.AddScoped<SkillService>();
builder.Services.AddScoped<AuthService>();


await builder.Build().RunAsync();
