using AssetVest.Application.Commands.Auth.Login;
using AssetVest.Application.Commands.Auth.RefreshToken;
using AssetVest.Application.Commands.Auth.Register;
using AssetVest.Application.Ports;
using AssetVest.Domain.Entities;
using AssetVest.Infrastructure.Handlers.Commands.Auth;
using AssetVest.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Moq;
using Xunit;

namespace AssetVest.Application.Tests.Handlers;

public class AuthHandlersTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly IConfiguration _configuration;

    public AuthHandlersTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockCurrentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());

        _context = new ApplicationDbContext(options, _mockCurrentUserService.Object);

        _mockTokenService = new Mock<ITokenService>();
        _mockTokenService.Setup(x => x.GenerateAccessToken(It.IsAny<User>())).Returns("test-access-token");
        _mockTokenService.Setup(x => x.GenerateRefreshToken()).Returns("test-refresh-token");
        _mockTokenService.Setup(x => x.HashRefreshToken(It.IsAny<string>())).Returns("hashed-refresh-token");
        _mockTokenService.Setup(x => x.VerifyRefreshToken(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var inMemorySettings = new Dictionary<string, string?>
        {
            { "Jwt:RefreshTokenExpirationDays", "7" },
            { "Jwt:AccessTokenExpirationMinutes", "15" }
        };

        _configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region RegisterCommandHandler

    [Fact]
    public async Task Register_WithValidData_CreatesUserAndReturnsTokens()
    {
        // Arrange
        var handler = new RegisterCommandHandler(_context, _mockTokenService.Object, _configuration);
        var command = new RegisterCommand
        {
            FirstName = "Mohamed",
            LastName = "Hamed",
            Email = "mohamed@example.com",
            Password = "SecureP@ss123"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("test-access-token");
        result.RefreshToken.Should().Be("test-refresh-token");
        result.ExpiresIn.Should().Be(900); // 15 * 60
        result.User.Should().NotBeNull();
        result.User.FirstName.Should().Be("Mohamed");
        result.User.LastName.Should().Be("Hamed");
        result.User.Email.Should().Be("mohamed@example.com");
        result.User.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ThrowsInvalidOperation()
    {
        // Arrange
        _context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Existing",
            LastName = "User",
            Email = "duplicate@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password"),
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var handler = new RegisterCommandHandler(_context, _mockTokenService.Object, _configuration);
        var command = new RegisterCommand
        {
            FirstName = "New",
            LastName = "User",
            Email = "duplicate@example.com",
            Password = "Password123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Register_StoresHashedPassword_NotPlainText()
    {
        // Arrange
        var handler = new RegisterCommandHandler(_context, _mockTokenService.Object, _configuration);
        var command = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = "hash.test@example.com",
            Password = "MyPlainPassword!"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var user = await _context.Users.FirstAsync(u => u.Email == "hash.test@example.com");
        user.PasswordHash.Should().NotBe("MyPlainPassword!");
        BCrypt.Net.BCrypt.Verify("MyPlainPassword!", user.PasswordHash).Should().BeTrue();
    }

    [Fact]
    public async Task Register_StoresRefreshToken()
    {
        // Arrange
        var handler = new RegisterCommandHandler(_context, _mockTokenService.Object, _configuration);
        var command = new RegisterCommand
        {
            FirstName = "Token",
            LastName = "User",
            Email = "token.user@example.com",
            Password = "SecureP@ss1"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var refreshTokens = await _context.RefreshTokens.ToListAsync();
        refreshTokens.Should().HaveCount(1);
        refreshTokens[0].TokenHash.Should().Be("hashed-refresh-token");
        refreshTokens[0].ExpiresAt.Should().BeAfter(DateTime.UtcNow);
    }

    #endregion

    #region LoginCommandHandler

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsTokens()
    {
        // Arrange
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("CorrectP@ss1");
        _context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Login",
            LastName = "User",
            Email = "login@example.com",
            PasswordHash = passwordHash,
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var handler = new LoginCommandHandler(_context, _mockTokenService.Object, _configuration);
        var command = new LoginCommand
        {
            Email = "login@example.com",
            Password = "CorrectP@ss1"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("test-access-token");
        result.RefreshToken.Should().Be("test-refresh-token");
        result.User.Email.Should().Be("login@example.com");
    }

    [Fact]
    public async Task Login_WithWrongPassword_ThrowsUnauthorized()
    {
        // Arrange
        _context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Wrong",
            LastName = "Pass",
            Email = "wrong@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("RealPassword"),
            IsActive = true
        });
        await _context.SaveChangesAsync();

        var handler = new LoginCommandHandler(_context, _mockTokenService.Object, _configuration);
        var command = new LoginCommand
        {
            Email = "wrong@example.com",
            Password = "WrongPassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Login_WithNonExistentEmail_ThrowsUnauthorized()
    {
        // Arrange
        var handler = new LoginCommandHandler(_context, _mockTokenService.Object, _configuration);
        var command = new LoginCommand
        {
            Email = "nobody@example.com",
            Password = "AnyPassword"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Login_WithInactiveUser_ThrowsUnauthorized()
    {
        // Arrange
        _context.Users.Add(new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Inactive",
            LastName = "User",
            Email = "inactive@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1"),
            IsActive = false
        });
        await _context.SaveChangesAsync();

        var handler = new LoginCommandHandler(_context, _mockTokenService.Object, _configuration);
        var command = new LoginCommand
        {
            Email = "inactive@example.com",
            Password = "Password1"
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Login_RevokesOldRefreshTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _context.Users.Add(new User
        {
            Id = userId,
            FirstName = "Revoke",
            LastName = "Test",
            Email = "revoke@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password1"),
            IsActive = true
        });
        _context.RefreshTokens.Add(new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = "old-token-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        });
        await _context.SaveChangesAsync();

        var handler = new LoginCommandHandler(_context, _mockTokenService.Object, _configuration);
        var command = new LoginCommand
        {
            Email = "revoke@example.com",
            Password = "Password1"
        };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var oldTokens = await _context.RefreshTokens
            .Where(rt => rt.UserId == userId && rt.TokenHash == "old-token-hash")
            .ToListAsync();
        oldTokens.Should().AllSatisfy(t => t.RevokedAt.Should().NotBeNull());
    }

    #endregion

    #region RefreshTokenCommandHandler

    [Fact]
    public async Task RefreshToken_WithValidToken_ReturnsNewTokens()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            FirstName = "Refresh",
            LastName = "User",
            Email = "refresh@example.com",
            PasswordHash = "hash",
            IsActive = true
        };
        _context.Users.Add(user);

        var existingToken = new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = "valid-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };
        _context.RefreshTokens.Add(existingToken);
        await _context.SaveChangesAsync();

        // Setup token service to verify the matching token
        _mockTokenService.Setup(x => x.VerifyRefreshToken("valid-refresh-token", "valid-hash")).Returns(true);

        var handler = new RefreshTokenCommandHandler(_context, _mockTokenService.Object, _configuration);
        var command = new RefreshTokenCommand { RefreshToken = "valid-refresh-token" };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("test-access-token");
        result.RefreshToken.Should().Be("test-refresh-token");
        result.User.Email.Should().Be("refresh@example.com");
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ThrowsUnauthorized()
    {
        // Arrange
        _mockTokenService.Setup(x => x.VerifyRefreshToken(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        var handler = new RefreshTokenCommandHandler(_context, _mockTokenService.Object, _configuration);
        var command = new RefreshTokenCommand { RefreshToken = "invalid-token" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task RefreshToken_WithExpiredToken_ThrowsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _context.Users.Add(new User
        {
            Id = userId,
            FirstName = "Expired",
            LastName = "Token",
            Email = "expired@example.com",
            PasswordHash = "hash",
            IsActive = true
        });
        _context.RefreshTokens.Add(new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = "expired-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(-1), // Expired
            CreatedAt = DateTime.UtcNow.AddDays(-8)
        });
        await _context.SaveChangesAsync();

        _mockTokenService.Setup(x => x.VerifyRefreshToken("expired-token", "expired-hash")).Returns(true);

        var handler = new RefreshTokenCommandHandler(_context, _mockTokenService.Object, _configuration);
        var command = new RefreshTokenCommand { RefreshToken = "expired-token" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task RefreshToken_RotatesToken_RevokesOldAndCreatesNew()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _context.Users.Add(new User
        {
            Id = userId,
            FirstName = "Rotate",
            LastName = "User",
            Email = "rotate@example.com",
            PasswordHash = "hash",
            IsActive = true
        });

        var oldToken = new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = "old-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            CreatedAt = DateTime.UtcNow.AddDays(-2)
        };
        _context.RefreshTokens.Add(oldToken);
        await _context.SaveChangesAsync();

        _mockTokenService.Setup(x => x.VerifyRefreshToken("my-old-token", "old-hash")).Returns(true);

        var handler = new RefreshTokenCommandHandler(_context, _mockTokenService.Object, _configuration);
        var command = new RefreshTokenCommand { RefreshToken = "my-old-token" };

        // Act
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var revokedToken = await _context.RefreshTokens.FindAsync(oldToken.Id);
        revokedToken!.RevokedAt.Should().NotBeNull();
        revokedToken.ReplacedByTokenId.Should().NotBeNull();

        var allTokens = await _context.RefreshTokens.Where(rt => rt.UserId == userId).ToListAsync();
        allTokens.Should().HaveCount(2);
    }

    [Fact]
    public async Task RefreshToken_WithInactiveUser_ThrowsUnauthorized()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _context.Users.Add(new User
        {
            Id = userId,
            FirstName = "Inactive",
            LastName = "Refresh",
            Email = "inactive.refresh@example.com",
            PasswordHash = "hash",
            IsActive = false
        });
        _context.RefreshTokens.Add(new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            TokenHash = "inactive-hash",
            ExpiresAt = DateTime.UtcNow.AddDays(5),
            CreatedAt = DateTime.UtcNow.AddDays(-1)
        });
        await _context.SaveChangesAsync();

        _mockTokenService.Setup(x => x.VerifyRefreshToken("inactive-token", "inactive-hash")).Returns(true);

        var handler = new RefreshTokenCommandHandler(_context, _mockTokenService.Object, _configuration);
        var command = new RefreshTokenCommand { RefreshToken = "inactive-token" };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    #endregion
}
