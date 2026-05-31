using AssetVest.Domain.Enums;

namespace AssetVest.Domain.Entities;

public class AssetTypeAllocationGoal
{
    public Guid Id { get; set; }
    public Guid AnnualGoalId { get; set; }
    public AssetType AssetType { get; set; }
    public decimal TargetAllocationPercent { get; set; }

    public AnnualGoal AnnualGoal { get; set; } = null!;
}
