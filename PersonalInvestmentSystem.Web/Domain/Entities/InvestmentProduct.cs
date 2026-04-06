using PersonalInvestmentSystem.Web.Domain.Enums;

namespace PersonalInvestmentSystem.Web.Domain.Entities
{
    public class InvestmentProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Code { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public InvestmentType Type { get; set; }
        public int CategoryId { get; set; }
        public int PublisherId { get; set; }
        public Decimal CurrentPrice { get; set; }
        public Decimal PreviousPrice { get; set; }
        public Decimal ChangePercent { get; set; }
        public RiskLevel RiskLevel { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsFeatured { get; set; } = false;
        public bool IsActive { get; set; } = true;
        public DateTime? DeleteDate { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;


        public Category Category { get; set; } = null!;
        public Publisher Publisher { get; set; } = null!;
        public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();

    }
}
