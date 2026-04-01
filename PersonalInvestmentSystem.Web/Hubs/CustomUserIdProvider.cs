using Microsoft.AspNetCore.SignalR;
using System.Security.Claims;

namespace PersonalInvestmentSystem.Web.Hubs
{
    /// <summary>
    /// Custom provider to map SignalR connections to user IDs from Identity
    /// </summary>
    public class CustomUserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            // Get the user ID from NameIdentifier claim (set by ASP.NET Identity)
            return connection.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty;
        }
    }
}
