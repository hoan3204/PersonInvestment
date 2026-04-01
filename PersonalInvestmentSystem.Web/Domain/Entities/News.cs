namespace PersonalInvestmentSystem.Web.Domain.Entities
{
    public class News
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Author { get; set; } = "Admin";
        public bool IsActive { get; set; } = true;
        public DateTime PublishedDate { get; set; } = DateTime.UtcNow;
    }
}
