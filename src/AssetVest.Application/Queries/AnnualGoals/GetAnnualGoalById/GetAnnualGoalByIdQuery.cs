using AssetVest.Application.DTOs.AnnualGoals;
using MediatR;

namespace AssetVest.Application.Queries.AnnualGoals.GetAnnualGoalById;

/// <summary>
/// Query to get an annual goal by its ID
/// </summary>
public record GetAnnualGoalByIdQuery(Guid Id) : IRequest<AnnualGoalDto?>;
