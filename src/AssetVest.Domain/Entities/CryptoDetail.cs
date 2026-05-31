namespace AssetVest.Domain.Entities;

public class CryptoDetail
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string Symbol { get; set; } = string.Empty;
    public decimal NumberOfUnits { get; set; }
    public decimal PurchasePricePerUnitUSD { get; set; }
    public decimal CurrentPricePerUnitUSD { get; set; }
    public decimal UsdToEgpRate { get; set; }

    public Asset Asset { get; set; } = null!;
}
