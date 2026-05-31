namespace AssetVest.Application.DTOs.AnnualGoals;

public record AnnualGoalDto
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required int Year { get; init; }
    public required decimal TargetTotalPortfolioValueEGP { get; init; }
    public decimal? TargetProfitPercent { get; init; }
    public string? Notes { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public IReadOnlyList<AllocationGoalDto> AllocationGoals { get; init; } = [];
}
