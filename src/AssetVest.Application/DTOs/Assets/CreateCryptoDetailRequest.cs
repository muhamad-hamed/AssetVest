namespace AssetVest.Application.DTOs.Assets;

public record CreateCryptoDetailRequest
{
    public required string Symbol { get; init; }
    public required decimal NumberOfUnits { get; init; }
    public required decimal PurchasePricePerUnitUSD { get; init; }
    public required decimal CurrentPricePerUnitUSD { get; init; }
    public required decimal UsdToEgpRate { get; init; }
}
