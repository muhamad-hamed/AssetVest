using System.Linq.Expressions;
using System.Text.Json;
using AssetVest.Application.Ports;
using AssetVest.Domain.Common;
using AssetVest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace AssetVest.Infrastructure.Persistence;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserService currentUserService)
    : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<AssetValueHistory> AssetValueHistories => Set<AssetValueHistory>();
    public DbSet<StockDetail> StockDetails => Set<StockDetail>();
    public DbSet<CurrencyDetail> CurrencyDetails => Set<CurrencyDetail>();
    public DbSet<GoldDetail> GoldDetails => Set<GoldDetail>();
    public DbSet<RealEstateDetail> RealEstateDetails => Set<RealEstateDetail>();
    public DbSet<MutualFundDetail> MutualFundDetails => Set<MutualFundDetail>();
    public DbSet<CryptoDetail> CryptoDetails => Set<CryptoDetail>();
    public DbSet<BondsDetail> BondsDetails => Set<BondsDetail>();
    public DbSet<FxRate> FxRates => Set<FxRate>();
    public DbSet<FxRateHistory> FxRateHistories => Set<FxRateHistory>();
    public DbSet<AnnualGoal> AnnualGoals => Set<AnnualGoal>();
    public DbSet<AssetTypeAllocationGoal> AssetTypeAllocationGoals => Set<AssetTypeAllocationGoal>();
    public DbSet<StockProfitGoal> StockProfitGoals => Set<StockProfitGoal>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        ApplySoftDeleteFilters(modelBuilder);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var auditEntries = CaptureAuditEntries();
        ApplyAuditFields();
        var result = await base.SaveChangesAsync(cancellationToken);
        await SaveAuditLogsAsync(auditEntries, cancellationToken);
        return result;
    }

    private void ApplyAuditFields()
    {
        var now = DateTime.UtcNow;
        var userId = currentUserService.UserId;

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = userId;
                    break;

                case EntityState.Modified:
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = userId;
                    break;

                case EntityState.Deleted:
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedAt = now;
                    entry.Entity.DeletedBy = userId;
                    break;
            }
        }
    }

    private List<AuditEntry> CaptureAuditEntries()
    {
        ChangeTracker.DetectChanges();
        var entries = new List<AuditEntry>();

        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            if (entry.State is EntityState.Unchanged or EntityState.Detached)
                continue;

            var auditEntry = new AuditEntry
            {
                EntityName = entry.Entity.GetType().Name,
                EntityId = GetPrimaryKeyValue(entry),
                Action = entry.State.ToString(),
                OldValues = entry.State == EntityState.Modified
                    ? JsonSerializer.Serialize(entry.OriginalValues.ToObject())
                    : null,
                NewValues = entry.State != EntityState.Deleted
                    ? JsonSerializer.Serialize(entry.CurrentValues.ToObject())
                    : null
            };

            entries.Add(auditEntry);
        }

        return entries;
    }

    private async Task SaveAuditLogsAsync(List<AuditEntry> entries, CancellationToken cancellationToken)
    {
        if (entries.Count == 0) return;

        var logs = entries.Select(e => new AuditLog
        {
            Id = Guid.NewGuid(),
            UserId = currentUserService.UserId,
            EntityName = e.EntityName,
            EntityId = e.EntityId,
            Action = e.Action,
            OldValues = e.OldValues,
            NewValues = e.NewValues,
            IpAddress = currentUserService.IpAddress,
            Timestamp = DateTime.UtcNow
        });

        AuditLogs.AddRange(logs);
        await base.SaveChangesAsync(cancellationToken);
    }

    private static string GetPrimaryKeyValue(EntityEntry entry)
    {
        var keyProperties = entry.Metadata.FindPrimaryKey()?.Properties;
        if (keyProperties == null) return string.Empty;
        var values = keyProperties.Select(p => entry.Property(p.Name).CurrentValue?.ToString());
        return string.Join(",", values);
    }

    private static void ApplySoftDeleteFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(AuditableEntity).IsAssignableFrom(entityType.ClrType)) continue;

            var param = Expression.Parameter(entityType.ClrType, "e");
            var prop = Expression.Property(param, nameof(AuditableEntity.IsDeleted));
            var filter = Expression.Lambda(Expression.Equal(prop, Expression.Constant(false)), param);
            modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
        }
    }

    private sealed class AuditEntry
    {
        public string EntityName { get; set; } = string.Empty;
        public string EntityId { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public string? OldValues { get; set; }
        public string? NewValues { get; set; }
    }
}
