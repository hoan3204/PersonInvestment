using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using System.Security.Claims;

namespace PersonalInvestmentSystem.Web.Controllers
{
    [Authorize]
    public class TransactionController : Controller
    {
        private readonly ITransactionService _transactionService;
        private readonly IProductService _productService;
        private readonly IWalletService _walletService;

        public TransactionController(ITransactionService transactionService, IProductService productService, IWalletService walletService)
        {
            _transactionService = transactionService;
            _productService = productService;
            _walletService = walletService;
        }

        //mua san pham
        [HttpPost]
        public async Task<IActionResult> Buy(int productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            bool success = await _transactionService.BuyProductAsync(userId, productId, quantity);

            if (success)
            {
                TempData["success"] = "Mua thanh cong";
                return RedirectToAction("Details", "Product", new { id = productId });
            }

            TempData["error"] = "mua that bai: khong co tien haha";
            return RedirectToAction("Details", "Product", new {id = productId});
        }

        [HttpPost]
        public async Task<IActionResult> Sell(int productId, int quantity = 1)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");

            bool success = await _transactionService.SellProductAsync(userId, productId, quantity);

            if (success)
            {
                TempData["success"] = "ban thanh cong";
                return RedirectToAction("Details", "Product", new { id = productId });
            }
            TempData["error"] = "ban that bai";
            return RedirectToAction("Details", "Product", new { id = productId });
        }

        //lich su giao dich
        public async Task<IActionResult> History()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var transaction = await _transactionService.GetUserTransactionsAsync(userId ?? "");
            return View(transaction);
        }
    }
}
