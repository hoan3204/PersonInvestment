using PersonalInvestmentSystem.Web.Domain.Enums;

namespace PersonalInvestmentSystem.Web.Domain.Entities
{
    public class Order
    {
        public int Id { get; set; }
        public string UsetId { get; set; } = string.Empty;
        public Decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? CompletedDtae { get; set; }

        public AppUser User { get; set; } = null!;
        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    }
}
