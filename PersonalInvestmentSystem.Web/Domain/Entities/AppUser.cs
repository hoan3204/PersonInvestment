using Microsoft.AspNetCore.Identity;

namespace PersonalInvestmentSystem.Web.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        public string FullName { get; set; } = string.Empty;
        public string? Avatar { get; set; } 
        public DateTime DateOfBirth { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;


    }
}
