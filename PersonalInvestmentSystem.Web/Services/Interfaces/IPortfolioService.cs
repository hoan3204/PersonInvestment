using PersonalInvestmentSystem.Web.Domain.Entities;
using PersonalInvestmentSystem.Web.ViewModels.Portfolio;

namespace PersonalInvestmentSystem.Web.Services.Interfaces
{
    public interface IPortfolioService
    {
        Task<PortfolioViewModel> GetUserPortfolioAsync(string userId);
        Task<IEnumerable<Portfolio>> GetUserPortfolioItemsAsync(string userId);
    }
}
