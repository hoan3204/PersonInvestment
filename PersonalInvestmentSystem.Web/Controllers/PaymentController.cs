using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PersonalInvestmentSystem.Web.Models;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.Models;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PersonalInvestmentSystem.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IWalletService _walletService;
        private readonly MoMoSettings _momoSettings;

        public PaymentController(IWalletService walletService, IOptions<MoMoSettings> momoSettings)
        {
            _walletService = walletService;
            _momoSettings = momoSettings.Value;
        }

        // GET: /Payment/Deposit
        public async Task<IActionResult> Deposit()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!string.IsNullOrEmpty(userId))
            {
                var currentBalance = await _walletService.GetBalanceAsync(userId);
                ViewBag.CurrentBalance = currentBalance;
            }
            else
            {
                ViewBag.CurrentBalance = 0;
            }

            return View();
        }

        // POST: Tạo yêu cầu thanh toán MoMo
        [HttpPost]
        public async Task<IActionResult> CreateMoMoPayment(decimal amount)
        {
            if (amount <= 0)
            {
                TempData["Error"] = "Số tiền nạp phải lớn hơn 0 ₫";
                return RedirectToAction("Deposit");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                string orderId = "INVEST_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string requestId = Guid.NewGuid().ToString();

                // Tạo raw signature theo chuẩn MoMo
                string rawSignature = $"accessKey={_momoSettings.AccessKey}" +
                                      $"&amount={amount}" +
                                      $"&extraData=" +
                                      $"&ipnUrl={_momoSettings.IpnUrl}" +
                                      $"&orderId={orderId}" +
                                      $"&orderInfo=Nạp tiền vào ví InvestPro" +
                                      $"&partnerCode={_momoSettings.PartnerCode}" +
                                      $"&redirectUrl={_momoSettings.ReturnUrl}" +
                                      $"&requestId={requestId}" +
                                      $"&requestType=payWithMethod";

                string signature = GetSignature(rawSignature, _momoSettings.SecretKey);

                var requestBody = new
                {
                    partnerCode = _momoSettings.PartnerCode,
                    partnerName = "InvestPro",
                    storeId = "InvestProStore",
                    requestId = requestId,
                    amount = (long)amount,
                    orderId = orderId,
                    orderInfo = "Nạp tiền vào ví InvestPro",
                    redirectUrl = _momoSettings.ReturnUrl,
                    ipnUrl = _momoSettings.IpnUrl,
                    extraData = userId,           // Quan trọng: truyền userId để callback biết ai nạp
                    requestType = "payWithMethod",
                    signature = signature,
                    lang = "vi"
                };

                using var client = new HttpClient();
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_momoSettings.Endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                var momoResponse = JsonSerializer.Deserialize<MoMoResponse>(responseString);

                if (momoResponse?.resultCode == 0)
                {
                    return Redirect(momoResponse.payUrl);   // Chuyển sang trang thanh toán MoMo
                }
                else
                {
                    TempData["Error"] = "Không thể tạo link MoMo: " + momoResponse?.message;
                    return RedirectToAction("Deposit");
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi hệ thống: " + ex.Message;
                return RedirectToAction("Deposit");
            }
        }

        // Callback khi người dùng thanh toán xong (Return URL)
        [HttpGet]
        public async Task<IActionResult> MoMoReturn(string orderId, string resultCode, string amount, string extraData)
        {
            if (resultCode == "0") // Thành công
            {
                var userId = extraData;
                decimal depositAmount = decimal.Parse(amount);

                await _walletService.UpdateBalanceAsync(userId, depositAmount, true);

                TempData["Success"] = $"Nạp thành công {depositAmount:N0} ₫ vào ví!";
            }
            else
            {
                TempData["Error"] = "Thanh toán MoMo thất bại hoặc bị hủy.";
            }

            return RedirectToAction("Index", "Wallet");
        }

        // Hàm tạo chữ ký
        private string GetSignature(string text, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
    }

    // Class để deserialize response từ MoMo
    public class MoMoResponse
    {
        public int resultCode { get; set; }
        public string message { get; set; } = string.Empty;
        public string payUrl { get; set; } = string.Empty;
    }
}