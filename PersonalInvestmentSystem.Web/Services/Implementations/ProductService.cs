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
                product.DeleteDate = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task RestoreProductAsync(int id)
        {
            var product = await _unitOfWork.InvestmentProducts.GetByIdAsync(id);
            if (product != null)
            {
                product.IsActive = true;
                product.DeleteDate = null;
                await _unitOfWork.SaveChangesAsync();
            }
        }

        public async Task PermanentlyDeleteProductAsync(int id)
        {
            var product = await _unitOfWork.InvestmentProducts.GetByIdAsync(id);
            if (product != null)
            {
                _unitOfWork.InvestmentProducts.Remove(product);
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

        public async Task<PagedResult<InvestmentProduct>> GetPagedDeletedProductsAsync(int page = 1, int pageSize = 10, string search = "")
        {
            var all = await _unitOfWork.InvestmentProducts.GetAllAsync();
            var query = all.Where(p => !p.IsActive);
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(p => p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) || p.Code.Contains(search, StringComparison.OrdinalIgnoreCase));

            }

            query = query.OrderByDescending(p => p.DeleteDate ?? DateTime.MinValue);
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

        public async Task<int> AutoDeleteExpiredProductsAsync(int retentionDays)
        {
            if (retentionDays <= 0) retentionDays = 30;

            var all = await _unitOfWork.InvestmentProducts.GetAllAsync();
            var cutoffDate = DateTime.UtcNow.AddDays(-retentionDays);
            var expiredProducts = all.Where(p => !p.IsActive && p.DeleteDate.HasValue && p.DeleteDate <= cutoffDate).ToList();

            if (expiredProducts.Any()) return 0;

            _unitOfWork.InvestmentProducts.RemoveRange(expiredProducts);

            await _unitOfWork.SaveChangesAsync();
            return expiredProducts.Count;
        }
    }
}
