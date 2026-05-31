using AssetVest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVest.Infrastructure.Persistence.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("assets");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Name).HasMaxLength(200).IsRequired();
        builder.Property(a => a.AssetType).IsRequired();
        builder.Property(a => a.BaseCurrency).HasMaxLength(3).IsRequired().HasDefaultValue("EGP");
        builder.Property(a => a.Notes).HasMaxLength(1000);

        builder.Property(a => a.InitialValueEGP).HasPrecision(18, 4).IsRequired();
        builder.Property(a => a.CurrentValueEGP).HasPrecision(18, 4).IsRequired();
        builder.Property(a => a.ProfitEGP).HasPrecision(18, 4);
        builder.Property(a => a.ProfitPercent).HasPrecision(10, 4);

        builder.HasIndex(a => a.UserId);
        builder.HasIndex(a => a.AssetType);

        builder.HasMany(a => a.ValueHistory)
            .WithOne(h => h.Asset)
            .HasForeignKey(h => h.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.StockDetail)
            .WithOne(d => d.Asset)
            .HasForeignKey<StockDetail>(d => d.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.CurrencyDetail)
            .WithOne(d => d.Asset)
            .HasForeignKey<CurrencyDetail>(d => d.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.GoldDetail)
            .WithOne(d => d.Asset)
            .HasForeignKey<GoldDetail>(d => d.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.RealEstateDetail)
            .WithOne(d => d.Asset)
            .HasForeignKey<RealEstateDetail>(d => d.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.MutualFundDetail)
            .WithOne(d => d.Asset)
            .HasForeignKey<MutualFundDetail>(d => d.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.CryptoDetail)
            .WithOne(d => d.Asset)
            .HasForeignKey<CryptoDetail>(d => d.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.BondsDetail)
            .WithOne(d => d.Asset)
            .HasForeignKey<BondsDetail>(d => d.AssetId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.StockProfitGoal)
            .WithOne(g => g.Asset)
            .HasForeignKey<StockProfitGoal>(g => g.AssetId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
