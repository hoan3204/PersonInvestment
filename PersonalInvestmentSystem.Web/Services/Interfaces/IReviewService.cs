using PersonalInvestmentSystem.Web.Domain.Entities;

namespace PersonalInvestmentSystem.Web.Services.Interfaces
{
    public interface IReviewService
    {
        Task<IEnumerable<Review>> GetReviewsByProductAsync(int product);
        Task<bool> AddReviewAsync(string userId, int productId, int rating, string comment);
        Task<double> GetAverageRatingAsync(int productId);
    }
}
