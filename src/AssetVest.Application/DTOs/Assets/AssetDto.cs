using AssetVest.Domain.Enums;

namespace AssetVest.Application.DTOs.Assets;

public record AssetDto
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required string Name { get; init; }
    public required AssetType AssetType { get; init; }
    public required string BaseCurrency { get; init; }
    public required decimal InitialValueEGP { get; init; }
    public required decimal CurrentValueEGP { get; init; }
    public required decimal ProfitEGP { get; init; }
    public required decimal ProfitPercent { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }

    // Detail objects (only one will be populated based on AssetType)
    public StockDetailDto? StockDetail { get; init; }
    public CurrencyDetailDto? CurrencyDetail { get; init; }
    public GoldDetailDto? GoldDetail { get; init; }
    public RealEstateDetailDto? RealEstateDetail { get; init; }
    public MutualFundDetailDto? MutualFundDetail { get; init; }
    public CryptoDetailDto? CryptoDetail { get; init; }
    public BondsDetailDto? BondsDetail { get; init; }
}
