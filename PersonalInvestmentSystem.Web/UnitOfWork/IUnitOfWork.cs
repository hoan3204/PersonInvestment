using DocumentFormat.OpenXml.Drawing.Diagrams;
using PersonalInvestmentSystem.Web.Domain.Entities;
using PersonalInvestmentSystem.Web.Repositories.Interfaces;

namespace PersonalInvestmentSystem.Web.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<PersonalInvestmentSystem.Web.Domain.Entities.Category> Categories { get; }
        IGenericRepository<Publisher> Publishers { get; }
        IGenericRepository<InvestmentProduct> InvestmentProducts { get; }
        IGenericRepository<Wallet> Wallets { get; }
        IGenericRepository<Transaction> Transactions { get; }
        IGenericRepository<Order> Orders { get; }
        IGenericRepository<OrderDetail> OrderDetails { get; }
        IGenericRepository<WatchList> WatchLists { get; }
        IGenericRepository<Notification> Notifications { get; }
        IGenericRepository<News> News { get; }
        IGenericRepository<Coupon> Coupons { get; }
        IGenericRepository<Banner> Banners { get; }
        IGenericRepository<Review> Reviews { get; }
        IGenericRepository<ChatMessage> ChatMessages { get; }
        IGenericRepository<Portfolio> Portfolios { get; }
        IGenericRepository<AppUser> Users { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
