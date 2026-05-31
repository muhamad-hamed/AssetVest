using AssetVest.Domain.Enums;

namespace AssetVest.Application.DTOs.AnnualGoals;

public record AllocationGoalDto
{
    public required Guid Id { get; init; }
    public required AssetType AssetType { get; init; }
    public required decimal TargetAllocationPercent { get; init; }
}
