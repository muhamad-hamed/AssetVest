using AssetVest.Application.Commands.Users.CreateUser;
using AssetVest.Application.Commands.Users.DeleteUser;
using AssetVest.Application.Commands.Users.ToggleActiveStatus;
using AssetVest.Application.Commands.Users.UpdateUser;
using AssetVest.Application.Queries.Users.GetAllUsers;
using AssetVest.Application.Queries.Users.GetUserByEmail;
using AssetVest.Application.Queries.Users.GetUserById;
using AssetVest.Domain.Entities;
using CreateUserHandler = AssetVest.Infrastructure.Handlers.Commands.Users.CreateUserCommandHandler;
using UpdateUserHandler = AssetVest.Infrastructure.Handlers.Commands.Users.UpdateUserCommandHandler;
using DeleteUserHandler = AssetVest.Infrastructure.Handlers.Commands.Users.DeleteUserCommandHandler;
using ToggleActiveHandler = AssetVest.Infrastructure.Handlers.Commands.Users.ToggleUserActiveStatusCommandHandler;
using GetAllUsersHandler = AssetVest.Infrastructure.Handlers.Queries.Users.GetAllUsersQueryHandler;
using GetUserByIdHandler = AssetVest.Infrastructure.Handlers.Queries.Users.GetUserByIdQueryHandler;
using GetUserByEmailHandler = AssetVest.Infrastructure.Handlers.Queries.Users.GetUserByEmailQueryHandler;
using AssetVest.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AssetVest.Application.Tests.Handlers;

public class UserHandlersTests
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<AssetVest.Application.Ports.ICurrentUserService> _mockCurrentUserService;

    public UserHandlersTests()
    {
        // Setup in-memory database
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _mockCurrentUserService = new Mock<AssetVest.Application.Ports.ICurrentUserService>();
        _mockCurrentUserService.Setup(x => x.UserId).Returns(Guid.NewGuid());

        _context = new ApplicationDbContext(options, _mockCurrentUserService.Object);
    }

    [Fact]
    public async Task GetUserByIdQueryHandler_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "John",
            LastName = "Doe",
            Email = "john.doe@example.com",
            PasswordHash = "hashedpassword",
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var handler = new GetUserByIdHandler(_context);
        var query = new GetUserByIdQuery(user.Id);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
        result.FirstName.Should().Be("John");
        result.LastName.Should().Be("Doe");
        result.Email.Should().Be("john.doe@example.com");
        result.IsActive.Should().BeTrue();
    }

    [Fact]
    public async Task GetUserByIdQueryHandler_WhenUserDoesNotExist_ReturnsNull()
    {
        // Arrange
        var handler = new GetUserByIdHandler(_context);
        var query = new GetUserByIdQuery(Guid.NewGuid());

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetUserByEmailQueryHandler_WhenUserExists_ReturnsUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith",
            Email = "jane.smith@example.com",
            PasswordHash = "hashedpassword",
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var handler = new GetUserByEmailHandler(_context);
        var query = new GetUserByEmailQuery("jane.smith@example.com");

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Email.Should().Be("jane.smith@example.com");
    }

    [Fact]
    public async Task CreateUserCommandHandler_WithValidData_CreatesUser()
    {
        // Arrange
        var handler = new CreateUserHandler(_context);
        var command = new CreateUserCommand
        {
            FirstName = "Alice",
            LastName = "Johnson",
            Email = "alice.johnson@example.com",
            Password = "SecurePassword123!"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.FirstName.Should().Be("Alice");
        result.LastName.Should().Be("Johnson");
        result.Email.Should().Be("alice.johnson@example.com");
        result.IsActive.Should().BeTrue();

        var userInDb = await _context.Users.FindAsync(result.Id);
        userInDb.Should().NotBeNull();
        userInDb!.PasswordHash.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateUserCommandHandler_WithDuplicateEmail_ThrowsException()
    {
        // Arrange
        var existingUser = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Existing",
            LastName = "User",
            Email = "duplicate@example.com",
            PasswordHash = "hashedpassword",
            IsActive = true
        };

        _context.Users.Add(existingUser);
        await _context.SaveChangesAsync();

        var handler = new CreateUserHandler(_context);
        var command = new CreateUserCommand
        {
            FirstName = "New",
            LastName = "User",
            Email = "duplicate@example.com",
            Password = "Password123!"
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task UpdateUserCommandHandler_WithValidData_UpdatesUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Bob",
            LastName = "Brown",
            Email = "bob.brown@example.com",
            PasswordHash = "hashedpassword",
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var handler = new UpdateUserHandler(_context);
        var command = new UpdateUserCommand
        {
            UserId = user.Id,
            FirstName = "Robert",
            LastName = "Brown Jr",
            Email = "robert.brown@example.com"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.FirstName.Should().Be("Robert");
        result.LastName.Should().Be("Brown Jr");
        result.Email.Should().Be("robert.brown@example.com");
    }

    [Fact]
    public async Task DeleteUserCommandHandler_WhenUserExists_DeletesUser()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Delete",
            LastName = "Me",
            Email = "delete.me@example.com",
            PasswordHash = "hashedpassword",
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var handler = new DeleteUserHandler(_context);
        var command = new DeleteUserCommand(user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        // Verify soft delete
        var deletedUser = await _context.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Id == user.Id);

        deletedUser.Should().NotBeNull();
        deletedUser!.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetAllUsersQueryHandler_ReturnsAllActiveUsers()
    {
        // Arrange
        var users = new[]
        {
            new User { Id = Guid.NewGuid(), FirstName = "User1", LastName = "A", Email = "user1@example.com", PasswordHash = "hash", IsActive = true },
            new User { Id = Guid.NewGuid(), FirstName = "User2", LastName = "B", Email = "user2@example.com", PasswordHash = "hash", IsActive = true },
            new User { Id = Guid.NewGuid(), FirstName = "User3", LastName = "C", Email = "user3@example.com", PasswordHash = "hash", IsActive = true }
        };

        _context.Users.AddRange(users);
        await _context.SaveChangesAsync();

        var handler = new GetAllUsersHandler(_context);
        var query = new GetAllUsersQuery();

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().HaveCount(3);
        result.Should().BeInAscendingOrder(u => u.LastName);
    }

    [Fact]
    public async Task ToggleUserActiveStatusCommandHandler_WhenUserExists_TogglesStatus()
    {
        // Arrange
        var user = new User
        {
            Id = Guid.NewGuid(),
            FirstName = "Toggle",
            LastName = "Test",
            Email = "toggle@example.com",
            PasswordHash = "hashedpassword",
            IsActive = true
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var handler = new ToggleActiveHandler(_context);
        var command = new ToggleUserActiveStatusCommand(user.Id);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        var updatedUser = await _context.Users.FindAsync(user.Id);
        updatedUser!.IsActive.Should().BeFalse();

        // Toggle back
        await handler.Handle(command, CancellationToken.None);
        var toggledBackUser = await _context.Users.FindAsync(user.Id);
        toggledBackUser!.IsActive.Should().BeTrue();
    }
}
