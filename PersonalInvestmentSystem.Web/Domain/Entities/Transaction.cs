using PersonalInvestmentSystem.Web.Domain.Enums;

namespace PersonalInvestmentSystem.Web.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int ProductId { get; set; }
        public TransactionType Type { get; set; }
        public int Quantity { get; set; }
        public Decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public TransactionStatus Status { get; set; } = TransactionStatus.Completed;
        public PaymentMethod PaymentMethod { get; set; } 
        public string? PaymentReference { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public AppUser User { get; set; } = null!;
        public InvestmentProduct Product { get; set; } = null!;

    }
}
