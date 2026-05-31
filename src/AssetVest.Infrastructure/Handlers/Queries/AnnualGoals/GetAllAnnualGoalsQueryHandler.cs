using AssetVest.Application.DTOs.AnnualGoals;
using AssetVest.Application.Ports;
using AssetVest.Application.Queries.AnnualGoals.GetAllAnnualGoals;
using AssetVest.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssetVest.Infrastructure.Handlers.Queries.AnnualGoals;

public class GetAllAnnualGoalsQueryHandler(ApplicationDbContext context, ICurrentUserService currentUserService)
    : IRequestHandler<GetAllAnnualGoalsQuery, IReadOnlyList<AnnualGoalDto>>
{
    public async Task<IReadOnlyList<AnnualGoalDto>> Handle(GetAllAnnualGoalsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var goals = await context.AnnualGoals
            .AsNoTracking()
            .Include(g => g.AllocationGoals)
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.Year)
            .ToListAsync(cancellationToken);

        return goals.Select(g => g.ToDto()).ToList();
    }
}
