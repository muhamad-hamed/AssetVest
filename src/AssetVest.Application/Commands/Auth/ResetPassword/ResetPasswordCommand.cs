using MediatR;

namespace AssetVest.Application.Commands.Auth.ResetPassword;

public record ResetPasswordCommand : IRequest<bool>
{
    public required string Token { get; init; }
    public required string NewPassword { get; init; }
}
