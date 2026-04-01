using PersonalInvestmentSystem.Web.ViewModels.Portfolio;

namespace PersonalInvestmentSystem.Web.Services.Interfaces
{
    public interface IExportService
    {
        byte[] ExportPortfolioToExcel(PortfolioViewModel portfolio, string userFullName);
        byte[] ExportPortfolioToPdf(PortfolioViewModel portfolio, string userFullName);
        byte[] ExportTransactionsToExcel(IEnumerable<PersonalInvestmentSystem.Web.Domain.Entities.Transaction> transactions);
        byte[] ExportTransactionsToPdf(IEnumerable<PersonalInvestmentSystem.Web.Domain.Entities.Transaction> transactions);
    }
}
