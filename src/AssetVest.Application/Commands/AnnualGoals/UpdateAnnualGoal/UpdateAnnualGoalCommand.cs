using AssetVest.Application.DTOs.AnnualGoals;
using MediatR;

namespace AssetVest.Application.Commands.AnnualGoals.UpdateAnnualGoal;

/// <summary>
/// Command to update an existing annual goal
/// </summary>
public record UpdateAnnualGoalCommand : IRequest<AnnualGoalDto?>
{
    public required Guid Id { get; init; }
    public required decimal TargetTotalPortfolioValueEGP { get; init; }
    public decimal? TargetProfitPercent { get; init; }
    public string? Notes { get; init; }
    public List<CreateAllocationGoalRequest>? AllocationGoals { get; init; }
}
