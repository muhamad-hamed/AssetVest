using MediatR;

namespace AssetVest.Application.Commands.Users.ChangePassword;

/// <summary>
/// Command to change a user's password
/// </summary>
public record ChangePasswordCommand : IRequest<bool>
{
    public required Guid UserId { get; init; }
    public required string CurrentPassword { get; init; }
    public required string NewPassword { get; init; }
}
