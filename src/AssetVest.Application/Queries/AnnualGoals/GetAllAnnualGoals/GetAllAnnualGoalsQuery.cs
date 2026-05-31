using AssetVest.Application.DTOs.AnnualGoals;
using MediatR;

namespace AssetVest.Application.Queries.AnnualGoals.GetAllAnnualGoals;

/// <summary>
/// Query to get all annual goals for the current user
/// </summary>
public record GetAllAnnualGoalsQuery : IRequest<IReadOnlyList<AnnualGoalDto>>;
