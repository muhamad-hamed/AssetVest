using AssetVest.Domain.Enums;

namespace AssetVest.Application.DTOs.AnnualGoals;

public record CreateAnnualGoalRequest
{
    public required int Year { get; init; }
    public required decimal TargetTotalPortfolioValueEGP { get; init; }
    public decimal? TargetProfitPercent { get; init; }
    public string? Notes { get; init; }
    public List<CreateAllocationGoalRequest>? AllocationGoals { get; init; }
}

public record CreateAllocationGoalRequest
{
    public required AssetType AssetType { get; init; }
    public required decimal TargetAllocationPercent { get; init; }
}
