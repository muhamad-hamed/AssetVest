using AssetVest.Application.DTOs.Users;

namespace AssetVest.Application.DTOs.Auth;

public record AuthResponse
{
    public required string AccessToken { get; init; }
    public required string RefreshToken { get; init; }
    public required int ExpiresIn { get; init; }
    public required UserDto User { get; init; }
}
