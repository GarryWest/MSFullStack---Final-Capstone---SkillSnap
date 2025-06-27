using Microsoft.AspNetCore.Identity;
using SkillSnap.Shared.Models;

namespace SkillSnap.Shared.Models;

public class ApplicationUser : IdentityUser
{
    public PortfolioUser? PortfolioProfile { get; set; }
}