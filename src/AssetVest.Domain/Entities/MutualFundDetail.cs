using AssetVest.Domain.Enums;

namespace AssetVest.Domain.Entities;

public class MutualFundDetail
{
    public Guid Id { get; set; }
    public Guid AssetId { get; set; }
    public string FundName { get; set; } = string.Empty;
    public string? ManagementCompany { get; set; }
    public MutualFundType FundType { get; set; }
    public decimal NumberOfUnits { get; set; }
    public decimal PurchaseNAVPerUnit { get; set; }
    public decimal CurrentNAVPerUnit { get; set; }

    public Asset Asset { get; set; } = null!;
}
