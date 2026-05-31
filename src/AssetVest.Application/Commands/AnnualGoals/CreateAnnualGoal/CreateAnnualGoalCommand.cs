using AssetVest.Application.DTOs.AnnualGoals;
using MediatR;

namespace AssetVest.Application.Commands.AnnualGoals.CreateAnnualGoal;

/// <summary>
/// Command to create a new annual goal
/// </summary>
public record CreateAnnualGoalCommand : IRequest<AnnualGoalDto>
{
    public required int Year { get; init; }
    public required decimal TargetTotalPortfolioValueEGP { get; init; }
    public decimal? TargetProfitPercent { get; init; }
    public string? Notes { get; init; }
    public List<CreateAllocationGoalRequest>? AllocationGoals { get; init; }
}
