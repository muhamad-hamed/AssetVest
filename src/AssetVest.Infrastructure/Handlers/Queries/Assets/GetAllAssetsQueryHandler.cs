using AssetVest.Application.DTOs.Assets;
using AssetVest.Application.Ports;
using AssetVest.Application.Queries.Assets.GetAllAssets;
using AssetVest.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssetVest.Infrastructure.Handlers.Queries.Assets;

public class GetAllAssetsQueryHandler(ApplicationDbContext context, ICurrentUserService currentUserService) 
    : IRequestHandler<GetAllAssetsQuery, IReadOnlyList<AssetDto>>
{
    public async Task<IReadOnlyList<AssetDto>> Handle(GetAllAssetsQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var assets = await context.Assets
            .AsNoTracking()
            .Include(a => a.StockDetail)
            .Include(a => a.CurrencyDetail)
            .Include(a => a.GoldDetail)
            .Include(a => a.RealEstateDetail)
            .Include(a => a.MutualFundDetail)
            .Include(a => a.CryptoDetail)
            .Include(a => a.BondsDetail)
            .Where(a => a.UserId == userId)
            .OrderBy(a => a.AssetType)
            .ThenBy(a => a.Name)
            .ToListAsync(cancellationToken);

        return assets.Select(a => a.ToDto()).ToList();
    }
}
