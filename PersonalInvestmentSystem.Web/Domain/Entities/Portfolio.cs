namespace PersonalInvestmentSystem.Web.Domain.Entities
{
    public class Portfolio
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 0;
        public decimal AverageBuyPrice { get; set; }
        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        
        public AppUser User { get; set; } = null!;
        public InvestmentProduct Product { get; set; } = null!;

    }
}
