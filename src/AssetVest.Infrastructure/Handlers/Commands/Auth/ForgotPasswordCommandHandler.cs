using System.Security.Cryptography;
using System.Text;
using AssetVest.Application.Commands.Auth.ForgotPassword;
using AssetVest.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssetVest.Infrastructure.Handlers.Commands.Auth;

public class ForgotPasswordCommandHandler(ApplicationDbContext context)
    : IRequestHandler<ForgotPasswordCommand, ForgotPasswordResult>
{
    private const int TokenExpirationMinutes = 30;

    public async Task<ForgotPasswordResult> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
    {
        // Always return a result to avoid user enumeration attacks
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);

        if (user is null)
        {
            // Return a dummy result — never reveal whether email exists
            return new ForgotPasswordResult
            {
                ResetToken = string.Empty,
                ExpiresAt = DateTime.UtcNow.AddMinutes(TokenExpirationMinutes)
            };
        }

        // Generate a cryptographically secure token
        var tokenBytes = RandomNumberGenerator.GetBytes(32);
        var token = Convert.ToBase64String(tokenBytes)
            .Replace("+", "-").Replace("/", "_").Replace("=", ""); // URL-safe

        // Store hash only
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

        user.PasswordResetTokenHash = tokenHash;
        user.PasswordResetTokenExpiresAt = DateTime.UtcNow.AddMinutes(TokenExpirationMinutes);
        user.UpdatedAt = DateTime.UtcNow;

        await context.SaveChangesAsync(cancellationToken);

        return new ForgotPasswordResult
        {
            ResetToken = token,
            ExpiresAt = user.PasswordResetTokenExpiresAt.Value
        };
    }
}
