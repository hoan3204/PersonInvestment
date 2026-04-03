using Microsoft.EntityFrameworkCore.Query;
using PersonalInvestmentSystem.Web.Domain.Entities;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.UnitOfWork;

namespace PersonalInvestmentSystem.Web.Services.Implementations
{
    public class WatchListService : IWatchlistService
    {
        private readonly IUnitOfWork _unitOfWork;

        public WatchListService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<WatchList>> GetUserWatchListAsync(string userId)
        {
            var watchlist = await _unitOfWork.WatchLists.FindAsync(w => w.UserId == userId);

            foreach (var w in watchlist)
            {
                if (w.Product == null)
                {
                    w.Product = await _unitOfWork.InvestmentProducts.GetByIdAsync(w.ProductId);
                }
            }
            return watchlist;
        }

        public async Task<bool> AddToWatchlistAsync(string userId, int productId)
        {
            var exists = await _unitOfWork.WatchLists.FindAsync(w => w.UserId == userId && w.ProductId == productId);
            if (exists.Any()) return false;

            var watchlist = new WatchList
            {
                UserId =userId,
                ProductId = productId,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.WatchLists.AddAsync(watchlist);
            await _unitOfWork.SaveChangesAsync();
            return true;

        }

        public async Task<bool> RemoveFromWatchListAsync(string userId, int productId)
        {

            var items = await _unitOfWork.WatchLists.FindAsync(w => w.UserId == userId && w.ProductId == productId);
            var item = items.FirstOrDefault();
            if (item == null) return false;

            _unitOfWork.WatchLists.Remove(item);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<bool> IsInWatchListAsync(string userId, int productId)
        {
            var items = await _unitOfWork.WatchLists.FindAsync(w => w.UserId == userId && w.ProductId == productId);
            return items.Any();
        }
    }
}
