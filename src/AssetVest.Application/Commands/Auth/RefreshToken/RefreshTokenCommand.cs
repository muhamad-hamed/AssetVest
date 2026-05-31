using AssetVest.Application.DTOs.Auth;
using MediatR;

namespace AssetVest.Application.Commands.Auth.RefreshToken;

public record RefreshTokenCommand : IRequest<AuthResponse>
{
    public required string RefreshToken { get; init; }
}
