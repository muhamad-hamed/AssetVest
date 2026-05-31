namespace AssetVest.Domain.Entities;

public class CurrencyDetail
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal InitialAmount { get; set; }
    public decimal CurrentFxRateToEGP { get; set; }
    public decimal CurrentValueEGP { get; set; }

    public Asset Asset { get; set; } = null!;
}
