namespace PersonalInvestmentSystem.Web.ViewModels.Portfolio
{
    public class PortfolioViewModel
    {
        public decimal TotalValue { get; set; } = 0;
        public decimal TotalCost { get; set; } = 0;
        public decimal TotalProfit { get; set; } = 0;
        public decimal ROIPercent { get; set; } = 0;

        public List<PortfolioItemViewModel> Items { get; set; } = new List<PortfolioItemViewModel>();
    }

    public class PortfolioItemViewModel
    {
       public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string ProductCode { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal AverageBuyPrice { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal Profit { get; set; }
        public decimal ProfitPercent { get; set; }
        public string Type { get; set; } = string.Empty;
    }

}
