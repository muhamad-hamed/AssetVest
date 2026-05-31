namespace AssetVest.Application.DTOs.Assets;

public record BondsDetailDto
{
    public required string Issuer { get; init; }
    public required decimal FaceValueEGP { get; init; }
    public required decimal CouponRatePercent { get; init; }
    public required DateTime MaturityDate { get; init; }
    public required decimal PurchasePriceEGP { get; init; }
}
