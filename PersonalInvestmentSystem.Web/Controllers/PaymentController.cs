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
using Microsoft.Extensions.Caching.Memory;

namespace PersonalInvestmentSystem.Controllers
{
    [Authorize]
    public class PaymentController : Controller
    {
        private readonly IWalletService _walletService;
        private readonly MoMoSettings _momoSettings;
        private readonly IMemoryCache _memoryCache;
        private readonly IEmailService _emailService;
        public PaymentController(IWalletService walletService, IOptions<MoMoSettings> momoSettings, IMemoryCache memoryCache, IEmailService emailService)
        {
            _walletService = walletService;
            _momoSettings = momoSettings.Value;
            _memoryCache = memoryCache;
            _emailService = emailService;
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
                if (!IsMoMoConfigValid())
                {
                    TempData["Error"] = "Thông tin MoMo chưa đúng. Vui lòng cập nhật PartnerCode/AccessKey/SecretKey tài khoản đã được kích hoạt.";
                    return RedirectToAction("Deposit");
                }
                var redirectUrl = BuildCallbackUrl("MoMoReturn", _momoSettings.ReturnUrl);
                var ipnUrl = BuildCallbackUrl("MoMoNotify", _momoSettings.EffectiveIpnUrl);
                if (string.IsNullOrWhiteSpace(redirectUrl) || string.IsNullOrWhiteSpace(ipnUrl))
                {
                    TempData["Error"] = "Không tạo được callback URL cho MoMo.";
                    return RedirectToAction("Deposit");
                }
                string orderId = "INVEST_" + DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string requestId = Guid.NewGuid().ToString();
                var requestType = _momoSettings.EffectiveRequestType;
                var amountValue = (long)amount;
                var extraData = userId;

                // Tạo raw signature theo chuẩn MoMo
                string rawSignature = $"accessKey={_momoSettings.AccessKey}" +
                                      $"&amount={amountValue}" +
                                      $"&extraData={extraData}" +
                                      $"&ipnUrl={ipnUrl}" +
                                      $"&orderId={orderId}" +
                                      $"&orderInfo=Nạp tiền vào ví InvestPro" +
                                      $"&partnerCode={_momoSettings.PartnerCode}" +
                                      $"&redirectUrl={redirectUrl}" +
                                      $"&requestId={requestId}" +
                                      $"&requestType={requestType}";

                string signature = GetSignature(rawSignature, _momoSettings.SecretKey);

                var requestBody = new
                {
                    partnerCode = _momoSettings.PartnerCode,
                    partnerName = _momoSettings.PartnerName,
                    storeId = _momoSettings.StoreId,
                    requestId = requestId,
                    amount = amountValue,
                    orderId = orderId,
                    orderInfo = "Nạp tiền vào ví InvestPro",
                    redirectUrl = redirectUrl,
                    ipnUrl = ipnUrl,
                    extraData = extraData,           // Quan trọng: truyền userId để callback biết ai nạp
                    requestType = requestType,
                    signature = signature,
                    lang = "vi"
                };

                using var client = new HttpClient();
                var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

                var response = await client.PostAsync(_momoSettings.Endpoint, content);
                var responseString = await response.Content.ReadAsStringAsync();

                var momoResponse = JsonSerializer.Deserialize<MoMoResponse>(responseString);

                if (response.IsSuccessStatusCode && momoResponse?.resultCode == 0 && !string.IsNullOrWhiteSpace(momoResponse.payUrl))
                {
                    return Redirect(momoResponse.payUrl);   // Chuyển sang trang thanh toán MoMo
                }
                else
                {
                    var momoMessage = momoResponse?.message ?? responseString;
                    if (momoMessage.Contains("Cấu hình doanh nghiệp không chính xác", StringComparison.OrdinalIgnoreCase) ||
                        momoMessage.Contains("tài khoản không hoạt động", StringComparison.OrdinalIgnoreCase))
                    {
                        TempData["Error"] = "Tài khoản MoMo Merchant chưa được kích hoạt đúng cho API/QR. Kiểm tra PartnerCode, AccessKey, SecretKey và trạng thái tài khoản trên MoMo Business.";
                    }
                    else
                    {
                        TempData["Error"] = "Không thể tạo link MoMo: " + momoMessage;
                    }
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
        [AllowAnonymous]
        public async Task<IActionResult> MoMoReturn(string orderId, string resultCode, string amount, string extraData)
        {
            if (resultCode == "0") // Thành công
            {
                var userId = !string.IsNullOrWhiteSpace(extraData)
                    ? extraData
                    : User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrWhiteSpace(userId)
                    && decimal.TryParse(amount, out var depositAmount)
                    && TryMarkOrderProcessed(orderId))
                {
                    await _walletService.UpdateBalanceAsync(userId, depositAmount, true);
                    TempData["Success"] = $"Nạp thành công {depositAmount:N0} ₫ vào ví!";
                }
                else
                {
                    TempData["Error"] = "Đã thanh toán thành công nhưng chưa thể cộng ví. Vui lòng liên hệ hỗ trợ với mã đơn " + orderId;
                }
            }
            else
            {
                TempData["Error"] = "Thanh toán MoMo thất bại hoặc bị hủy.";
            }

            if (User?.Identity?.IsAuthenticated == true)
            {

                return RedirectToAction("Index", "Wallet");
            }

            return RedirectToAction("Login", "Account", new
            {
                returnUrl = Url.Action("Index", "Wallet")
            });
        }
        // Callback server-to-server từ MoMo (IPN)
        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> MoMoNotify([FromBody] MoMoNotifyRequest request)
        {
            if (request == null || request.resultCode != 0)
            {
                return Ok(new { message = "Ignored" });
            }

            if (string.IsNullOrWhiteSpace(request.extraData)
                || request.amount <= 0
                || !TryMarkOrderProcessed(request.orderId))
            {
                return Ok(new { message = "Skipped" });
            }

            await _walletService.UpdateBalanceAsync(request.extraData, request.amount, true);
            return Ok(new { message = "Success" });
        }
        // Hàm tạo chữ ký
        private string GetSignature(string text, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(text));
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        }
        private bool IsMoMoConfigValid()
        {
            return !string.IsNullOrWhiteSpace(_momoSettings.PartnerCode)
                && !string.IsNullOrWhiteSpace(_momoSettings.AccessKey)
                && !string.IsNullOrWhiteSpace(_momoSettings.SecretKey)
                && !_momoSettings.PartnerCode.Contains("YOUR_", StringComparison.OrdinalIgnoreCase)
                && !_momoSettings.AccessKey.Contains("YOUR_", StringComparison.OrdinalIgnoreCase)
                && !_momoSettings.SecretKey.Contains("YOUR_", StringComparison.OrdinalIgnoreCase);
        }
        private bool TryMarkOrderProcessed(string? orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return false;
            }

            var cacheKey = $"momo_paid_{orderId}";
            if (_memoryCache.TryGetValue(cacheKey, out _))
            {
                return false;
            }

            _memoryCache.Set(cacheKey, true, TimeSpan.FromHours(1));
            return true;
        }
        private string BuildCallbackUrl(string actionName, string configuredUrl)
        {
            if (!string.IsNullOrWhiteSpace(configuredUrl))
            {
                return configuredUrl;
            }

            return Url.Action(
                action: actionName,
                controller: "Payment",
                values: null,
                protocol: Request.Scheme,
                host: Request.Host.ToString()) ?? string.Empty;
        }
    }

    // Class để deserialize response từ MoMo
    public class MoMoResponse
    {
        public int resultCode { get; set; }
        public string message { get; set; } = string.Empty;
        public string payUrl { get; set; } = string.Empty;
    }
    public class MoMoNotifyRequest
    {
        public string orderId { get; set; } = string.Empty;
        public int resultCode { get; set; }
        public decimal amount { get; set; }
        public string extraData { get; set; } = string.Empty;
    }
}