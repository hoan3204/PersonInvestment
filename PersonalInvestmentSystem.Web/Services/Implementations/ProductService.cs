using PersonalInvestmentSystem.Web.Domain.Entities;
using PersonalInvestmentSystem.Web.Domain.Enums;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.UnitOfWork;
using PersonalInvestmentSystem.Web.ViewModels.Product;

namespace PersonalInvestmentSystem.Web.Services.Implementations
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ProductService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<InvestmentProduct>> GetAllProductsAsync()
        {
            return await _unitOfWork.InvestmentProducts.GetAllAsync();
        }

        public async Task<InvestmentProduct?> GetProductByIdAsync(int id)
        {
            return await _unitOfWork.InvestmentProducts.GetByIdAsync(id);
        }

        public async Task<IEnumerable<InvestmentProduct>> GetFeaturedProductsAsync(int count = 8)
        {
            var all = await _unitOfWork.InvestmentProducts.GetAllAsync();
            return all.Where(p => p.IsFeatured && p.IsActive)
                      .OrderByDescending(p => p.CurrentPrice)
                      .Take(count);
        }
        public async Task<IEnumerable<InvestmentProduct>> GetProductsByTypeAsync(InvestmentType type)
        {
            var all = await _unitOfWork.InvestmentProducts.GetAllAsync();
            return all.Where(p => p.Type == type && p.IsActive);
        }
        public async Task<IEnumerable<InvestmentProduct>> GetProductsByCategoryAsync(int categoryId)
        {
            var all = await _unitOfWork.InvestmentProducts.GetAllAsync();
            return all.Where(p => p.CategoryId == categoryId && p.IsActive);
        }

        public async Task AddProductAsync(InvestmentProduct product)
        {
            await _unitOfWork.InvestmentProducts.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateProductAsync(InvestmentProduct product)
        {
            _unitOfWork.InvestmentProducts.Update(product);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task DeleteProductAsync(int id)
        {
            var product = await _unitOfWork.InvestmentProducts.GetByIdAsync(id);
            if (product != null)
            {
                product.IsActive = false;
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task<PagedResult<InvestmentProduct>> GetPagedProductsAsync(int page = 1, int pageSize = 10, string search = "")
        {
            var all = await _unitOfWork.InvestmentProducts.GetAllAsync();
            var query = all.Where(p => p.IsActive);

            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase)
                || p.Code.Contains(search, StringComparison.OrdinalIgnoreCase));
                    
            }

            var totalItems = query.Count();
            var items = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            return new PagedResult<InvestmentProduct>
            {
                Items = items,
                TotalItems = totalItems,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
