namespace PersonalInvestmentSystem.Web.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string toEmail, string subject, string body);
        Task SendBuySuccessEmailAsync(string toEmail, string fullName, string productName, decimal amount);
        Task SendSellSuccessEmailAsync(string toEmail, string fullName, string productName, decimal amount);
    }
}
