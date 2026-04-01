namespace PersonalInvestmentSystem.Web.ViewModels.Admin
{
    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalProducts { get; set; }
        public decimal TotalPortfolioValue { get; set; }
        public int TotalTransactions { get; set; }
        public decimal TotalTransactionAmount { get; set; }

        public List<RecentTransactionViewModel> RecentTransactions { get; set; } = new();
        public List<TopProductViewModel> TopProducts { get; set; } = new();
    }

    public class RecentTransactionViewModel
    {
        public string UserFullName { get; set; } = string.Empty;
        public string ProductName { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedDate { get; set; }
    }

    public class TopProductViewModel
    {
        public string ProductName { get; set; } = string.Empty;
        public int TransactionCount { get; set; }
        public decimal TotalVolume { get; set; }

    }



}
