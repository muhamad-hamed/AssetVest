using AssetVest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVest.Infrastructure.Persistence.Configurations;

public class CurrencyDetailConfiguration : IEntityTypeConfiguration<CurrencyDetail>
{
    public void Configure(EntityTypeBuilder<CurrencyDetail> builder)
    {
        builder.ToTable("currency_details");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.CurrencyCode).HasMaxLength(3).IsRequired();
        builder.Property(d => d.InitialAmount).HasPrecision(18, 4).IsRequired();
        builder.Property(d => d.CurrentFxRateToEGP).HasPrecision(18, 6).IsRequired();
        builder.Property(d => d.CurrentValueEGP).HasPrecision(18, 4).IsRequired();

        builder.HasIndex(d => d.AssetId).IsUnique();
    }
}
