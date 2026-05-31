namespace AssetVest.Domain.Entities;

public class RealEstateDetail
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string? Location { get; set; }
    public decimal? AreaSqm { get; set; }
    public decimal PurchaseValueEGP { get; set; }
    public decimal CurrentEstimatedValueEGP { get; set; }

    public Asset Asset { get; set; } = null!;
}
