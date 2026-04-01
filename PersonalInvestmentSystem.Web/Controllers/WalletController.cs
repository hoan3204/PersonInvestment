using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using System.Security.Claims;

namespace PersonalInvestmentSystem.Web.Controllers
{
    [Authorize]
    public class WalletController : Controller
    {
        private readonly IWalletService _walletService;

        public WalletController(IWalletService walletService)
        {
            _walletService = walletService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return RedirectToAction("Login", "Account");
            var wallet =await _walletService.GetWalletByUserIdAsync(userId);
            ViewBag.Balance = wallet?.Balance ?? 0;
            return View(wallet);
        }

        //api lay so du
        [HttpGet]
        public async Task<JsonResult> GetBalance()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var balance = await _walletService.GetBalanceAsync(userId);
            return Json(new { balance = balance.ToString("N0") + " đ" });
        }
    }
}
