using AssetVest.Application.DTOs.Assets;
using AssetVest.Application.Ports;
using AssetVest.Application.Queries.Assets.GetAssetsByType;
using AssetVest.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssetVest.Infrastructure.Handlers.Queries.Assets;

public class GetAssetsByTypeQueryHandler(ApplicationDbContext context, ICurrentUserService currentUserService) 
    : IRequestHandler<GetAssetsByTypeQuery, IReadOnlyList<AssetDto>>
{
    public async Task<IReadOnlyList<AssetDto>> Handle(GetAssetsByTypeQuery request, CancellationToken cancellationToken)
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
            .Where(a => a.UserId == userId && a.AssetType == request.AssetType)
            .OrderBy(a => a.Name)
            .ToListAsync(cancellationToken);

        return assets.Select(a => a.ToDto()).ToList();
    }
}
