using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using System.Security.Claims;

namespace PersonalInvestmentSystem.Web.Controllers
{
    [Authorize]
    public class PortfolioController : Controller
    {
        private readonly IPortfolioService _portfolioService;
        private readonly IExportService _exportService;
        private readonly IAIService _aiService;
        public PortfolioController(IPortfolioService portfolioService, IExportService exportService, IAIService aiService)
        {
            _portfolioService = portfolioService;
            _exportService = exportService;
            _aiService = aiService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            var portfolio = await _portfolioService.GetUserPortfolioAsync(userId);
            return View(portfolio);
        }

        [HttpGet]
        public async Task<IActionResult> ExportExcel()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userFullName = User.Identity?.Name ?? "User";
            var portfolio = await _portfolioService.GetUserPortfolioAsync(userId ?? "");

            var bytes = _exportService.ExportPortfolioToExcel(portfolio, userFullName);

            return File(bytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                $"Portfolio_{userFullName}_{DateTime.Now:yyyyMMdd}.xlsx");
        }
        // Export PDF
        [HttpGet]
        public async Task<IActionResult> ExportPdf()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var userFullName = User.Identity?.Name ?? "User";
            var portfolio = await _portfolioService.GetUserPortfolioAsync(userId ?? "");

            var bytes = _exportService.ExportPortfolioToPdf(portfolio, userFullName);

            return File(bytes, "application/pdf", $"Portfolio_{userFullName}_{DateTime.Now:yyyyMMdd}.pdf");
        }

        //goi ai phan tich danh muc
        [HttpPost]
        public async Task<IActionResult> Analyze()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return RedirectToAction("Login", "Account");

            string analysis = await _aiService.AnalyzePortfolioAsync(userId);

            TempData["AI_Analysis"] = analysis;
            return RedirectToAction(nameof(Index));
        }
    }
}
