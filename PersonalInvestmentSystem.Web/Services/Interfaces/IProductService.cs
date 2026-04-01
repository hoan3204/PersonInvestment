using PersonalInvestmentSystem.Web.Domain.Entities;
using PersonalInvestmentSystem.Web.Domain.Enums;
using PersonalInvestmentSystem.Web.ViewModels.Product;

namespace PersonalInvestmentSystem.Web.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<InvestmentProduct>> GetAllProductsAsync();
        Task<InvestmentProduct?> GetProductByIdAsync(int id);
        Task<IEnumerable<InvestmentProduct>> GetFeaturedProductsAsync(int count = 8);
        Task<IEnumerable<InvestmentProduct>> GetProductsByTypeAsync(InvestmentType type);
        Task<IEnumerable<InvestmentProduct>> GetProductsByCategoryAsync(int categoryId);

        Task AddProductAsync(InvestmentProduct product);
        Task UpdateProductAsync(InvestmentProduct product);
        Task DeleteProductAsync(int id);

        //dung cho admin
        Task<PagedResult<InvestmentProduct>> GetPagedProductsAsync(int page = 1, int pageSize = 10, string search = "");
    }
}
