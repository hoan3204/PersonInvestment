using PersonalInvestmentSystem.Web.Domain.Entities;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.UnitOfWork;

namespace PersonalInvestmentSystem.Web.Services.Implementations
{
    public class ReviewService: IReviewService
    {
        private readonly IUnitOfWork _unitOfWork;
        public ReviewService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        public async Task<IEnumerable<Review>> GetReviewsByProductAsync(int productId)
        {
            return await _unitOfWork.Reviews.FindAsync(r => r.ProductId == productId);
        }

        public async Task<bool> AddReviewAsync(string userId, int productId,int rating, string comment)
        {
            var exists = await _unitOfWork.Reviews.FindAsync(r => r.UserId == userId && r.ProductId == productId);
            if (exists.Any()) return false;

            var review = new Review
            {
                UserId = userId,
                ProductId = productId,
                Rating = rating,
                Comment = comment,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Reviews.AddAsync(review);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }

        public async Task<double> GetAverageRatingAsync(int productId)
        {
            var review = await _unitOfWork.Reviews.FindAsync(r => r.ProductId == productId);
            if (!review.Any()) return 0;

            return review.Average(r => r.Rating);
        }
    }
}
