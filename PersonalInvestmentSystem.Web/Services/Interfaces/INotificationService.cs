namespace PersonalInvestmentSystem.Web.Services.Interfaces
{
    public interface INotificationService
    {
        Task SendBuySuccessNotificationAsync(string userId, string productName, decimal amount);
        Task SendSellSuccessNotificationAsync(string userId, string productName, decimal amount);
        Task SendLowBalanceWarningAsync(string userId, decimal currentBalece);
    }
}
