using AssetVest.Domain.Enums;

namespace AssetVest.Application.DTOs.Assets;

public record MutualFundDetailDto
{
    public required string FundName { get; init; }
    public string? ManagementCompany { get; init; }
    public required MutualFundType FundType { get; init; }
    public required decimal NumberOfUnits { get; init; }
    public required decimal PurchaseNAVPerUnit { get; init; }
    public required decimal CurrentNAVPerUnit { get; init; }
}
