using PersonalInvestmentSystem.Web.ViewModels.Admin;

namespace PersonalInvestmentSystem.Web.Services.Interfaces
{
    public interface IAdminService
    {
        Task<AdminDashboardViewModel> GetDashboardDataAsync();
    }
}
