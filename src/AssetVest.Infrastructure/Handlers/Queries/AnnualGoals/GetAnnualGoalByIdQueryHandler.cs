using AssetVest.Application.DTOs.AnnualGoals;
using AssetVest.Application.Ports;
using AssetVest.Application.Queries.AnnualGoals.GetAnnualGoalById;
using AssetVest.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssetVest.Infrastructure.Handlers.Queries.AnnualGoals;

public class GetAnnualGoalByIdQueryHandler(ApplicationDbContext context, ICurrentUserService currentUserService)
    : IRequestHandler<GetAnnualGoalByIdQuery, AnnualGoalDto?>
{
    public async Task<AnnualGoalDto?> Handle(GetAnnualGoalByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var goal = await context.AnnualGoals
            .AsNoTracking()
            .Include(g => g.AllocationGoals)
            .FirstOrDefaultAsync(g => g.Id == request.Id && g.UserId == userId, cancellationToken);

        return goal?.ToDto();
    }
}
