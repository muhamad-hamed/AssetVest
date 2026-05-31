using AssetVest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVest.Infrastructure.Persistence.Configurations;

public class StockProfitGoalConfiguration : IEntityTypeConfiguration<StockProfitGoal>
{
    public void Configure(EntityTypeBuilder<StockProfitGoal> builder)
    {
        builder.ToTable("stock_profit_goals");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Year).IsRequired();
        builder.Property(g => g.TargetProfitPercent).HasPrecision(10, 4).IsRequired();
        builder.Property(g => g.TargetProfitAmountEGP).HasPrecision(18, 4);

        builder.HasIndex(g => new { g.AssetId, g.Year }).IsUnique();
        builder.HasIndex(g => g.UserId);
    }
}
