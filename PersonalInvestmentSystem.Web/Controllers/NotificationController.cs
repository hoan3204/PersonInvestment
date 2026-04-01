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
                    id = n.Id,
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

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return BadRequest();
            if (id <= 0) return BadRequest();

            var notification = await _unitOfWork.Notifications.GetByIdAsync(id);

            if (notification != null && notification.UserId == userId)
            {
                notification.IsRead = true;
                _unitOfWork.Notifications.Update(notification);
                await _unitOfWork.SaveChangesAsync();
            }
            return Ok();
        }

        // Đánh dấu tất cả là đã đọc (tùy chọn)
        [HttpPost]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return BadRequest();

            var notifications = await _unitOfWork.Notifications.FindAsync(n => n.UserId == userId && !n.IsRead);
            foreach (var noti in notifications)
            {
                noti.IsRead = true;
                _unitOfWork.Notifications.Update(noti);
            }
            await _unitOfWork.SaveChangesAsync();

            return Ok();
        }
    }
}
