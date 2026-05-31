using AssetVest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVest.Infrastructure.Persistence.Configurations;

public class GoldDetailConfiguration : IEntityTypeConfiguration<GoldDetail>
{
    public void Configure(EntityTypeBuilder<GoldDetail> builder)
    {
        builder.ToTable("gold_details");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.WeightGrams).HasPrecision(10, 3).IsRequired();
        builder.Property(d => d.Karat).IsRequired();
        builder.Property(d => d.PurchasePricePerGramEGP).HasPrecision(18, 4).IsRequired();
        builder.Property(d => d.CurrentPricePerGramEGP).HasPrecision(18, 4).IsRequired();

        builder.HasIndex(d => d.AssetId).IsUnique();
    }
}
