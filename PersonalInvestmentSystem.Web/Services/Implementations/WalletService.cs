using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.UnitOfWork;
using PersonalInvestmentSystem.Web.Domain.Entities;

namespace PersonalInvestmentSystem.Web.Services.Implementations
{
    public class WalletService : IWalletService
    {
        private readonly IUnitOfWork _unitOfWork;
        public WalletService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Wallet?> GetWalletByUserIdAsync(string userId)
        {
            var wallets = await _unitOfWork.Wallets.FindAsync(W => W.UserId == userId);
            return wallets.FirstOrDefault();
        }

        public async Task<decimal> GetBalanceAsync(string userId)
        {
            var wallet = await GetWalletByUserIdAsync(userId);
            return wallet?.Balance ?? 0;
        }

        public async Task<bool> UpdateBalanceAsync(string userId, decimal amount, bool isIncrease)
        {
            var wallet = await GetWalletByUserIdAsync(userId);
            if (wallet == null) return false;

            if (isIncrease)
            {
                wallet.Balance += amount;
            }
            else
            {
                if (wallet.Balance < amount) return false;
                wallet.Balance -= amount;
            }

            wallet.UpdatedDate = DateTime.UtcNow;
            _unitOfWork.Wallets.Update(wallet);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<bool> HasEnoughBalanceAsync(string userId, decimal requiredAmount)
        {
            var balance = await GetBalanceAsync(userId);
            return balance >= requiredAmount;
        }
    }
}
