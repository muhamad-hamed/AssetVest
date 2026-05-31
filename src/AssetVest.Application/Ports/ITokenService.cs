using AssetVest.Domain.Entities;

namespace AssetVest.Application.Ports;

/// <summary>
/// Service for generating and validating JWT access tokens and refresh tokens
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// Generate a JWT access token for a user
    /// </summary>
    string GenerateAccessToken(User user);

    /// <summary>
    /// Generate a cryptographically secure refresh token
    /// </summary>
    string GenerateRefreshToken();

    /// <summary>
    /// Hash a refresh token for storage
    /// </summary>
    string HashRefreshToken(string token);

    /// <summary>
    /// Verify a refresh token against its hash
    /// </summary>
    bool VerifyRefreshToken(string token, string hash);
}
