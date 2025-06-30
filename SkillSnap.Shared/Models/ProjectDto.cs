namespace SkillSnap.Shared.Models;

public class ProjectDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string PortfolioUserName { get; set; } = string.Empty;
}