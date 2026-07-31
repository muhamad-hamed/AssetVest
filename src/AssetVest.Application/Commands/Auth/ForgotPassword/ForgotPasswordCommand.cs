using MediatR;

namespace AssetVest.Application.Commands.Auth.ForgotPassword;

public record ForgotPasswordCommand : IRequest<ForgotPasswordResult>
{
    public required string Email { get; init; }
}

public record ForgotPasswordResult
{
    /// <summary>
    /// In production this would be sent via email.
    /// Returned here for development / testing purposes.
    /// </summary>
    public string ResetToken { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
}
