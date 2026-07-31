using System.Security.Cryptography;
using System.Text;
using AssetVest.Application.Commands.Auth.ResetPassword;
using AssetVest.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssetVest.Infrastructure.Handlers.Commands.Auth;

public class ResetPasswordCommandHandler(ApplicationDbContext context)
    : IRequestHandler<ResetPasswordCommand, bool>
{
    public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        var tokenHash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(request.Token)));

        var user = await context.Users
            .FirstOrDefaultAsync(u =>
                u.PasswordResetTokenHash == tokenHash &&
                !u.IsDeleted &&
                u.IsActive,
                cancellationToken);

        if (user is null)
            throw new InvalidOperationException("Invalid or expired reset token.");

        if (user.PasswordResetTokenExpiresAt is null || user.PasswordResetTokenExpiresAt < DateTime.UtcNow)
            throw new InvalidOperationException("Reset token has expired.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetTokenHash = null;
        user.PasswordResetTokenExpiresAt = null;
        user.UpdatedAt = DateTime.UtcNow;

        // Existing sessions must not survive a password reset
        var activeTokens = await context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var refreshToken in activeTokens)
        {
            refreshToken.RevokedAt = DateTime.UtcNow;
        }

        await context.SaveChangesAsync(cancellationToken);

        return true;
    }
}
