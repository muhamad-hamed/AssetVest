using AssetVest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVest.Infrastructure.Persistence.Configurations;

public class FxRateConfiguration : IEntityTypeConfiguration<FxRate>
{
    public void Configure(EntityTypeBuilder<FxRate> builder)
    {
        builder.ToTable("fx_rates");
        builder.HasKey(f => f.Id);

        builder.Property(f => f.FromCurrency).HasMaxLength(3).IsRequired();
        builder.Property(f => f.ToCurrency).HasMaxLength(3).IsRequired().HasDefaultValue("EGP");
        builder.Property(f => f.Rate).HasPrecision(18, 6).IsRequired();
        builder.Property(f => f.Source).HasMaxLength(100).IsRequired();
        builder.Property(f => f.FetchedAt).IsRequired();

        builder.HasIndex(f => new { f.FromCurrency, f.ToCurrency }).IsUnique();
    }
}
