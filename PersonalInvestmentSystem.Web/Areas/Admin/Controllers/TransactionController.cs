using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalInvestmentSystem.Web.UnitOfWork;

namespace PersonalInvestmentSystem.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles ="Admin")]
    public class TransactionController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public TransactionController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var transaction = await _unitOfWork.Transactions.GetAllAsync(
                t => t.User, 
                t => t.Product
            );

            return View(transaction.OrderByDescending(t => t.CreatedDate));
        }

        public async Task<IActionResult> Details(int id)
        {
            var transaction = await _unitOfWork.Transactions.GetByIdAsync(id, t => t.User, t => t.Product);

            if (transaction == null) return NotFound();
            return View(transaction);
        }
    }
}
