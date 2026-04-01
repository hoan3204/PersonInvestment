using PersonalInvestmentSystem.Web.Domain.Entities;
using PersonalInvestmentSystem.Web.Domain.Enums;
using PersonalInvestmentSystem.Web.Data;

namespace PersonalInvestmentSystem.Web.Data.Seed

{
    public static class SeedData
    {
        public static async Task InitializeAsync(ApplicationDbContext context)
        {
            // Nếu đã có dữ liệu thì không seed nữa
            if (context.Categories.Any())
            {
                Console.WriteLine("⏭️ Seed data already exists. Skipping...");
                return;
            }

            Console.WriteLine("🌱 Starting to seed data...");

            try
            {
                // 1. Seed Categories
                var categories = new List<Category>
            {
                new Category { Name = "Cổ phiếu", Slug = "co-phieu", IsActive = true },
                new Category { Name = "Trái phiếu", Slug = "trai-phieu", IsActive = true },
                new Category { Name = "Quỹ đầu tư", Slug = "quy-dau-tu", IsActive = true },
                new Category { Name = "Vàng", Slug = "vang", IsActive = true },
                new Category { Name = "Tiền điện tử", Slug = "tien-dien-tu", IsActive = true }
            };
            await context.Categories.AddRangeAsync(categories);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Categories seeded successfully!");

            // 2. Seed Publishers (Nhà phát hành)
            var publishers = new List<Publisher>
            {
                new Publisher { Name = "VNDIRECT", Description = "Công ty chứng khoán VNDIRECT", IsActive = true },
                new Publisher { Name = "SSI", Description = "Công ty Chứng khoán SSI", IsActive = true },
                new Publisher { Name = "TCBS", Description = "Techcom Securities", IsActive = true },
                new Publisher { Name = "Binance", Description = "Sàn giao dịch tiền điện tử", IsActive = true },
                new Publisher { Name = "PNJ", Description = "Công ty Vàng bạc đá quý PNJ", IsActive = true }
            };
            await context.Publishers.AddRangeAsync(publishers);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Publishers seeded successfully!");

            // 3. Seed Investment Products (20 sản phẩm mẫu)
            var products = new List<InvestmentProduct>
            {
                new InvestmentProduct
                {
                    Name = "Cổ phiếu VCB", Code = "VCB",
                    Description = "Ngân hàng TMCP Ngoại thương Việt Nam",
                    Type = InvestmentType.Stock, CategoryId = 1, PublisherId = 1,
                    CurrentPrice = 95000, PreviousPrice = 92000, ChangePercent = 3.26m,
                    RiskLevel = RiskLevel.Low, IsFeatured = true, IsActive = true,
                    ImageUrl = "https://via.placeholder.com/300x200/0066cc/ffffff?text=VCB"
                },
                new InvestmentProduct
                {
                    Name = "Cổ phiếu VIC", Code = "VIC",
                    Description = "Tập đoàn Vingroup",
                    Type = InvestmentType.Stock, CategoryId = 1, PublisherId = 2,
                    CurrentPrice = 45000, PreviousPrice = 46000, ChangePercent = -2.17m,
                    RiskLevel = RiskLevel.Medium, IsFeatured = true, IsActive = true,
                    ImageUrl = "https://via.placeholder.com/300x200/cc0000/ffffff?text=VIC"
                },
                new InvestmentProduct
                {
                    Name = "Trái phiếu Chính phủ 5 năm", Code = "TP05",
                    Description = "Trái phiếu kho bạc nhà nước kỳ hạn 5 năm",
                    Type = InvestmentType.Bond, CategoryId = 2, PublisherId = 3,
                    CurrentPrice = 102500, PreviousPrice = 102000, ChangePercent = 0.49m,
                    RiskLevel = RiskLevel.Low, IsFeatured = false, IsActive = true,
                    ImageUrl = "https://via.placeholder.com/300x200/009900/ffffff?text=BOND"
                },
                new InvestmentProduct
                {
                    Name = "Quỹ Dragon Capital VFM", Code = "DCVFM",
                    Description = "Quỹ mở Dragon Capital Việt Nam",
                    Type = InvestmentType.Fund, CategoryId = 3, PublisherId = 1,
                    CurrentPrice = 28500, PreviousPrice = 27800, ChangePercent = 2.52m,
                    RiskLevel = RiskLevel.Medium, IsFeatured = true, IsActive = true,
                    ImageUrl = "https://via.placeholder.com/300x200/3366ff/ffffff?text=FUND"
                },
                new InvestmentProduct
                {
                    Name = "Vàng SJC 9999", Code = "SJC",
                    Description = "Vàng miếng SJC tiêu chuẩn",
                    Type = InvestmentType.Gold, CategoryId = 4, PublisherId = 5,
                    CurrentPrice = 8250000, PreviousPrice = 8180000, ChangePercent = 0.86m,
                    RiskLevel = RiskLevel.Low, IsFeatured = true, IsActive = true,
                    ImageUrl = "https://via.placeholder.com/300x200/ffcc00/000000?text=GOLD"
                },
                new InvestmentProduct
                {
                    Name = "Bitcoin (BTC)", Code = "BTC",
                    Description = "Tiền điện tử Bitcoin",
                    Type = InvestmentType.Crypto, CategoryId = 5, PublisherId = 4,
                    CurrentPrice = 1650000000, PreviousPrice = 1580000000, ChangePercent = 4.43m,
                    RiskLevel = RiskLevel.High, IsFeatured = true, IsActive = true,
                    ImageUrl = "https://via.placeholder.com/300x200/ff9900/ffffff?text=BTC"
                }
                // Bạn có thể thêm thêm nhiều sản phẩm nữa (tôi để sẵn 6 cái để test nhanh)
            };

            await context.InvestmentProducts.AddRangeAsync(products);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Investment Products seeded successfully!");

            // 4. Seed Banners
            var banners = new List<Banner>
            {
                new Banner { Title = "Khuyến mãi đầu tư cổ phiếu", ImageUrl = "https://via.placeholder.com/1200x400/0066cc/ffffff?text=Banner+Co+Phieu", LinkUrl = "/Product", DisplayOrder = 1, IsActive = true },
                new Banner { Title = "Vàng SJC đang tăng mạnh", ImageUrl = "https://via.placeholder.com/1200x400/ffcc00/000000?text=Banner+Vang", LinkUrl = "/Product", DisplayOrder = 2, IsActive = true }
            };
            await context.Banners.AddRangeAsync(banners);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ Banners seeded successfully!");

            // 5. Seed News (Tin tức tài chính)
            var news = new List<News>
            {
                new News { Title = "Thị trường chứng khoán Việt Nam tăng điểm mạnh đầu tuần", Content = "VN-Index tăng hơn 20 điểm...", ImageUrl = "https://via.placeholder.com/600x400/009900/ffffff?text=Tin+Chung+Khoan", Author = "Admin", IsActive = true },
                new News { Title = "Giá Bitcoin vượt mốc 100.000 USD", Content = "Tiền điện tử tiếp tục đà tăng...", ImageUrl = "https://via.placeholder.com/600x400/ff9900/ffffff?text=Bitcoin", Author = "Admin", IsActive = true }
            };
            await context.News.AddRangeAsync(news);
            await context.SaveChangesAsync();
            Console.WriteLine("✅ News seeded successfully!");

            Console.WriteLine("✅ Seed Data completed successfully!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error seeding data: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw;
            }
        }
    }
}
