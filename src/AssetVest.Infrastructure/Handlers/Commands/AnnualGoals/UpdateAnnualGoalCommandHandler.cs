using AssetVest.Application.Commands.AnnualGoals.UpdateAnnualGoal;
using AssetVest.Application.DTOs.AnnualGoals;
using AssetVest.Application.Ports;
using AssetVest.Domain.Entities;
using AssetVest.Infrastructure.Handlers.Queries.AnnualGoals;
using AssetVest.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssetVest.Infrastructure.Handlers.Commands.AnnualGoals;

public class UpdateAnnualGoalCommandHandler(ApplicationDbContext context, ICurrentUserService currentUserService)
    : IRequestHandler<UpdateAnnualGoalCommand, AnnualGoalDto?>
{
    public async Task<AnnualGoalDto?> Handle(UpdateAnnualGoalCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var goal = await context.AnnualGoals
            .Include(g => g.AllocationGoals)
            .FirstOrDefaultAsync(g => g.Id == request.Id && g.UserId == userId, cancellationToken);

        if (goal is null)
            return null;

        goal.TargetTotalPortfolioValueEGP = request.TargetTotalPortfolioValueEGP;
        goal.TargetProfitPercent = request.TargetProfitPercent;
        goal.Notes = request.Notes;

        // Replace allocation goals
        if (request.AllocationGoals is not null)
        {
            // Remove existing allocations
            context.Set<AssetTypeAllocationGoal>().RemoveRange(goal.AllocationGoals);

            // Add new allocations
            goal.AllocationGoals = request.AllocationGoals.Select(a => new AssetTypeAllocationGoal
            {
                Id = Guid.NewGuid(),
                AnnualGoalId = goal.Id,
                AssetType = a.AssetType,
                TargetAllocationPercent = a.TargetAllocationPercent
            }).ToList();
        }

        await context.SaveChangesAsync(cancellationToken);

        return goal.ToDto();
    }
}
