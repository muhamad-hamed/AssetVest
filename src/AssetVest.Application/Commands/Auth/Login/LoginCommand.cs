using AssetVest.Application.DTOs.Auth;
using MediatR;

namespace AssetVest.Application.Commands.Auth.Login;

public record LoginCommand : IRequest<AuthResponse>
{
    public required string Email { get; init; }
    public required string Password { get; init; }
}
