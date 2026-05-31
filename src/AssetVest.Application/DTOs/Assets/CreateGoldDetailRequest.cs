namespace AssetVest.Application.DTOs.Assets;

public record CreateGoldDetailRequest
{
    public required decimal WeightGrams { get; init; }
    public required int Karat { get; init; }
    public required decimal PurchasePricePerGramEGP { get; init; }
    public required decimal CurrentPricePerGramEGP { get; init; }
}
