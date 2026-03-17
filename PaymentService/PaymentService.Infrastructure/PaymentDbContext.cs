using Microsoft.EntityFrameworkCore;
using PaymentService.Domain.Entities;
using PaymentService.Infrastructure.Idempotency;

namespace PaymentService.Infrastructure;

public sealed class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Balance> Balances => Set<Balance>();
    public DbSet<ProcessedIntegrationEvent> ProcessedIntegrationEvents => Set<ProcessedIntegrationEvent>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Balance>(builder =>
        {
            builder.HasKey(x => x.CustomerId);

            builder.Property(x => x.CustomerId)
                .IsRequired();

            builder.Property(x => x.Amount)
                .IsRequired();

            builder.Property(x => x.CreatedAt)
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

