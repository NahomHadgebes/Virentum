using Microsoft.EntityFrameworkCore;
using Virentum.Api.Infrastructure.Persistence.Entities;

namespace Virentum.Api.Infrastructure.Persistence;

/// <summary>
/// EF Core unit of work for the Virentum domain. Schema is configured via the
/// fluent API in <see cref="OnModelCreating"/> rather than data annotations on
/// the entities, keeping persistence concerns out of the domain types.
/// </summary>
public sealed class VirentumDbContext : DbContext
{
    public VirentumDbContext(DbContextOptions<VirentumDbContext> options)
        : base(options)
    {
    }

    public DbSet<InspectionRecord> Inspections => Set<InspectionRecord>();

    public DbSet<UserAccount> Users => Set<UserAccount>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<InspectionRecord>(entity =>
        {
            entity.ToTable("inspections");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StoreId).IsRequired().HasMaxLength(128);
            // Persist enums as their readable string names for durable, query-friendly data.
            entity.Property(e => e.FruitType).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(e => e.CommercialStatus).HasConversion<string>().HasMaxLength(32).IsRequired();
            entity.Property(e => e.Recommendation).IsRequired().HasMaxLength(512);
            entity.Property(e => e.ScannedAt).IsRequired();
            entity.HasIndex(e => new { e.StoreId, e.ScannedAt });
        });

        modelBuilder.Entity<UserAccount>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StoreId).IsRequired().HasMaxLength(128);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(256);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(128);
            entity.Property(e => e.Station).IsRequired().HasMaxLength(64);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(32);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.StoreId).IsUnique();
        });
    }
}
