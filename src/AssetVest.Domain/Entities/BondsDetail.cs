namespace AssetVest.Domain.Entities;

public class BondsDetail
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string Issuer { get; set; } = string.Empty;
    public decimal FaceValueEGP { get; set; }
    public decimal CouponRatePercent { get; set; }
    public DateTime MaturityDate { get; set; }
    public decimal PurchasePriceEGP { get; set; }

    public Asset Asset { get; set; } = null!;
}
