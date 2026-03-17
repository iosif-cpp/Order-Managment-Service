using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Idempotency;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Infrastructure;

public sealed class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options)
        : base(options)
    {
    }

    public DbSet<Product> Products => Set<Product>();

    public DbSet<Category> Categories => Set<Category>();

    public DbSet<ProcessedIntegrationEvent> ProcessedIntegrationEvents => Set<ProcessedIntegrationEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Product>(builder =>
        {
            builder.HasKey(p => p.Id);

            builder.Property(p => p.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(p => p.Description)
                .HasMaxLength(2000);

            builder.Property(p => p.Price)
                .IsRequired();

            builder.Property(p => p.Sku)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(p => p.IsActive)
                .IsRequired();

            builder.Property(p => p.CreatedAt)
                .IsRequired();

            builder.Property(p => p.Stock)
                .IsRequired();

            builder.HasIndex(p => p.Sku)
                .IsUnique();

            builder
                .HasOne(p => p.Category)
                .WithMany()
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Category>(builder =>
        {
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Name)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(c => c.Description)
                .HasMaxLength(2000);

            builder.Property(c => c.IsActive)
                .IsRequired();

            builder.Property(c => c.CreatedAt)
                .IsRequired();
        });

        modelBuilder.Entity<ProcessedIntegrationEvent>(builder =>
        {
            builder.HasKey(x => x.EventId);

            builder.Property(x => x.EventId)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.ProcessedAt)
                .IsRequired();
        });
    }
}

