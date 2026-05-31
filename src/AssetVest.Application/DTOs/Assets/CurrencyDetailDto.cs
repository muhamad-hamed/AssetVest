namespace AssetVest.Application.DTOs.Assets;

public record CurrencyDetailDto
{
    public required string CurrencyCode { get; init; }
    public required decimal InitialAmount { get; init; }
    public required decimal CurrentFxRateToEGP { get; init; }
    public required decimal CurrentValueEGP { get; init; }
}
