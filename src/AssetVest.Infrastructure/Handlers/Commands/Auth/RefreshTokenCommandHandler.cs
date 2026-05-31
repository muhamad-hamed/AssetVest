using AssetVest.Application.Commands.Auth.RefreshToken;
using AssetVest.Application.DTOs.Auth;
using AssetVest.Application.DTOs.Users;
using AssetVest.Application.Ports;
using AssetVest.Domain.Entities;
using AssetVest.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AssetVest.Infrastructure.Handlers.Commands.Auth;

public class RefreshTokenCommandHandler(
    ApplicationDbContext context,
    ITokenService tokenService,
    IConfiguration configuration) : IRequestHandler<RefreshTokenCommand, AuthResponse>
{
    private readonly int _refreshTokenExpirationDays = configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);
    private readonly int _accessTokenExpirationMinutes = configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes", 15);

    public async Task<AuthResponse> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        // Find all active refresh tokens
        var storedTokens = await context.RefreshTokens
            .Include(rt => rt.User)
            .Where(rt => rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
            .ToListAsync(cancellationToken);

        // Find matching token by verifying hash
        RefreshToken? matchingToken = null;
        foreach (var storedToken in storedTokens)
        {
            if (tokenService.VerifyRefreshToken(request.RefreshToken, storedToken.TokenHash))
            {
                matchingToken = storedToken;
                break;
            }
        }

        if (matchingToken is null)
            throw new UnauthorizedAccessException("Invalid or expired refresh token");

        var user = matchingToken.User;

        // Check if user is active
        if (!user.IsActive || user.IsDeleted)
            throw new UnauthorizedAccessException("User account is inactive or deleted");

        // Generate new tokens
        var newAccessToken = tokenService.GenerateAccessToken(user);
        var newRefreshToken = tokenService.GenerateRefreshToken();
        var newRefreshTokenHash = tokenService.HashRefreshToken(newRefreshToken);

        // Create new refresh token entity
        var newRefreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = newRefreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        // Revoke old token (token rotation)
        matchingToken.RevokedAt = DateTime.UtcNow;
        matchingToken.ReplacedByTokenId = newRefreshTokenEntity.Id;

        context.RefreshTokens.Add(newRefreshTokenEntity);
        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresIn = _accessTokenExpirationMinutes * 60, // seconds
            User = new UserDto
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                IsActive = user.IsActive,
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            }
        };
    }
}
