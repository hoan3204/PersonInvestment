namespace PersonalInvestmentSystem.Web.Domain.Entities
{
    public class WatchList
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public AppUser User { get; set; } = null!;
        public InvestmentProduct Product { get; set; } = null!;
    }
}
