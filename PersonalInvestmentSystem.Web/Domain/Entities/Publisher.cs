namespace PersonalInvestmentSystem.Web.Domain.Entities
{
    public class Publisher
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public ICollection<InvestmentProduct> Products { get; set; } = new List<InvestmentProduct>();
    }
}
