namespace AssetVest.Application.DTOs.Assets;

public record CreateRealEstateDetailRequest
{
    public required string Description { get; init; }
    public string? Location { get; init; }
    public decimal? AreaSqm { get; init; }
    public required decimal PurchaseValueEGP { get; init; }
    public required decimal CurrentEstimatedValueEGP { get; init; }
}
