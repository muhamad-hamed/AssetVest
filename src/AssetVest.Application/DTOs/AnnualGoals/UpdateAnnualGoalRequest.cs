using AssetVest.Domain.Enums;

namespace AssetVest.Application.DTOs.AnnualGoals;

public record UpdateAnnualGoalRequest
{
    public required decimal TargetTotalPortfolioValueEGP { get; init; }
    public decimal? TargetProfitPercent { get; init; }
    public string? Notes { get; init; }
    public List<CreateAllocationGoalRequest>? AllocationGoals { get; init; }
}
