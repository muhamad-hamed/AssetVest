using AssetVest.Application.DTOs.AnnualGoals;
using AssetVest.Application.Ports;
using AssetVest.Application.Queries.AnnualGoals.GetAnnualGoalByYear;
using AssetVest.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssetVest.Infrastructure.Handlers.Queries.AnnualGoals;

public class GetAnnualGoalByYearQueryHandler(ApplicationDbContext context, ICurrentUserService currentUserService)
    : IRequestHandler<GetAnnualGoalByYearQuery, AnnualGoalDto?>
{
    public async Task<AnnualGoalDto?> Handle(GetAnnualGoalByYearQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var goal = await context.AnnualGoals
            .AsNoTracking()
            .Include(g => g.AllocationGoals)
            .FirstOrDefaultAsync(g => g.Year == request.Year && g.UserId == userId, cancellationToken);

        return goal?.ToDto();
    }
}
