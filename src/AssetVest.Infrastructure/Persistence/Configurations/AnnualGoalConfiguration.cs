using AssetVest.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AssetVest.Infrastructure.Persistence.Configurations;

public class AnnualGoalConfiguration : IEntityTypeConfiguration<AnnualGoal>
{
    public void Configure(EntityTypeBuilder<AnnualGoal> builder)
    {
        builder.ToTable("annual_goals");
        builder.HasKey(g => g.Id);

        builder.Property(g => g.Year).IsRequired();
        builder.Property(g => g.TargetTotalPortfolioValueEGP).HasPrecision(18, 4).IsRequired();
        builder.Property(g => g.TargetProfitPercent).HasPrecision(10, 4);
        builder.Property(g => g.Notes).HasMaxLength(1000);

        builder.HasIndex(g => new { g.UserId, g.Year }).IsUnique();

        builder.HasMany(g => g.AllocationGoals)
            .WithOne(a => a.AnnualGoal)
            .HasForeignKey(a => a.AnnualGoalId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
