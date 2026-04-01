namespace PersonalInvestmentSystem.Web.Domain.Entities
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsFromAdmin { get; set; } = false;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;

        public AppUser User { get; set; } = null!;
    }
}
