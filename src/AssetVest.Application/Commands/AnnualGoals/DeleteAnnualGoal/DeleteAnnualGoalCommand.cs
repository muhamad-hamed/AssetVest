using MediatR;

namespace AssetVest.Application.Commands.AnnualGoals.DeleteAnnualGoal;

/// <summary>
/// Command to delete an annual goal (soft delete)
/// </summary>
public record DeleteAnnualGoalCommand(Guid Id) : IRequest<bool>;
