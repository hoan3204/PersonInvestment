namespace PersonalInvestmentSystem.Web.Domain.Entities
{
    public class Wallet
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal Balance { get; set; } = 0;
        public string Currency { get; set; } = "VND";
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; }
        public DateTime DeletedDate { get; set; }
        public AppUser User { get; set; } = null!;

    }
}
