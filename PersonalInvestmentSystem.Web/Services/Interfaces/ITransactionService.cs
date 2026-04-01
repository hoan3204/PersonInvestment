using PersonalInvestmentSystem.Web.Domain.Entities;
using PersonalInvestmentSystem.Web.Domain.Enums;

namespace PersonalInvestmentSystem.Web.Services.Interfaces
{
    public interface ITransactionService
    {
        Task<bool> BuyProductAsync(string userId, int productId, int quantity);
        Task<bool> SellProductAsync(string userId, int productId, int quantity);
        Task<IEnumerable<Transaction>> GetUserTransactionsAsync(string userId);
        Task<Transaction> GetTransactionByIdAsync(int id);
    }
}
