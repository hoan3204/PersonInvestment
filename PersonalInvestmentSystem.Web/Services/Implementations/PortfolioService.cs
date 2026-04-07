using PersonalInvestmentSystem.Web.Domain.Entities;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.UnitOfWork;
using PersonalInvestmentSystem.Web.ViewModels.Portfolio;

namespace PersonalInvestmentSystem.Web.Services.Implementations
{
    public class PortfolioService : IPortfolioService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IProductService _productService;

        public PortfolioService(IUnitOfWork unitOfWork, IProductService productService)
        {
            _unitOfWork = unitOfWork;
            _productService = productService;
   
        }
        public async Task<PortfolioViewModel> GetUserPortfolioAsync(string userId)
        {
            var items = await GetUserPortfolioItemsAsync(userId);
            var vm = new PortfolioViewModel();
            foreach (var item in items)
            {
                // Fetch product using ProductService instead of relying on navigation property
                var product = await _productService.GetProductByIdAsync(item.ProductId);
                if (product == null) continue;

                decimal currentValue = item.Quantity * product.CurrentPrice;
                decimal cost = item.Quantity * item.AverageBuyPrice;
                decimal profit = currentValue - cost;
                decimal profitPercent = cost > 0 ? (profit / cost) * 100 : 0;

                vm.Items.Add(new PortfolioItemViewModel
                { ProductId = item.ProductId,
                  ProductName = product.Name,
                  ProductCode = product.Code,
                  Quantity = item.Quantity,
                  AverageBuyPrice = item.AverageBuyPrice,
                  CurrentPrice = product.CurrentPrice,
                  Profit = profit,
                  ProfitPercent = profitPercent,
                  Type = product.Type.ToString()
                });
                vm.TotalValue += currentValue;
                vm.TotalCost += cost;
                
            }
            vm.TotalProfit = vm.TotalValue - vm.TotalCost;
            vm.ROIPercent = vm.TotalCost > 0 ? (vm.TotalProfit / vm.TotalCost) * 100 : 0;
            return vm;
        }

        public async Task<IEnumerable<Portfolio>> GetUserPortfolioItemsAsync(string userId)
        {
            // Lấy danh sách Portfolio trước
            var portfolios = await _unitOfWork.Portfolios.FindAsync(
                p => p.UserId == userId && p.Quantity > 0
            );

            // Load Product thủ công cho từng Portfolio để tránh null
            foreach (var portfolio in portfolios)
            {
                if (portfolio.Product == null)
                {
                    portfolio.Product = await _unitOfWork.InvestmentProducts.GetByIdAsync(portfolio.ProductId);
                }
            }

            return portfolios;
        }
    }
}
