using PersonalInvestmentSystem.Web.Domain.Entities;

namespace PersonalInvestmentSystem.Web.Services.Interfaces
{
    public interface IWatchlistService
    {
        Task<IEnumerable<WatchList>> GetUserWatchListAsync(string userId);
        Task<bool> AddToWatchlistAsync(string userId, int productId);
        Task<bool> RemoveFromWatchListAsync(string userId, int productId);
        Task<bool> IsInWatchListAsync(string userId, int productId);
    }
}
