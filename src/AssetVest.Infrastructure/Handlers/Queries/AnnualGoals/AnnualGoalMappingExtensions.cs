using AssetVest.Application.DTOs.AnnualGoals;
using AssetVest.Domain.Entities;

namespace AssetVest.Infrastructure.Handlers.Queries.AnnualGoals;

internal static class AnnualGoalMappingExtensions
{
    public static AnnualGoalDto ToDto(this AnnualGoal goal) => new()
    {
        Id = goal.Id,
        UserId = goal.UserId,
        Year = goal.Year,
        TargetTotalPortfolioValueEGP = goal.TargetTotalPortfolioValueEGP,
        TargetProfitPercent = goal.TargetProfitPercent,
        Notes = goal.Notes,
        CreatedAt = goal.CreatedAt,
        UpdatedAt = goal.UpdatedAt,
        AllocationGoals = goal.AllocationGoals.Select(a => new AllocationGoalDto
        {
            Id = a.Id,
            AssetType = a.AssetType,
            TargetAllocationPercent = a.TargetAllocationPercent
        }).ToList()
    };
}
