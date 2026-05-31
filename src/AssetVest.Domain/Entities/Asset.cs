using AssetVest.Domain.Common;
using AssetVest.Domain.Enums;

namespace AssetVest.Domain.Entities;

public class Asset : AuditableEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public AssetType AssetType { get; set; }
    public string BaseCurrency { get; set; } = "EGP";
    public decimal InitialValueEGP { get; set; }
    public decimal CurrentValueEGP { get; set; }
    public decimal ProfitEGP { get; private set; }
    public decimal ProfitPercent { get; private set; }
    public string? Notes { get; set; }

    public User User { get; set; } = null!;
    public ICollection<AssetValueHistory> ValueHistory { get; set; } = [];

    public StockDetail? StockDetail { get; set; }
    public CurrencyDetail? CurrencyDetail { get; set; }
    public GoldDetail? GoldDetail { get; set; }
    public RealEstateDetail? RealEstateDetail { get; set; }
    public MutualFundDetail? MutualFundDetail { get; set; }
    public CryptoDetail? CryptoDetail { get; set; }
    public BondsDetail? BondsDetail { get; set; }
    public StockProfitGoal? StockProfitGoal { get; set; }

    public void RecalculateProfit()
    {
        ProfitEGP = CurrentValueEGP - InitialValueEGP;
        ProfitPercent = InitialValueEGP == 0
            ? 0
            : Math.Round(ProfitEGP / InitialValueEGP * 100, 2);
    }
}
