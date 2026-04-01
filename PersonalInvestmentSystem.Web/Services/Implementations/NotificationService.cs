using Microsoft.AspNetCore.SignalR;
using PersonalInvestmentSystem.Web.Hubs;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.Hubs;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.UnitOfWork;
using PersonalInvestmentSystem.Web.Domain.Entities;

namespace PersonalInvestmentSystem.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationService(IHubContext<NotificationHub> hubContext, IUnitOfWork unitOfWork)
        {
            _hubContext = hubContext;
            _unitOfWork = unitOfWork;
        }

        public async Task SendBuySuccessNotificationAsync(string userId, string productName, decimal amount)
        {
            string title = "Mua thành công!";
            string message = $"Bạn đã mua {productName} với tổng số tiền {amount} đ";

            var notification = new Notification
            { 
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", title, message);

        }

        public async Task SendSellSuccessNotificationAsync(string userId, string productName, decimal amount)
        {
            string title = "Bán thành công!";
            string message = $"Bạn đã bán {productName} và nhận được {amount:N0} ₫";

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", title, message);
        }

        public async Task SendLowBalanceWarningAsync(string userId, decimal currentBalance)
        {
            string title = "Cảnh báo số dư!";
            string message = $"Số dư ví của bạn chỉ còn {currentBalance:N0} ₫. Hãy nạp thêm.";

            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                IsRead = false,
                CreatedDate = DateTime.UtcNow
            };

            await _unitOfWork.Notifications.AddAsync(notification);
            await _unitOfWork.SaveChangesAsync();

            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", title, message);
        }
    }
}