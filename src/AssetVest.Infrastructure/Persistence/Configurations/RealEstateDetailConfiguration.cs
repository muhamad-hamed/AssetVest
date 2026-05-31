using AssetVest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVest.Infrastructure.Persistence.Configurations;

public class RealEstateDetailConfiguration : IEntityTypeConfiguration<RealEstateDetail>
{
    public void Configure(EntityTypeBuilder<RealEstateDetail> builder)
    {
        builder.ToTable("real_estate_details");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Description).HasMaxLength(500).IsRequired();
        builder.Property(d => d.Location).HasMaxLength(300);
        builder.Property(d => d.AreaSqm).HasPrecision(10, 2);
        builder.Property(d => d.PurchaseValueEGP).HasPrecision(18, 4).IsRequired();
        builder.Property(d => d.CurrentEstimatedValueEGP).HasPrecision(18, 4).IsRequired();

        builder.HasIndex(d => d.AssetId).IsUnique();
    }
}
