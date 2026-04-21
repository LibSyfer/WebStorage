using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using WebStorage.Domain.Entities;
using WebStorage.Infrastructure.Auth;
using WebStorage.Infrastructure.Identity;

namespace WebStorage.Infrastructure.Data;

public sealed class AppDbContext : IdentityDbContext<ApplicationUser>
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshSession> RefreshSessions => Set<RefreshSession>();
    public DbSet<FileEntry> FileEntries => Set<FileEntry>();
    public DbSet<UserStorage> UserStorages => Set<UserStorage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<FileEntry>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).IsRequired();
            b.Property(x => x.FileName).IsRequired();
            b.Property(x => x.StorageKey).IsRequired();
            b.Property(x => x.Size).IsRequired();
            b.HasIndex(x => x.UserId);
        });

        builder.Entity<UserStorage>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.UserId).IsRequired();
            b.HasIndex(x => x.UserId).IsUnique();
        });
    }
}
