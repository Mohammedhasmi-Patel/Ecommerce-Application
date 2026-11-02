using Ecommerce.Enum;
using Ecommerce.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Data;

public class AppDbContext : IdentityDbContext<AspNetUsers, IdentityRole<Guid>, Guid>
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Country> Countries => Set<Country>();
    public DbSet<State> States => Set<State>();
    public DbSet<City> Cities => Set<City>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<EmailVerificationOtp> EmailVerificationOtps => Set<EmailVerificationOtp>();
    public DbSet<Cart> Carts => Set<Cart>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<OrderAddress> OrderAddresses => Set<OrderAddress>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Country>(entity =>
        {
            entity.ToTable("Countries");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Iso2).HasMaxLength(2).IsRequired();
            entity.Property(c => c.Iso3).HasMaxLength(3).IsRequired();
            entity.Property(c => c.PhoneCode).HasMaxLength(10);
            entity.Property(c => c.CurrencyCode).HasMaxLength(3);
            entity.Property(c => c.CurrencySymbol).HasMaxLength(10);
            entity.Property(c => c.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<State>(entity =>
        {
            entity.ToTable("States");
            entity.HasKey(s => s.Id);
            entity.Property(s => s.Name).HasMaxLength(100).IsRequired();
            entity.Property(s => s.Iso2).HasMaxLength(10);
            entity.Property(s => s.Iso3166_2).HasMaxLength(10);
            entity.Property(s => s.Type).HasMaxLength(50);
            entity.Property(s => s.Latitude).HasPrecision(10, 8);
            entity.Property(s => s.Longitude).HasPrecision(11, 8);
            entity.Property(s => s.IsActive).HasDefaultValue(true);

            entity.HasOne(s => s.Country)
                  .WithMany(c => c.States)
                  .HasForeignKey(s => s.CountryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<City>(entity =>
        {
            entity.ToTable("Cities");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(100).IsRequired();
            entity.Property(c => c.Latitude).HasPrecision(10, 8);
            entity.Property(c => c.Longitude).HasPrecision(11, 8);
            entity.Property(c => c.IsActive).HasDefaultValue(true);

            entity.HasOne(c => c.State)
                  .WithMany(s => s.Cities)
                  .HasForeignKey(c => c.StateId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.ToTable("Category");
            entity.HasKey(c => c.Id);
            entity.Property(c => c.Name).HasMaxLength(150).IsRequired();
            entity.Property(c => c.Slug).HasMaxLength(180).IsRequired();
            entity.Property(c => c.Logo).HasMaxLength(500);
            entity.Property(c => c.DisplayOrder).HasDefaultValue(0);
            entity.Property(c => c.IsActive).HasDefaultValue(true);

            entity.HasIndex(c => c.Slug).IsUnique();

            entity.HasOne(c => c.ParentCategory)
                  .WithMany(c => c.SubCategories)
                  .HasForeignKey(c => c.ParentCategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Name).HasMaxLength(180).IsRequired();
            entity.Property(p => p.Slug).HasMaxLength(200).IsRequired();
            entity.Property(p => p.Price).HasPrecision(18, 2).IsRequired();
            entity.Property(p => p.DiscountPrice).HasPrecision(18, 2);
            entity.Property(p => p.StockQuantity).HasDefaultValue(0);
            entity.Property(p => p.IsActive).HasDefaultValue(true);
            entity.Property(p => p.IsFeatured).HasDefaultValue(false);
            entity.Property(p => p.Logo).HasMaxLength(500);

            entity.HasIndex(p => p.Slug).IsUnique();

            entity.HasOne(p => p.Category)
                  .WithMany(c => c.Products)
                  .HasForeignKey(p => p.CategoryId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AspNetUsers>(entity =>
        {
            entity.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(u => u.LastName).HasMaxLength(100);
            entity.Property(u => u.ProfileImageUrl).HasMaxLength(500);
            entity.Property(u => u.IsActive).HasDefaultValue(false);
        });

        modelBuilder.Entity<EmailVerificationOtp>(entity =>
        {
            entity.ToTable("EmailVerificationOtps");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Otp).HasMaxLength(6).IsRequired();
            entity.Property(e => e.Attempts).HasDefaultValue(0);
            entity.Property(e => e.IsUsed).HasDefaultValue(false);

            entity.HasOne(e => e.User)
                  .WithMany(u => u.EmailVerificationOtps)
                  .HasForeignKey(e => e.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.ToTable("Carts");
            entity.HasKey(c => c.Id);

            entity.HasIndex(c => c.UserId).IsUnique();

            entity.HasOne(c => c.User)
                  .WithOne(u => u.Cart)
                  .HasForeignKey<Cart>(c => c.UserId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.ToTable("CartItems");
            entity.HasKey(ci => ci.Id);
            entity.Property(ci => ci.Quantity).HasDefaultValue(1);

            entity.HasIndex(ci => new { ci.CartId, ci.ProductId }).IsUnique();

            entity.HasOne(ci => ci.Cart)
                  .WithMany(c => c.CartItems)
                  .HasForeignKey(ci => ci.CartId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ci => ci.Product)
                  .WithMany(p => p.CartItems)
                  .HasForeignKey(ci => ci.ProductId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("Orders");
            entity.HasKey(o => o.Id);
            entity.Property(o => o.OrderNumber).HasMaxLength(50).IsRequired();
            entity.Property(o => o.SubTotal).HasPrecision(18, 2).IsRequired();
            entity.Property(o => o.DiscountAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(o => o.ShippingAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(o => o.TaxAmount).HasPrecision(18, 2).HasDefaultValue(0m);
            entity.Property(o => o.TotalAmount).HasPrecision(18, 2).IsRequired();
            entity.Property(o => o.OrderStatus).HasConversion<int>().HasDefaultValue(OrderStatusEnum.Pending);
            entity.Property(o => o.PaymentStatus).HasConversion<int>().HasDefaultValue(PaymentStatusEnum.Pending);

            entity.HasIndex(o => o.OrderNumber).IsUnique();

            entity.HasOne(o => o.User)
                  .WithMany(u => u.Orders)
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.ToTable("Payments");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.PaymentNumber).HasMaxLength(100).IsRequired();
            entity.Property(p => p.TransactionId).HasMaxLength(200);
            entity.Property(p => p.GatewayTransactionId).HasMaxLength(200);
            entity.Property(p => p.Amount).HasPrecision(18, 2).IsRequired();
            entity.Property(p => p.Currency).HasMaxLength(3).HasDefaultValue("INR");
            entity.Property(p => p.PaymentMethod).HasConversion<int>();
            entity.Property(p => p.PaymentStatus).HasConversion<int>().HasDefaultValue(PaymentStatusEnum.Pending);

            entity.HasIndex(p => p.PaymentNumber).IsUnique();
            entity.HasIndex(p => p.TransactionId);

            entity.HasOne(p => p.Order)
                  .WithMany(o => o.Payments)
                  .HasForeignKey(p => p.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Address>(entity =>
        {
            entity.ToTable("Addresses");
            entity.HasKey(a => a.Id);
            entity.Property(a => a.FullName).HasMaxLength(150).IsRequired();
            entity.Property(a => a.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(a => a.AddressLine1).HasMaxLength(250).IsRequired();
            entity.Property(a => a.AddressLine2).HasMaxLength(250);
            entity.Property(a => a.PostalCode).HasMaxLength(20).IsRequired();
            entity.Property(a => a.Landmark).HasMaxLength(200);
            entity.Property(a => a.AddressType).HasConversion<int>();
            entity.Property(a => a.IsDefault).HasDefaultValue(false);

            entity.HasOne(a => a.User)
                  .WithMany(u => u.Addresses)
                  .HasForeignKey(a => a.UserId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Country)
                  .WithMany(c => c.Addresses)
                  .HasForeignKey(a => a.CountryId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.State)
                  .WithMany(s => s.Addresses)
                  .HasForeignKey(a => a.StateId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(a => a.City)
                  .WithMany(c => c.Addresses)
                  .HasForeignKey(a => a.CityId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderAddress>(entity =>
        {
            entity.ToTable("OrderAddresses");
            entity.HasKey(oa => oa.Id);
            entity.Property(oa => oa.FullName).HasMaxLength(150).IsRequired();
            entity.Property(oa => oa.PhoneNumber).HasMaxLength(20).IsRequired();
            entity.Property(oa => oa.AddressLine1).HasMaxLength(250).IsRequired();
            entity.Property(oa => oa.AddressLine2).HasMaxLength(250);
            entity.Property(oa => oa.Country).HasMaxLength(100).IsRequired();
            entity.Property(oa => oa.State).HasMaxLength(100).IsRequired();
            entity.Property(oa => oa.City).HasMaxLength(100).IsRequired();
            entity.Property(oa => oa.PostalCode).HasMaxLength(20).IsRequired();
            entity.Property(oa => oa.Landmark).HasMaxLength(200);

            entity.HasIndex(oa => oa.OrderId).IsUnique();

            entity.HasOne(oa => oa.Order)
                  .WithOne(o => o.OrderAddress)
                  .HasForeignKey<OrderAddress>(oa => oa.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.ToTable("OrderItems");
            entity.HasKey(oi => oi.Id);
            entity.Property(oi => oi.ProductName).HasMaxLength(180).IsRequired();
            entity.Property(oi => oi.UnitPrice).HasPrecision(18, 2).IsRequired();
            entity.Property(oi => oi.Quantity).IsRequired();
            entity.Property(oi => oi.TotalPrice).HasPrecision(18, 2).IsRequired();

            entity.HasOne(oi => oi.Order)
                  .WithMany(o => o.OrderItems)
                  .HasForeignKey(oi => oi.OrderId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(oi => oi.Product)
                  .WithMany(p => p.OrderItems)
                  .HasForeignKey(oi => oi.ProductId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
