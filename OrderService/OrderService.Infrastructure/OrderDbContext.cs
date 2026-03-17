using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OrderService.Domain.Entities;
using OrderService.Infrastructure.ReadModels;

namespace OrderService.Infrastructure;

public sealed class OrderDbContext : DbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options)
        : base(options)
    {
    }

    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<CatalogProductSnapshot> CatalogProducts => Set<CatalogProductSnapshot>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Order>(ConfigureOrder);
        modelBuilder.Entity<OrderItem>(ConfigureOrderItem);
        modelBuilder.Entity<CatalogProductSnapshot>(ConfigureCatalogProductSnapshot);
    }

    private static void ConfigureOrder(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired();

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder
            .HasMany(o => o.Items)
            .WithOne()
            .HasForeignKey(i => i.OrderId)
            .IsRequired();

        builder.Navigation(o => o.Items)
            .UsePropertyAccessMode(PropertyAccessMode.Field);

        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.CreatedAt);
    }

    private static void ConfigureOrderItem(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.OrderId)
            .IsRequired();

        builder.Property(i => i.ProductId)
            .IsRequired();

        builder.Property(i => i.ProductName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(i => i.UnitPrice)
            .IsRequired();

        builder.Property(i => i.Quantity)
            .IsRequired();

        builder.HasIndex(i => i.OrderId);
    }

    private static void ConfigureCatalogProductSnapshot(EntityTypeBuilder<CatalogProductSnapshot> builder)
    {
        builder.HasKey(x => x.ProductId);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Price)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.Stock)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired();
    }
}
