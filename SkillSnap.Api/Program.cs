using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using SkillSnap.Api.Data;

#region Builder setup
var builder = WebApplication.CreateBuilder(args);
var config = builder.Configuration;

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Register the DbContext with SQLite
// Ensure you have the Microsoft.EntityFrameworkCore.Sqlite package installed
builder.Services.AddDbContext<SkillSnapContext>(options =>
options.UseSqlite(config.GetConnectionString("DefaultConnection")));
#endregion

#region App pipeline
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
#endregion