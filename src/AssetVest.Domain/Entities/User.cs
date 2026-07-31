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
    public string? AvatarUrl { get; set; }

    // Forgot-password reset token (SHA-256 hash stored, plain token sent to user)
    public string? PasswordResetTokenHash { get; set; }
    public DateTime? PasswordResetTokenExpiresAt { get; set; }

    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Asset> Assets { get; set; } = [];
    public ICollection<AnnualGoal> AnnualGoals { get; set; } = [];
    public ICollection<StockProfitGoal> StockProfitGoals { get; set; } = [];
}
