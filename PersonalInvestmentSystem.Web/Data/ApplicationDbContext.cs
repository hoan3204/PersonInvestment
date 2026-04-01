using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PersonalInvestmentSystem.Web.Domain.Entities;
using System.Reflection.Emit;

namespace PersonalInvestmentSystem.Web.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            :base(options) {}

        public DbSet<Category> Categories { get; set; }
        public DbSet<Publisher> Publishers { get; set; }
        public DbSet<InvestmentProduct> InvestmentProducts { get; set; }
        public DbSet<Wallet> Wallets { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<WatchList> WatchLists { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<Coupon> Coupons { get; set; }
        public DbSet<Banner> Banners { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<Portfolio> Portfolios { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            // Configure decimal precision and scale
            modelBuilder.Entity<Coupon>()
                .Property(c => c.DiscountPercent)
                .HasPrecision(5, 2);

            modelBuilder.Entity<Coupon>()
                .Property(c => c.MaxDiscountAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvestmentProduct>()
                .Property(p => p.ChangePercent)
                .HasPrecision(8, 4);

            modelBuilder.Entity<InvestmentProduct>()
                .Property(p => p.CurrentPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<InvestmentProduct>()
                .Property(p => p.PreviousPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderDetail>()
                .Property(od => od.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Transaction>()
                .Property(t => t.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Wallet>()
                .Property(w => w.Balance)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Portfolio>()
                .Property(p => p.AverageBuyPrice)
                .HasPrecision(18, 2);

            // Index tối ưu
            modelBuilder.Entity<InvestmentProduct>()
            .HasIndex(p => p.Code);

            modelBuilder.Entity<Transaction>()
                .HasIndex(t => new { t.UserId, t.CreatedDate });

            modelBuilder.Entity<Wallet>()
                .HasIndex(w => w.UserId);

            modelBuilder.Entity<WatchList>()
                .HasIndex(w => new { w.UserId, w.ProductId })
                .IsUnique();

            // Relationship configurations nếu cần
            modelBuilder.Entity<Transaction>()
                .Property(t => t.Status)
                .HasConversion<string>();

            modelBuilder.Entity<Transaction>()
                .Property(t => t.Type)
                .HasConversion<string>();
            modelBuilder.Entity<Portfolio>()
                .HasIndex(p => new { p.UserId, p.ProductId })
                .IsUnique();
        }
    }
}
