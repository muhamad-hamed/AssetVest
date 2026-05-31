using System.ComponentModel.DataAnnotations;
using AssetVest.Domain.Enums;

namespace AssetVest.Application.DTOs.Assets;

public record CreateAssetRequest
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; init; }

    [Required]
    public required AssetType AssetType { get; init; }

    [MaxLength(3)]
    public string BaseCurrency { get; init; } = "EGP";

    [Required]
    [Range(0, double.MaxValue)]
    public required decimal InitialValueEGP { get; init; }

    [Required]
    [Range(0, double.MaxValue)]
    public required decimal CurrentValueEGP { get; init; }

    [MaxLength(1000)]
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
