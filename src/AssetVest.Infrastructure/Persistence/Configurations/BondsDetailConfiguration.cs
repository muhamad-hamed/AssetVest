using AssetVest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVest.Infrastructure.Persistence.Configurations;

public class BondsDetailConfiguration : IEntityTypeConfiguration<BondsDetail>
{
    public void Configure(EntityTypeBuilder<BondsDetail> builder)
    {
        builder.ToTable("bonds_details");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Issuer).HasMaxLength(200).IsRequired();
        builder.Property(d => d.FaceValueEGP).HasPrecision(18, 4).IsRequired();
        builder.Property(d => d.CouponRatePercent).HasPrecision(6, 4).IsRequired();
        builder.Property(d => d.MaturityDate).IsRequired();
        builder.Property(d => d.PurchasePriceEGP).HasPrecision(18, 4).IsRequired();

        builder.HasIndex(d => d.AssetId).IsUnique();
    }
}
