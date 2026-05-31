using AssetVest.Application.DTOs.Auth;
using MediatR;

namespace AssetVest.Application.Commands.Auth.Register;

public record RegisterCommand : IRequest<AuthResponse>
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}
