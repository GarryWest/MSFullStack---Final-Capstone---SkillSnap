namespace SkillSnap.Client.Services;

using SkillSnap.Shared.Models;

public interface IProjectService
{
    event Action? OnUnauthorized;
    Task<List<ProjectDto>?> GetProjectsAsync();
    Task<List<ProjectDto>?> GetProjectsMineAsync();
    Task<Project?> AddProjectAsync(Project project);


}