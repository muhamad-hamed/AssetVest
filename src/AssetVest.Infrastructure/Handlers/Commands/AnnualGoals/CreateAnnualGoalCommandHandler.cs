using AssetVest.Application.Commands.AnnualGoals.CreateAnnualGoal;
using AssetVest.Application.DTOs.AnnualGoals;
using AssetVest.Application.Ports;
using AssetVest.Domain.Entities;
using AssetVest.Infrastructure.Handlers.Queries.AnnualGoals;
using AssetVest.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssetVest.Infrastructure.Handlers.Commands.AnnualGoals;

public class CreateAnnualGoalCommandHandler(ApplicationDbContext context, ICurrentUserService currentUserService)
    : IRequestHandler<CreateAnnualGoalCommand, AnnualGoalDto>
{
    public async Task<AnnualGoalDto> Handle(CreateAnnualGoalCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        // Check if a goal already exists for this year
        var existingGoal = await context.AnnualGoals
            .AnyAsync(g => g.UserId == userId && g.Year == request.Year, cancellationToken);

        if (existingGoal)
            throw new InvalidOperationException($"An annual goal already exists for year {request.Year}");

        var annualGoal = new AnnualGoal
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Year = request.Year,
            TargetTotalPortfolioValueEGP = request.TargetTotalPortfolioValueEGP,
            TargetProfitPercent = request.TargetProfitPercent,
            Notes = request.Notes
        };

        if (request.AllocationGoals is { Count: > 0 })
        {
            annualGoal.AllocationGoals = request.AllocationGoals.Select(a => new AssetTypeAllocationGoal
            {
                Id = Guid.NewGuid(),
                AnnualGoalId = annualGoal.Id,
                AssetType = a.AssetType,
                TargetAllocationPercent = a.TargetAllocationPercent
            }).ToList();
        }

        context.AnnualGoals.Add(annualGoal);
        await context.SaveChangesAsync(cancellationToken);

        return annualGoal.ToDto();
    }
}
