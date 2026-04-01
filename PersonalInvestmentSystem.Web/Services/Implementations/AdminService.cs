using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.UnitOfWork;
using PersonalInvestmentSystem.Web.ViewModels.Admin;
using PersonalInvestmentSystem.Web.Domain.Entities;

namespace PersonalInvestmentSystem.Web.Services.Implementations
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;

        public AdminService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<AdminDashboardViewModel> GetDashboardDataAsync()
        {
            var vm = new AdminDashboardViewModel();

            //tong so nguoi dung
            vm.TotalUsers = (await _unitOfWork.Users.GetAllAsync()).Count();

            var products = await _unitOfWork.InvestmentProducts.GetAllAsync();
            vm.TotalProducts = products.Count(p => p.IsActive);

            //tong giao dich

            var transaction = await _unitOfWork.Transactions.GetAllAsync();
            vm.TotalTransactions = transaction.Count();
            vm.TotalTransactionAmount = transaction.Sum(p => p.TotalAmount);

            //giao dich gan day
            vm.RecentTransactions = transaction
                .OrderByDescending(t => t.CreatedDate)
                .Take(10)
                .Select(t => new RecentTransactionViewModel
                {
                    UserFullName = t.User?.FullName ?? "Unknow",
                    ProductName = t.Product?.Name ?? "Unknown",
                    Type = t.Type.ToString(),
                    TotalAmount = t.TotalAmount,
                    CreatedDate = t.CreatedDate
                })
                .ToList();

            //top san pham giao dich nhieu
            vm.TopProducts = transaction
                .GroupBy(t => t.ProductId)
                .Select(g => new TopProductViewModel
                {
                    ProductName = g.First().Product?.Name ?? "UNknown",
                    TransactionCount = g.Count(),
                    TotalVolume = g.Sum(t => t.TotalAmount)
                })
                .OrderByDescending(x => x.TransactionCount)
                .Take(2)
                .ToList();

            return vm;
        }

    }
}
