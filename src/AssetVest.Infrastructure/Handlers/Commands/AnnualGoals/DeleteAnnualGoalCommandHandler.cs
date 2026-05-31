using AssetVest.Application.Commands.AnnualGoals.DeleteAnnualGoal;
using AssetVest.Application.Ports;
using AssetVest.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssetVest.Infrastructure.Handlers.Commands.AnnualGoals;

public class DeleteAnnualGoalCommandHandler(ApplicationDbContext context, ICurrentUserService currentUserService)
    : IRequestHandler<DeleteAnnualGoalCommand, bool>
{
    public async Task<bool> Handle(DeleteAnnualGoalCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var goal = await context.AnnualGoals
            .FirstOrDefaultAsync(g => g.Id == request.Id && g.UserId == userId, cancellationToken);

        if (goal is null)
            return false;

        context.AnnualGoals.Remove(goal);
        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
