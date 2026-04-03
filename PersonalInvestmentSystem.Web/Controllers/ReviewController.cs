using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using System.Security.Claims;

namespace PersonalInvestmentSystem.Web.Controllers
{
    [Authorize]
    public class ReviewController : Controller
    {
        private readonly IReviewService _reviewService;
        public ReviewController(IReviewService reviewService)
        {
            _reviewService = reviewService;
        }

        [HttpPost] 
        public async Task<IActionResult> Add(int productId, int rating, string comment)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool success = await _reviewService.AddReviewAsync(userId ?? "", productId, rating, comment);
            if (success)
                TempData["success"] = "Cảm ơn bạn đã đánh giá sản phẩm.";
            else
                TempData["error"] = "Bạn đã đánh giá sản phẩm này rồi!";
            return RedirectToAction("Details", "Product", new { id = productId });
        }
    }
}
