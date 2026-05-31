using AssetVest.Domain.Common;

namespace AssetVest.Domain.Entities;

public class AnnualGoal : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Year { get; set; }
    public decimal TargetTotalPortfolioValueEGP { get; set; }
    public decimal? TargetProfitPercent { get; set; }
    public string? Notes { get; set; }

    public User User { get; set; } = null!;
    public ICollection<AssetTypeAllocationGoal> AllocationGoals { get; set; } = [];
}
