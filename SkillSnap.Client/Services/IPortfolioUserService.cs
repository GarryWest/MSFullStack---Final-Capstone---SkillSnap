namespace SkillSnap.Client.Services;

using SkillSnap.Shared.Models;

public interface IPortfolioUserService
{
    event Action? OnUnauthorized;

    Task<PortfolioUserDto?> GetPortfolioUserAsync(string username);
    Task<PortfolioUserDto?> GetPortfolioUserMineAsync();
    Task<List<PortfolioUserDto>?> GetPortfolioUsersAsync();
    Task<PortfolioUserDto?> UpdatePortfolioUserAsync(PortfolioUserDto portfolioUser);

}