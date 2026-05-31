using AssetVest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVest.Infrastructure.Persistence.Configurations;

public class StockDetailConfiguration : IEntityTypeConfiguration<StockDetail>
{
    public void Configure(EntityTypeBuilder<StockDetail> builder)
    {
        builder.ToTable("stock_details");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.StockSymbol).HasMaxLength(20).IsRequired();
        builder.Property(d => d.Exchange).HasMaxLength(50);
        builder.Property(d => d.NumberOfUnits).HasPrecision(18, 6).IsRequired();
        builder.Property(d => d.PurchasePricePerUnitEGP).HasPrecision(18, 4).IsRequired();
        builder.Property(d => d.CurrentPricePerUnitEGP).HasPrecision(18, 4).IsRequired();

        builder.HasIndex(d => d.AssetId).IsUnique();
    }
}
