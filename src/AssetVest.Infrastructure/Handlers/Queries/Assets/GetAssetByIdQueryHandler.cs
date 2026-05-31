using AssetVest.Application.DTOs.Assets;
using AssetVest.Application.Ports;
using AssetVest.Application.Queries.Assets.GetAssetById;
using AssetVest.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssetVest.Infrastructure.Handlers.Queries.Assets;

public class GetAssetByIdQueryHandler(ApplicationDbContext context, ICurrentUserService currentUserService) 
    : IRequestHandler<GetAssetByIdQuery, AssetDto?>
{
    public async Task<AssetDto?> Handle(GetAssetByIdQuery request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var asset = await context.Assets
            .AsNoTracking()
            .Include(a => a.StockDetail)
            .Include(a => a.CurrencyDetail)
            .Include(a => a.GoldDetail)
            .Include(a => a.RealEstateDetail)
            .Include(a => a.MutualFundDetail)
            .Include(a => a.CryptoDetail)
            .Include(a => a.BondsDetail)
            .Where(a => a.Id == request.AssetId && a.UserId == userId)
            .FirstOrDefaultAsync(cancellationToken);

        return asset?.ToDto();
    }
}
