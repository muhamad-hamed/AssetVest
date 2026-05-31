using AssetVest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVest.Infrastructure.Persistence.Configurations;

public class CryptoDetailConfiguration : IEntityTypeConfiguration<CryptoDetail>
{
    public void Configure(EntityTypeBuilder<CryptoDetail> builder)
    {
        builder.ToTable("crypto_details");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Symbol).HasMaxLength(20).IsRequired();
        builder.Property(d => d.NumberOfUnits).HasPrecision(18, 8).IsRequired();
        builder.Property(d => d.PurchasePricePerUnitUSD).HasPrecision(18, 4).IsRequired();
        builder.Property(d => d.CurrentPricePerUnitUSD).HasPrecision(18, 4).IsRequired();
        builder.Property(d => d.UsdToEgpRate).HasPrecision(18, 6).IsRequired();

        builder.HasIndex(d => d.AssetId).IsUnique();
    }
}
