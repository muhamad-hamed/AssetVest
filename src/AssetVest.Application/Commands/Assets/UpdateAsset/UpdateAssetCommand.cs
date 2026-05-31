using AssetVest.Application.DTOs.Assets;
using AssetVest.Domain.Enums;
using MediatR;

namespace AssetVest.Application.Commands.Assets.UpdateAsset;

/// <summary>
/// Command to update an existing asset
/// </summary>
public record UpdateAssetCommand : IRequest<AssetDto?>
{
    public required Guid AssetId { get; init; }
    public required string Name { get; init; }
    public required AssetType AssetType { get; init; }
    public string BaseCurrency { get; init; } = "EGP";
    public required decimal CurrentValueEGP { get; init; }
    public string? Notes { get; init; }

    // Detail objects (only one should be populated based on AssetType)
    public CreateStockDetailRequest? StockDetail { get; init; }
    public CreateCurrencyDetailRequest? CurrencyDetail { get; init; }
    public CreateGoldDetailRequest? GoldDetail { get; init; }
    public CreateRealEstateDetailRequest? RealEstateDetail { get; init; }
    public CreateMutualFundDetailRequest? MutualFundDetail { get; init; }
    public CreateCryptoDetailRequest? CryptoDetail { get; init; }
    public CreateBondsDetailRequest? BondsDetail { get; init; }
}
