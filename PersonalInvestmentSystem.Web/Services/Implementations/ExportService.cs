using ClosedXML.Excel;
using iText.IO.Font;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using PersonalInvestmentSystem.Web.Services.Interfaces;
using PersonalInvestmentSystem.Web.ViewModels.Portfolio;

namespace PersonalInvestmentSystem.Web.Services.Implementations
{
    public class ExportService : IExportService
    {
        public byte[] ExportPortfolioToExcel(PortfolioViewModel portfolio, string userFullName)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Portfolio");

            worksheet.Cell(1, 1).Value = "BÁO CÁO DANH MỤC ĐẦU TƯ";
            worksheet.Cell(2, 1).Value = $"Người dùng: {userFullName}";
            worksheet.Cell(3, 1).Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";

            worksheet.Cell(5, 1).Value = "Sản phẩm";
            worksheet.Cell(5, 2).Value = "Mã";
            worksheet.Cell(5, 3).Value = "Số lượng";
            worksheet.Cell(5, 4).Value = "Giá TB";
            worksheet.Cell(5, 5).Value = "Giá hiện tại";
            worksheet.Cell(5, 6).Value = "Giá trị hiện tại";
            worksheet.Cell(5, 7).Value = "Lãi/Lỗ";

            int row = 6;
            foreach (var item in portfolio.Items)
            {
                worksheet.Cell(row, 1).Value = item.ProductName;
                worksheet.Cell(row, 2).Value = item.ProductCode;
                worksheet.Cell(row, 3).Value = item.Quantity;
                worksheet.Cell(row, 4).Value = item.AverageBuyPrice;
                worksheet.Cell(row, 5).Value = item.CurrentPrice;
                worksheet.Cell(row, 6).Value = item.Quantity * item.CurrentPrice;
                worksheet.Cell(row, 7).Value = item.Profit;
                row++;
            }

            worksheet.Cell(row, 1).Value = "TỔNG";
            worksheet.Cell(row, 6).Value = portfolio.TotalValue;
            worksheet.Cell(row, 7).Value = portfolio.TotalProfit;

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportPortfolioToPdf(PortfolioViewModel portfolio, string userFullName)
        {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            
            // Default font supports Unicode/Vietnamese characters
            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            // Title
            var title = new Paragraph("BÁO CÁO DANH MỤC ĐẦU TƯ")
                .SetFont(font)
                .SetFontSize(18);
            document.Add(title);

            document.Add(new Paragraph($"Người dùng: {userFullName}").SetFont(font).SetFontSize(12));
            document.Add(new Paragraph($"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}").SetFont(font).SetFontSize(12));
            document.Add(new Paragraph("\n"));

            // Bảng
            var table = new Table(7);
            table.SetWidth(UnitValue.CreatePercentValue(100));

            table.AddHeaderCell(new Cell().Add(new Paragraph("Sản phẩm").SetFont(font)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Mã").SetFont(font)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("SL").SetFont(font)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Giá TB").SetFont(font)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Giá HT").SetFont(font)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Giá trị").SetFont(font)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Lãi/Lỗ").SetFont(font)));

            foreach (var item in portfolio.Items)
            {
                table.AddCell(new Cell().Add(new Paragraph(item.ProductName).SetFont(font)));
                table.AddCell(new Cell().Add(new Paragraph(item.ProductCode).SetFont(font)));
                table.AddCell(new Cell().Add(new Paragraph(item.Quantity.ToString()).SetFont(font)));
                table.AddCell(new Cell().Add(new Paragraph(item.AverageBuyPrice.ToString("N0")).SetFont(font)));
                table.AddCell(new Cell().Add(new Paragraph(item.CurrentPrice.ToString("N0")).SetFont(font)));
                table.AddCell(new Cell().Add(new Paragraph((item.Quantity * item.CurrentPrice).ToString("N0")).SetFont(font)));
                table.AddCell(new Cell().Add(new Paragraph(item.Profit.ToString("N0")).SetFont(font)));
            }

            document.Add(table);
            document.Close();

            return stream.ToArray();
        }

        public byte[] ExportTransactionsToExcel(IEnumerable<PersonalInvestmentSystem.Web.Domain.Entities.Transaction> transactions)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Giao dịch");

            worksheet.Cell(1, 1).Value = "LỊCH SỬ GIAO DỊCH";
            worksheet.Cell(2, 1).Value = $"Ngày xuất: {DateTime.Now:dd/MM/yyyy HH:mm}";

            worksheet.Cell(4, 1).Value = "Ngày";
            worksheet.Cell(4, 2).Value = "Người dùng";
            worksheet.Cell(4, 3).Value = "Sản phẩm";
            worksheet.Cell(4, 4).Value = "Loại";
            worksheet.Cell(4, 5).Value = "Số lượng";
            worksheet.Cell(4, 6).Value = "Giá";
            worksheet.Cell(4, 7).Value = "Tổng tiền";

            int row = 5;
            foreach (var t in transactions.OrderByDescending(x => x.CreatedDate))
            {
                worksheet.Cell(row, 1).Value = t.CreatedDate;
                worksheet.Cell(row, 2).Value = t.User?.FullName;
                worksheet.Cell(row, 3).Value = t.Product?.Name;
                worksheet.Cell(row, 4).Value = t.Type.ToString();
                worksheet.Cell(row, 5).Value = t.Quantity;
                worksheet.Cell(row, 6).Value = t.Price;
                worksheet.Cell(row, 7).Value = t.TotalAmount;
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public byte[] ExportTransactionsToPdf(IEnumerable<PersonalInvestmentSystem.Web.Domain.Entities.Transaction> transactions)
        {
            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            
            // Default font supports Unicode/Vietnamese characters
            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);

            var title = new Paragraph("LỊCH SỬ GIAO DỊCH")
                .SetFont(font)
                .SetFontSize(18);
            document.Add(title);

            var table = new Table(7);
            table.SetWidth(UnitValue.CreatePercentValue(100));

            table.AddHeaderCell(new Cell().Add(new Paragraph("Ngày").SetFont(font)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Người dùng").SetFont(font)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Sản phẩm").SetFont(font)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Ngày").SetFont(font)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Người dùng").SetFont(font)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Sản phẩm").SetFont(font)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Loại").SetFont(font)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("SL").SetFont(font)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Giá").SetFont(font)));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Tổng").SetFont(font)));

            foreach (var t in transactions.OrderByDescending(x => x.CreatedDate))
            {
                table.AddCell(new Cell().Add(new Paragraph(t.CreatedDate.ToString("dd/MM/yyyy HH:mm")).SetFont(font)));
                table.AddCell(new Cell().Add(new Paragraph(t.User?.FullName ?? "").SetFont(font)));
                table.AddCell(new Cell().Add(new Paragraph(t.Product?.Name ?? "").SetFont(font)));
                table.AddCell(new Cell().Add(new Paragraph(t.Type.ToString()).SetFont(font)));
                table.AddCell(new Cell().Add(new Paragraph(t.Quantity.ToString()).SetFont(font)));
                table.AddCell(new Cell().Add(new Paragraph(t.Price.ToString("N0")).SetFont(font)));
                table.AddCell(new Cell().Add(new Paragraph(t.TotalAmount.ToString("N0")).SetFont(font)));
            }

            document.Add(table);
            document.Close();

            return stream.ToArray();
        }
    }
}
