using Microsoft.AspNetCore.Mvc;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using System.Security.Claims;

namespace PersonalInvestmentSystem.Web.Controllers
{
    public class WatchListController : Controller
    {
        private readonly IWatchlistService _watchlistService;
        public WatchListController(IWatchlistService watchlistService)
        {
            _watchlistService = watchlistService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var items = await _watchlistService.GetUserWatchListAsync(userId ?? "");

            return View(items);
        }

        [HttpPost]
        public async Task<IActionResult> Add(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            bool success = await _watchlistService.AddToWatchlistAsync(userId ?? "", productId);

            if (success)
                TempData["success"] = "Đã thêm vào danh sách yêu thích.";
            else
                TempData["Info"] = "Sản phẩm đã có trong danh sách yêu thích.";

            return RedirectToAction("Details", "Product", new { id = productId });
        }

        [HttpPost]
        public async Task<IActionResult> Remove(int productId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _watchlistService.RemoveFromWatchListAsync(userId ?? "", productId);
            TempData["success"] = "Đã xóa khỏi danh sách yêu thích.";
            return RedirectToAction(nameof(Index));
        }
    }
}
