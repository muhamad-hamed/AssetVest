using AssetVest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVest.Infrastructure.Persistence.Configurations;

public class FxRateHistoryConfiguration : IEntityTypeConfiguration<FxRateHistory>
{
    public void Configure(EntityTypeBuilder<FxRateHistory> builder)
    {
        builder.ToTable("fx_rate_history");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FromCurrency).HasMaxLength(3).IsRequired();
        builder.Property(f => f.ToCurrency).HasMaxLength(3).IsRequired().HasDefaultValue("EGP");
        builder.Property(f => f.Rate).HasPrecision(18, 6).IsRequired();
        builder.Property(f => f.Source).HasMaxLength(100).IsRequired();
        builder.Property(f => f.RecordedAt).IsRequired();

        builder.HasIndex(f => new { f.FromCurrency, f.RecordedAt });
    }
}
