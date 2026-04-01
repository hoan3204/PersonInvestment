using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using PersonalInvestmentSystem.Web.Models;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using MailKit.Security;

namespace PersonalInvestmentSystem.Web.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = body };
            email.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
            _emailSettings.SmtpServer,
            _emailSettings.SmtpPort,
            SecureSocketOptions.StartTls // 🔥 QUAN TRỌNG
);
            await smtp.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendBuySuccessEmailAsync(string toEmail, string fullName, string productName, decimal amount)
        {
            string subject = "✅ InvestPro - Mua thành công";
            string body = $@"
                <h3>Xin chào {fullName},</h3>
                <p>Bạn đã mua thành công <strong>{productName}</strong> với số tiền <strong>{amount:N0} ₫</strong>.</p>
                <p>Cảm ơn bạn đã sử dụng InvestPro!</p>
                <br>
                <small>Trân trọng,<br>Đội ngũ InvestPro</small>";

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendSellSuccessEmailAsync(string toEmail, string fullName, string productName, decimal amount)
        {
            string subject = "✅ InvestPro - Bán thành công";
            string body = $@"
                <h3>Xin chào {fullName},</h3>
                <p>Bạn đã bán thành công <strong>{productName}</strong> và nhận được <strong>{amount:N0} ₫</strong>.</p>
                <p>Cảm ơn bạn đã sử dụng InvestPro!</p>
                <br>
                <small>Trân trọng,<br>Đội ngũ InvestPro</small>";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
