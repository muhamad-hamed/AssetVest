using AssetVest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVest.Infrastructure.Persistence.Configurations;

public class AssetValueHistoryConfiguration : IEntityTypeConfiguration<AssetValueHistory>
{
    public void Configure(EntityTypeBuilder<AssetValueHistory> builder)
    {
        builder.ToTable("asset_value_history");
        builder.HasKey(h => h.Id);

        builder.Property(h => h.ValueEGP).HasPrecision(18, 4).IsRequired();
        builder.Property(h => h.ProfitEGP).HasPrecision(18, 4).IsRequired();
        builder.Property(h => h.ProfitPercent).HasPrecision(10, 4).IsRequired();
        builder.Property(h => h.RecordedAt).IsRequired();
        builder.Property(h => h.Source).IsRequired();
        builder.Property(h => h.Notes).HasMaxLength(500);

        builder.HasIndex(h => h.AssetId);
        builder.HasIndex(h => h.RecordedAt);
    }
}
