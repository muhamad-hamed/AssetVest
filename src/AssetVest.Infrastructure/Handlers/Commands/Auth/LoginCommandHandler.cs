using AssetVest.Application.Commands.Auth.Login;
using AssetVest.Application.DTOs.Auth;
using AssetVest.Application.DTOs.Users;
using AssetVest.Application.Ports;
using AssetVest.Domain.Entities;
using AssetVest.Infrastructure.Persistence;
using BCrypt.Net;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace AssetVest.Infrastructure.Handlers.Commands.Auth;

public class LoginCommandHandler(
    ApplicationDbContext context,
    ITokenService tokenService,
    IConfiguration configuration) : IRequestHandler<LoginCommand, AuthResponse>
{
    private readonly int _refreshTokenExpirationDays = configuration.GetValue<int>("Jwt:RefreshTokenExpirationDays", 7);
    private readonly int _accessTokenExpirationMinutes = configuration.GetValue<int>("Jwt:AccessTokenExpirationMinutes", 15);

    public async Task<AuthResponse> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // Find user by email
        var user = await context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && !u.IsDeleted, cancellationToken);

        if (user is null)
            throw new UnauthorizedAccessException("Invalid email or password");

        // Verify password
        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid email or password");

        // Check if user is active
        if (!user.IsActive)
            throw new UnauthorizedAccessException("User account is inactive");

        // Generate tokens
        var accessToken = tokenService.GenerateAccessToken(user);
        var refreshToken = tokenService.GenerateRefreshToken();
        var refreshTokenHash = tokenService.HashRefreshToken(refreshToken);

        // Revoke old refresh tokens (optional: keep last N tokens)
        var oldTokens = await context.RefreshTokens
            .Where(rt => rt.UserId == user.Id && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var oldToken in oldTokens)
        {
            oldToken.RevokedAt = DateTime.UtcNow;
        }

        // Store new refresh token
        var refreshTokenEntity = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            TokenHash = refreshTokenHash,
            ExpiresAt = DateTime.UtcNow.AddDays(_refreshTokenExpirationDays),
            CreatedAt = DateTime.UtcNow
        };

        context.RefreshTokens.Add(refreshTokenEntity);
        await context.SaveChangesAsync(cancellationToken);

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
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
