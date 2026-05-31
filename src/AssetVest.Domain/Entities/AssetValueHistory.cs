using AssetVest.Domain.Enums;

namespace AssetVest.Domain.Entities;

public class AssetValueHistory
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public decimal ValueEGP { get; set; }
    public decimal ProfitEGP { get; set; }
    public decimal ProfitPercent { get; set; }
    public DateTime RecordedAt { get; set; }
    public AssetValueSource Source { get; set; }
    public string? Notes { get; set; }

    public Asset Asset { get; set; } = null!;
}
