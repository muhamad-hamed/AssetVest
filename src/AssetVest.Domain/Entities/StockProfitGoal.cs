using AssetVest.Domain.Common;

namespace AssetVest.Domain.Entities;

public class StockProfitGoal : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid AssetId { get; set; }
    public int Year { get; set; }
    public decimal TargetProfitPercent { get; set; }
    public decimal? TargetProfitAmountEGP { get; set; }

    public User User { get; set; } = null!;
    public Asset Asset { get; set; } = null!;
}
