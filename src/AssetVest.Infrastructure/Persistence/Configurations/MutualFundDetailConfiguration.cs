using AssetVest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVest.Infrastructure.Persistence.Configurations;

public class MutualFundDetailConfiguration : IEntityTypeConfiguration<MutualFundDetail>
{
    public void Configure(EntityTypeBuilder<MutualFundDetail> builder)
    {
        builder.ToTable("mutual_fund_details");
        builder.HasKey(d => d.Id);

        builder.Property(d => d.FundName).HasMaxLength(200).IsRequired();
        builder.Property(d => d.ManagementCompany).HasMaxLength(200);
        builder.Property(d => d.FundType).IsRequired();
        builder.Property(d => d.NumberOfUnits).HasPrecision(18, 6).IsRequired();
        builder.Property(d => d.PurchaseNAVPerUnit).HasPrecision(18, 4).IsRequired();
        builder.Property(d => d.CurrentNAVPerUnit).HasPrecision(18, 4).IsRequired();

        builder.HasIndex(d => d.AssetId).IsUnique();
    }
}
