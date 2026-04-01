using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PersonalInvestmentSystem.Web.UnitOfWork;
using System.Security.Claims;

namespace PersonalInvestmentSystem.Web.Controllers
{
    [Authorize]
    public class NotificationController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        public NotificationController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        //lay thong tin
        [HttpGet]
        public async Task<JsonResult> GetNotification()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Json(new { notifications = new List<object>(), unreadCount = 0 });
            var notifications = await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId);

            var result = notifications
                .OrderByDescending(n => n.CreatedDate)
                .Take(5)
                .Select(n => new
                { 
                    title = n.Title,
                    message = n.Message,
                    createdDate = n.CreatedDate.ToString("dd/MM/yyyy:mm"),
                    isRead = n.IsRead
                })
                .ToList();

            int unreadCount = notifications.Count(n => !n.IsRead);

            return Json(new
            {
                notifications = result,
                unreadCount = unreadCount
            });

        }
    }
}
