using Microsoft.AspNetCore.SignalR;
using PersonalInvestmentSystem.Web.Hubs;
using PersonalInvestmentSystem.Web.Services.Interfaces;

namespace PersonalInvestmentSystem.Web.Services.Implementations
{
    public class NotificationService : INotificationService
    {
        private readonly IHubContext<NotificationHub> _hubContext;
        public NotificationService(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }
            

        public async Task SendBuySuccessNotificationAsync(string userId, string productName, decimal amount)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification", "Mua thành công!", $"Bạn đã mua {productName} với tổng số tiền {amount} đ");
        }
        public async Task SendSellSuccessNotificationAsync(string userId, string productName, decimal amount)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification",
                "Bán thành công!",
                $"Bạn đã bán {productName} và nhận được {amount:N0} ₫");
        }

        public async Task SendLowBalanceWarningAsync(string userId, decimal currentBalance)
        {
            await _hubContext.Clients.User(userId).SendAsync("ReceiveNotification",
                "Cảnh báo số dư!",
                $"Số dư ví của bạn chỉ còn {currentBalance:N0} ₫. Hãy nạp thêm để tiếp tục giao dịch.");
        }
    }
}
