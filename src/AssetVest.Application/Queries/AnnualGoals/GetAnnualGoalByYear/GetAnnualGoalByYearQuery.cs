using AssetVest.Application.DTOs.AnnualGoals;
using MediatR;

namespace AssetVest.Application.Queries.AnnualGoals.GetAnnualGoalByYear;

/// <summary>
/// Query to get an annual goal by year for the current user
/// </summary>
public record GetAnnualGoalByYearQuery(int Year) : IRequest<AnnualGoalDto?>;
