using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;

#region Builder setup
var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.WriteIndented = true; // optional, for readability
    });

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register the DbContext with SQLite
// Ensure you have the Microsoft.EntityFrameworkCore.Sqlite package installed
builder.Services.AddDbContext<SkillSnapContext>(options =>
options.UseSqlite(config.GetConnectionString("DefaultConnection")));

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowClient", policy =>
    {
        policy.WithOrigins("http://localhost:5097")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

#endregion

#region App pipeline
var app = builder.Build();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowClient");


app.UseAuthorization();

app.MapControllers();

app.Run();
#endregion