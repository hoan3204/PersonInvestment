using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.UnitOfWork;

using PersonalInvestmentSystem.Web.Domain.Entities;
using PersonalInvestmentSystem.Web.Domain.Enums;

namespace PersonalInvestmentSystem.Web.Services.Implementations
{
    public class TransactionService : ITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWalletService _walletService;
        private readonly IProductService _productService;
        private readonly INotificationService _notificationService;
        public TransactionService(IUnitOfWork unitOfWork, IWalletService walletService, IProductService productService, INotificationService notificationService)
        {
            _unitOfWork = unitOfWork;
            _walletService = walletService;
            _productService = productService;
            _notificationService = notificationService;
        }

        public async Task<bool> BuyProductAsync(string userId, int productId, int quantity)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null || !product.IsActive) return false;

            decimal totalAmount = product.CurrentPrice * quantity;

            //kiem tra so du vi
            if (!await _walletService.HasEnoughBalanceAsync(userId, totalAmount))
                return false;

            //tru tien vi
            bool walletUpdate = await _walletService.UpdateBalanceAsync(userId, totalAmount, false);
            if (!walletUpdate) return false;

            //cap nhat hoac tao portfolio
            var portfolio = (await _unitOfWork.Portfolios.FindAsync(p => p.UserId == userId && p.ProductId == productId))
                .FirstOrDefault();

            if (portfolio == null)
            {
                portfolio = new Portfolio
                {
                    UserId = userId,
                    ProductId = productId,
                    Quantity = quantity,
                    AverageBuyPrice = product.CurrentPrice
                };
                await _unitOfWork.Portfolios.AddAsync(portfolio);
                
            }
            else
            {
                //tinh gia tb moi 
                decimal totalCost = portfolio.Quantity * portfolio.AverageBuyPrice + quantity * product.CurrentPrice;
                portfolio.Quantity += quantity;
                portfolio.AverageBuyPrice = totalCost / portfolio.Quantity;
                portfolio.LastUpdated = DateTime.UtcNow;
                _unitOfWork.Portfolios.Update(portfolio);
            }
                //tao giao dich
                var transaction = new Transaction
                {
                    UserId = userId,
                    ProductId = productId,
                    Type = TransactionType.Buy,
                    Quantity = quantity,
                    Price = product.CurrentPrice,
                    TotalAmount = totalAmount,
                    Status = TransactionStatus.Completed,
                    PaymentMethod = PaymentMethod.BankTransfer, // Mock
                    CreatedDate = DateTime.UtcNow
                };
            await _unitOfWork.Transactions.AddAsync(transaction);
            await _notificationService.SendBuySuccessNotificationAsync(userId, product.Name, totalAmount);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }
        public async Task<bool> SellProductAsync(string userId, int productId, int quantity)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null) return false;

            //kiem tra portfolio
            var portfolio = (await _unitOfWork.Portfolios.FindAsync(p => p.UserId == userId && p.ProductId == productId)).FirstOrDefault();
            if (portfolio == null || portfolio.Quantity < quantity)
                return false; //k co de ban

            decimal totalAmount = product.CurrentPrice * quantity;

            // Cộng tiền vào ví
            bool walletUpdated = await _walletService.UpdateBalanceAsync(userId, totalAmount, true);
            if (!walletUpdated) return false;

            //giam so luong trong portfolio
            portfolio.Quantity -= quantity;
            portfolio.LastUpdated = DateTime.UtcNow;
            if (portfolio.Quantity == 0)
            {
                _unitOfWork.Portfolios.Remove(portfolio);
            }
            else
            {
                _unitOfWork.Portfolios.Update(portfolio);
            }

                var transaction = new Transaction
                {
                    UserId = userId,
                    ProductId = productId,
                    Type = TransactionType.Sell,
                    Quantity = quantity,
                    Price = product.CurrentPrice,
                    TotalAmount = totalAmount,
                    Status = TransactionStatus.Completed,
                    PaymentMethod = PaymentMethod.BankTransfer,
                    CreatedDate = DateTime.UtcNow
                };

            await _unitOfWork.Transactions.AddAsync(transaction);
            await _notificationService.SendSellSuccessNotificationAsync(userId, product.Name, totalAmount);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<Transaction>> GetUserTransactionsAsync(string userId)
        {
            return await _unitOfWork.Transactions.FindAsync(t => t.UserId == userId);
        }

        public async Task<Transaction?> GetTransactionByIdAsync(int id)
        {
            return await _unitOfWork.Transactions.GetByIdAsync(id);
        }
    }
}
