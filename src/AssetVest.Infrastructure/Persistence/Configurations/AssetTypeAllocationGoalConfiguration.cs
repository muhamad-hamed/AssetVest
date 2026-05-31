using AssetVest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVest.Infrastructure.Persistence.Configurations;

public class AssetTypeAllocationGoalConfiguration : IEntityTypeConfiguration<AssetTypeAllocationGoal>
{
    public void Configure(EntityTypeBuilder<AssetTypeAllocationGoal> builder)
    {
        builder.ToTable("asset_type_allocation_goals");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.AssetType).IsRequired();
        builder.Property(g => g.TargetAllocationPercent).HasPrecision(6, 4).IsRequired();

        builder.HasIndex(g => new { g.AnnualGoalId, g.AssetType }).IsUnique();
    }
}
