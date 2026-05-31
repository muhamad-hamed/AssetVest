namespace AssetVest.Application.DTOs.Assets;

public record CreateStockDetailRequest
{
    public required string StockSymbol { get; init; }
    public string? Exchange { get; init; }
    public required decimal NumberOfUnits { get; init; }
    public required decimal PurchasePricePerUnitEGP { get; init; }
    public required decimal CurrentPricePerUnitEGP { get; init; }
}
