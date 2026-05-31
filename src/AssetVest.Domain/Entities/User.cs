using AssetVest.Domain.Common;

namespace AssetVest.Domain.Entities;

public class User : AuditableEntity
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Asset> Assets { get; set; } = [];
    public ICollection<AnnualGoal> AnnualGoals { get; set; } = [];
    public ICollection<StockProfitGoal> StockProfitGoals { get; set; } = [];
}
