using PersonalInvestmentSystem.Web.Domain.Entities;

namespace PersonalInvestmentSystem.Web.Services.Interfaces
{
    public interface IWalletService
    {
        Task<Wallet?> GetWalletByUserIdAsync(string userId);
        Task<decimal> GetBalanceAsync(string userId);
        Task<bool> UpdateBalanceAsync(string userId, decimal amount, bool isIncrease); //true cong tien , false - tienf
        Task<bool> HasEnoughBalanceAsync(string userId, decimal requiredAmount);                                                                            
    }
}
