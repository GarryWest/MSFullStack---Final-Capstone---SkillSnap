using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SkillSnap.Shared.Models;

public class Project
{
    [Key]
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ImageUrl { get; set; } = string.Empty;

    [ForeignKey(nameof(PortfolioUser))]
    public int PortfolioUserId { get; set; }

    public PortfolioUser? PortfolioUser { get; set; }
}