using AuthService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Infrastructure;

public sealed class AuthDbContext : DbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(builder =>
        {
            builder.HasKey(u => u.Id);
            builder.Property(u => u.Email)
                .IsRequired()
                .HasMaxLength(256);
            builder.Property(u => u.UserName)
                .IsRequired()
                .HasMaxLength(256);
            builder.Property(u => u.PasswordHash)
                .IsRequired();

            builder.HasMany(u => u.RefreshTokens)
                .WithOne(rt => rt.User)
                .HasForeignKey(rt => rt.UserId);
        });

        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.HasKey(rt => rt.Id);
            builder.Property(rt => rt.Token)
                .IsRequired()
                .HasMaxLength(512);
        });
    }
}
