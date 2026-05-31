namespace AssetVest.Domain.Entities;

public class GoldDetail
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public decimal WeightGrams { get; set; }
    public int Karat { get; set; }
    public decimal PurchasePricePerGramEGP { get; set; }
    public decimal CurrentPricePerGramEGP { get; set; }

    public Asset Asset { get; set; } = null!;
}
