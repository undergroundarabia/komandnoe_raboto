using Microsoft.EntityFrameworkCore;
using ShoeStore.Models;

namespace ShoeStore.Data;

public class ShoeStoreContext : DbContext
{
    private const string ConnectionString =
        @"Server=(localdb)\MSSQLLocalDB;Database=ShoeStoreDb;Trusted_Connection=True;TrustServerCertificate=True";

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Manufacturer> Manufacturers => Set<Manufacturer>();
    public DbSet<Size> Sizes => Set<Size>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductSize> ProductSizes => Set<ProductSize>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(ConnectionString);
    }

    // База создаётся скриптом Database/ShoeStoreDb.sql, здесь только описание соответствия
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("Role");
            e.Property(x => x.Name).HasMaxLength(50);
            e.HasIndex(x => x.Name).IsUnique();
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("User");
            e.Property(x => x.Login).HasMaxLength(50);
            e.Property(x => x.LastName).HasMaxLength(50);
            e.Property(x => x.FirstName).HasMaxLength(50);
            e.Property(x => x.Patronymic).HasMaxLength(50);
            e.HasIndex(x => x.Login).IsUnique();
            e.Ignore(x => x.FullName);
            e.HasOne(x => x.Role).WithMany(r => r.Users)
             .HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(e =>
        {
            e.ToTable("Category");
            e.Property(x => x.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Manufacturer>(e =>
        {
            e.ToTable("Manufacturer");
            e.Property(x => x.Name).HasMaxLength(100);
        });

        modelBuilder.Entity<Size>(e =>
        {
            e.ToTable("Size");
            e.Property(x => x.Value).HasPrecision(4, 1);
        });

        modelBuilder.Entity<Product>(e =>
        {
            e.ToTable("Product");
            e.Property(x => x.Name).HasMaxLength(150);
            e.Property(x => x.Composition).HasMaxLength(300);
            e.Property(x => x.ImagePath).HasMaxLength(255);
            e.Property(x => x.Price).HasPrecision(10, 2);
            e.Ignore(x => x.TotalQuantity);
            e.HasOne(x => x.Category).WithMany(c => c.Products)
             .HasForeignKey(x => x.CategoryId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Manufacturer).WithMany(m => m.Products)
             .HasForeignKey(x => x.ManufacturerId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<ProductSize>(e =>
        {
            e.ToTable("ProductSize");
            e.HasIndex(x => new { x.ProductId, x.SizeId }).IsUnique();
            e.HasOne(x => x.Product).WithMany(p => p.ProductSizes)
             .HasForeignKey(x => x.ProductId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.Size).WithMany(s => s.ProductSizes)
             .HasForeignKey(x => x.SizeId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Order>(e =>
        {
            e.ToTable("Order");
            e.Ignore(x => x.TotalSum);
            e.HasOne(x => x.User).WithMany(u => u.Orders)
             .HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<OrderItem>(e =>
        {
            e.ToTable("OrderItem");
            e.Property(x => x.UnitPrice).HasPrecision(10, 2);
            e.HasIndex(x => new { x.OrderId, x.ProductSizeId }).IsUnique();
            e.HasOne(x => x.Order).WithMany(o => o.OrderItems)
             .HasForeignKey(x => x.OrderId).OnDelete(DeleteBehavior.Cascade);
            e.HasOne(x => x.ProductSize).WithMany(ps => ps.OrderItems)
             .HasForeignKey(x => x.ProductSizeId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
