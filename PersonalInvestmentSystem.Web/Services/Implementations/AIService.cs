using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.UnitOfWork;
using System.Text.Json;
using System.Text;

namespace PersonalInvestmentSystem.Web.Services.Implementations
{
    public class AIService : IAIService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;

        public AIService(IUnitOfWork unitOfWork, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _configuration = configuration;
        }

        public async Task<string> AnalyzePortfolioAsync(string userId)
        {
            // Lấy danh sách Portfolio
            var portfolios = await _unitOfWork.Portfolios.FindAsync(
                p => p.UserId == userId && p.Quantity > 0
            );

            if (!portfolios.Any())
            {
                return "Hiện tại bạn chưa có sản phẩm nào trong danh mục đầu tư.";
            }

            // Load Product thủ công để tránh null
            var summary = new List<object>();

            foreach (var p in portfolios)
            {
                // Load Product nếu chưa có
                if (p.Product == null)
                {
                    p.Product = await _unitOfWork.InvestmentProducts.GetByIdAsync(p.ProductId);
                }

                if (p.Product == null) continue;

                summary.Add(new
                {
                    Name = p.Product.Name,
                    Code = p.Product.Code,
                    Quantity = p.Quantity,
                    AvgBuyPrice = p.AverageBuyPrice,
                    CurrentPrice = p.Product.CurrentPrice,
                    Profit = (p.Product.CurrentPrice - p.AverageBuyPrice) * p.Quantity,
                    RiskLevel = p.Product.RiskLevel.ToString()
                });
            }

            if (!summary.Any())
            {
                return "Không thể tải thông tin sản phẩm trong danh mục.";
            }

            string portfolioJson = JsonSerializer.Serialize(summary);

            string prompt = $@"
                Bạn là chuyên gia phân tích đầu tư tài chính tại Việt Nam. 
                Hãy phân tích danh mục đầu tư sau và đưa ra lời khuyên rõ ràng, ngắn gọn bằng tiếng Việt:

                Danh mục đầu tư:
                {portfolioJson}

                Yêu cầu phân tích:
                1. Tổng giá trị danh mục hiện tại
                2. Lợi nhuận/Lỗ tổng thể
                3. Mức độ rủi ro tổng thể
                4. Lời khuyên cụ thể (nên giữ hay bán, điều chỉnh danh mục như thế nào)

                Trả lời chuyên nghiệp, dễ hiểu, có số liệu cụ thể.";

            return await CallOpenAI(prompt);
        }
        private async Task<string> CallOpenAI(string prompt)
        {
            var apiKey = _configuration["AI:ApiKey"];
            var model = _configuration["AI:Model"] ?? "gpt-4o-mini";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("Authorization", $"Bearer {apiKey}");

            var requestBody = new
            {
                model = model,
                messages = new[]
                {
                    new { role = "system", content = "Bạn là chuyên gia phân tích đầu tư tài chính." },
                    new { role = "user", content = prompt }
                },
                temperature = 0.7,
                max_tokens = 800
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");

            var response = await client.PostAsync("https://api.openai.com/v1/chat/completions", content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                return $"Lỗi khi gọi AI: {responseString}";
            }

            var json = JsonDocument.Parse(responseString);
            return json.RootElement
                       .GetProperty("choices")[0]
                       .GetProperty("message")
                       .GetProperty("content")
                       .GetString() ?? "Không nhận được phản hồi từ AI.";
        }

        // Các method khác (AnalyzeProductAsync, GetInvestmentAdviceAsync) bạn có thể bổ sung sau
        public Task<string> AnalyzeProductAsync(int productId, string userQuestion = "") => Task.FromResult("Chức năng đang phát triển.");
        public Task<string> GetInvestmentAdviceAsync(string userId, string question) => Task.FromResult("Chức năng đang phát triển.");
    }
}
    

