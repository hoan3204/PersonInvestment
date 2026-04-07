using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PersonalInvestmentSystem.Web.Domain.Entities;
using PersonalInvestmentSystem.Web.UnitOfWork;

namespace PersonalInvestmentSystem.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize (Roles ="Admin")]
    public class UserController : Controller
    {
        private readonly UserManager<AppUser> _userManager;

        public UserController(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        public async Task<IActionResult> Details(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();
            return View(user);
        }
    }
}
