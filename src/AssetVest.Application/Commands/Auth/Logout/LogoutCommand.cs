using MediatR;

namespace AssetVest.Application.Commands.Auth.Logout;

/// <summary>
/// Command to revoke all refresh tokens for the current user (logout)
/// </summary>
public record LogoutCommand : IRequest<bool>;
