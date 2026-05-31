namespace AssetVest.Domain.Entities;

public class StockDetail
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string StockSymbol { get; set; } = string.Empty;
    public string? Exchange { get; set; }
    public decimal NumberOfUnits { get; set; }
    public decimal PurchasePricePerUnitEGP { get; set; }
    public decimal CurrentPricePerUnitEGP { get; set; }

    public Asset Asset { get; set; } = null!;
}
