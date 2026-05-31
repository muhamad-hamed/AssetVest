namespace AssetVest.Application.DTOs.Assets;

public record CreateCurrencyDetailRequest
{
    public required string CurrencyCode { get; init; }
    public required decimal InitialAmount { get; init; }
    public required decimal CurrentFxRateToEGP { get; init; }
}
