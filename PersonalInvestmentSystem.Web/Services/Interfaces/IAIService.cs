namespace PersonalInvestmentSystem.Web.Services.Interfaces
{
    public interface IAIService
    {
        Task<string> AnalyzePortfolioAsync(string userId);
        Task<string> AnalyzeProductAsync(int productId, string userQuestion = "");
        Task<string> GetInvestmentAdviceAsync(string userId, string question);
    }
}
