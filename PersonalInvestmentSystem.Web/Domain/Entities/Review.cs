namespace PersonalInvestmentSystem.Web.Domain.Entities
{
    public class Review
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public int Rating { get; set; }           // 1-5 sao
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public AppUser User { get; set; } = null!;
        public InvestmentProduct Product { get; set; } = null!;
    }
}
