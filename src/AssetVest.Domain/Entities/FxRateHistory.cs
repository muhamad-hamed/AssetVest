namespace AssetVest.Domain.Entities;

public class FxRateHistory
{
    public Guid Id { get; set; }
    public string FromCurrency { get; set; } = string.Empty;
    public string ToCurrency { get; set; } = "EGP";
    public decimal Rate { get; set; }
    public string Source { get; set; } = string.Empty;
    public DateTime RecordedAt { get; set; }
}
