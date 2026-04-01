using PersonalInvestmentSystem.Web.Data;
using PersonalInvestmentSystem.Web.Domain.Entities;
using PersonalInvestmentSystem.Web.Repositories.Interfaces;
using PersonalInvestmentSystem.Web.Repositories.Implementations;

namespace PersonalInvestmentSystem.Web.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context;
        private bool _disposed = false;
        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
            
        }
        //khai bao repository
        private IGenericRepository<Category>? _categories;
        private IGenericRepository<Publisher>? _publishers;
        private IGenericRepository<InvestmentProduct>? _investmentProducts;
        private IGenericRepository<Wallet>? _wallets;
        private IGenericRepository<Transaction>? _transactions;
        private IGenericRepository<Order>? _orders;
        private IGenericRepository<OrderDetail>? _orderDetails;
        private IGenericRepository<WatchList>? _watchList;
        private IGenericRepository<Notification>? _notifications;
        private IGenericRepository<News>? _news;
        private IGenericRepository<Coupon>? _coupons;
        private IGenericRepository<Banner>? _banners;
        private IGenericRepository<Review>? _reviews;
        private IGenericRepository<ChatMessage>? _chatMessages;
        private IGenericRepository<Portfolio>? _portfolios;
        private IGenericRepository<AppUser>? _users;

        public IGenericRepository<Portfolio> Portfolios
            => _portfolios ??= new GenericRepository<Portfolio>(_context);
        public IGenericRepository<Category> Categories
            => _categories ??= new GenericRepository<Category>(_context);

        public IGenericRepository<Publisher> Publishers
            => _publishers ??= new GenericRepository<Publisher>(_context);

        public IGenericRepository<InvestmentProduct> InvestmentProducts
            => _investmentProducts ??= new GenericRepository<InvestmentProduct>(_context);

        public IGenericRepository<Wallet> Wallets
            => _wallets ??= new GenericRepository<Wallet>(_context);

        public IGenericRepository<Transaction> Transactions
            => _transactions ??= new GenericRepository<Transaction>(_context);

        public IGenericRepository<Order> Orders
            => _orders ??= new GenericRepository<Order>(_context);

        public IGenericRepository<OrderDetail> OrderDetails
            => _orderDetails ??= new GenericRepository<OrderDetail>(_context);

        public IGenericRepository<WatchList> WatchLists
            => _watchList ??= new GenericRepository<WatchList>(_context);

        public IGenericRepository<Notification> Notifications
            => _notifications ??= new GenericRepository<Notification>(_context);

        public IGenericRepository<News> News
            => _news ??= new GenericRepository<News>(_context);

        public IGenericRepository<Coupon> Coupons
            => _coupons ??= new GenericRepository<Coupon>(_context);

        public IGenericRepository<Banner> Banners
            => _banners ??= new GenericRepository<Banner>(_context);

        public IGenericRepository<Review> Reviews
            => _reviews ??= new GenericRepository<Review>(_context);

        public IGenericRepository<ChatMessage> ChatMessages
            => _chatMessages ??= new GenericRepository<ChatMessage>(_context);

        public IGenericRepository<AppUser> Users
            => _users ??= new GenericRepository<AppUser>(_context);

        public async Task<int> SaveChangesAsync() => await _context.SaveChangesAsync();

        public async Task BeginTransactionAsync() => await _context.Database.BeginTransactionAsync();
        public async Task CommitTransactionAsync() => await _context.Database.CommitTransactionAsync();
        public async Task RollbackTransactionAsync() => await _context.Database.RollbackTransactionAsync();

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed && disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
