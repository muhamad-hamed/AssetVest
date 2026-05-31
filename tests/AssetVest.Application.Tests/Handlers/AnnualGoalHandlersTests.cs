using AssetVest.Application.Commands.AnnualGoals.CreateAnnualGoal;
using AssetVest.Application.Commands.AnnualGoals.DeleteAnnualGoal;
using AssetVest.Application.Commands.AnnualGoals.UpdateAnnualGoal;
using AssetVest.Application.DTOs.AnnualGoals;
using AssetVest.Application.Ports;
using AssetVest.Application.Queries.AnnualGoals.GetAllAnnualGoals;
using AssetVest.Application.Queries.AnnualGoals.GetAnnualGoalById;
using AssetVest.Application.Queries.AnnualGoals.GetAnnualGoalByYear;
using AssetVest.Domain.Entities;
using AssetVest.Domain.Enums;
using AssetVest.Infrastructure.Handlers.Commands.AnnualGoals;
using AssetVest.Infrastructure.Handlers.Queries.AnnualGoals;
using AssetVest.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AssetVest.Application.Tests.Handlers;

public class AnnualGoalHandlersTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Guid _userId = Guid.NewGuid();

    public AnnualGoalHandlersTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _mockCurrentUserService = new Mock<ICurrentUserService>();
        _mockCurrentUserService.Setup(x => x.UserId).Returns(_userId);

        _context = new ApplicationDbContext(options, _mockCurrentUserService.Object);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    #region GetAllAnnualGoalsQueryHandler

    [Fact]
    public async Task GetAllAnnualGoals_ReturnsOnlyCurrentUsersGoals()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        _context.AnnualGoals.AddRange(
            CreateGoal(_userId, 2025),
            CreateGoal(_userId, 2026),
            CreateGoal(otherUserId, 2026)
        );
        await _context.SaveChangesAsync();

        var handler = new GetAllAnnualGoalsQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAllAnnualGoalsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(g => g.UserId == _userId);
    }

    [Fact]
    public async Task GetAllAnnualGoals_OrdersByYearDescending()
    {
        // Arrange
        _context.AnnualGoals.AddRange(
            CreateGoal(_userId, 2024),
            CreateGoal(_userId, 2026),
            CreateGoal(_userId, 2025)
        );
        await _context.SaveChangesAsync();

        var handler = new GetAllAnnualGoalsQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAllAnnualGoalsQuery(), CancellationToken.None);

        // Assert
        result.Should().BeInDescendingOrder(g => g.Year);
    }

    [Fact]
    public async Task GetAllAnnualGoals_IncludesAllocationGoals()
    {
        // Arrange
        var goal = CreateGoal(_userId, 2026);
        goal.AllocationGoals = new List<AssetTypeAllocationGoal>
        {
            new() { Id = Guid.NewGuid(), AnnualGoalId = goal.Id, AssetType = AssetType.Stocks, TargetAllocationPercent = 40 },
            new() { Id = Guid.NewGuid(), AnnualGoalId = goal.Id, AssetType = AssetType.Gold, TargetAllocationPercent = 30 }
        };
        _context.AnnualGoals.Add(goal);
        await _context.SaveChangesAsync();

        var handler = new GetAllAnnualGoalsQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAllAnnualGoalsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].AllocationGoals.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetAllAnnualGoals_WhenNotAuthenticated_ThrowsUnauthorized()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.UserId).Returns((Guid?)null);
        var handler = new GetAllAnnualGoalsQueryHandler(_context, _mockCurrentUserService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new GetAllAnnualGoalsQuery(), CancellationToken.None));
    }

    #endregion

    #region GetAnnualGoalByIdQueryHandler

    [Fact]
    public async Task GetAnnualGoalById_WhenGoalExists_ReturnsGoal()
    {
        // Arrange
        var goal = CreateGoal(_userId, 2026);
        _context.AnnualGoals.Add(goal);
        await _context.SaveChangesAsync();

        var handler = new GetAnnualGoalByIdQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAnnualGoalByIdQuery(goal.Id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(goal.Id);
        result.Year.Should().Be(2026);
    }

    [Fact]
    public async Task GetAnnualGoalById_WhenGoalBelongsToOtherUser_ReturnsNull()
    {
        // Arrange
        var goal = CreateGoal(Guid.NewGuid(), 2026);
        _context.AnnualGoals.Add(goal);
        await _context.SaveChangesAsync();

        var handler = new GetAnnualGoalByIdQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAnnualGoalByIdQuery(goal.Id), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAnnualGoalById_WhenGoalNotFound_ReturnsNull()
    {
        // Arrange
        var handler = new GetAnnualGoalByIdQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAnnualGoalByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAnnualGoalByYearQueryHandler

    [Fact]
    public async Task GetAnnualGoalByYear_WhenGoalExistsForYear_ReturnsGoal()
    {
        // Arrange
        _context.AnnualGoals.AddRange(
            CreateGoal(_userId, 2025),
            CreateGoal(_userId, 2026)
        );
        await _context.SaveChangesAsync();

        var handler = new GetAnnualGoalByYearQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAnnualGoalByYearQuery(2026), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Year.Should().Be(2026);
    }

    [Fact]
    public async Task GetAnnualGoalByYear_WhenNoGoalForYear_ReturnsNull()
    {
        // Arrange
        _context.AnnualGoals.Add(CreateGoal(_userId, 2025));
        await _context.SaveChangesAsync();

        var handler = new GetAnnualGoalByYearQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAnnualGoalByYearQuery(2026), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAnnualGoalByYear_DoesNotReturnOtherUsersGoal()
    {
        // Arrange
        _context.AnnualGoals.Add(CreateGoal(Guid.NewGuid(), 2026));
        await _context.SaveChangesAsync();

        var handler = new GetAnnualGoalByYearQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAnnualGoalByYearQuery(2026), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region CreateAnnualGoalCommandHandler

    [Fact]
    public async Task CreateAnnualGoal_WithValidData_CreatesGoal()
    {
        // Arrange
        var handler = new CreateAnnualGoalCommandHandler(_context, _mockCurrentUserService.Object);
        var command = new CreateAnnualGoalCommand
        {
            Year = 2026,
            TargetTotalPortfolioValueEGP = 500000m,
            TargetProfitPercent = 15m,
            Notes = "Test goal",
            AllocationGoals =
            [
                new CreateAllocationGoalRequest { AssetType = AssetType.Stocks, TargetAllocationPercent = 40 },
                new CreateAllocationGoalRequest { AssetType = AssetType.Gold, TargetAllocationPercent = 30 }
            ]
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Year.Should().Be(2026);
        result.UserId.Should().Be(_userId);
        result.TargetTotalPortfolioValueEGP.Should().Be(500000m);
        result.TargetProfitPercent.Should().Be(15m);
        result.AllocationGoals.Should().HaveCount(2);
    }

    [Fact]
    public async Task CreateAnnualGoal_WithDuplicateYear_ThrowsInvalidOperation()
    {
        // Arrange
        _context.AnnualGoals.Add(CreateGoal(_userId, 2026));
        await _context.SaveChangesAsync();

        var handler = new CreateAnnualGoalCommandHandler(_context, _mockCurrentUserService.Object);
        var command = new CreateAnnualGoalCommand
        {
            Year = 2026,
            TargetTotalPortfolioValueEGP = 500000m
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAnnualGoal_SameYearDifferentUser_Succeeds()
    {
        // Arrange
        _context.AnnualGoals.Add(CreateGoal(Guid.NewGuid(), 2026));
        await _context.SaveChangesAsync();

        var handler = new CreateAnnualGoalCommandHandler(_context, _mockCurrentUserService.Object);
        var command = new CreateAnnualGoalCommand
        {
            Year = 2026,
            TargetTotalPortfolioValueEGP = 300000m
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Year.Should().Be(2026);
        result.UserId.Should().Be(_userId);
    }

    [Fact]
    public async Task CreateAnnualGoal_WithoutAllocations_CreatesGoalWithEmptyAllocations()
    {
        // Arrange
        var handler = new CreateAnnualGoalCommandHandler(_context, _mockCurrentUserService.Object);
        var command = new CreateAnnualGoalCommand
        {
            Year = 2026,
            TargetTotalPortfolioValueEGP = 200000m
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AllocationGoals.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAnnualGoal_WhenNotAuthenticated_ThrowsUnauthorized()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.UserId).Returns((Guid?)null);
        var handler = new CreateAnnualGoalCommandHandler(_context, _mockCurrentUserService.Object);
        var command = new CreateAnnualGoalCommand
        {
            Year = 2026,
            TargetTotalPortfolioValueEGP = 100000m
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    #endregion

    #region UpdateAnnualGoalCommandHandler

    [Fact]
    public async Task UpdateAnnualGoal_WithValidData_UpdatesGoal()
    {
        // Arrange
        var goal = CreateGoal(_userId, 2026);
        _context.AnnualGoals.Add(goal);
        await _context.SaveChangesAsync();
        _context.ChangeTracker.Clear();

        var handler = new UpdateAnnualGoalCommandHandler(_context, _mockCurrentUserService.Object);
        var command = new UpdateAnnualGoalCommand
        {
            Id = goal.Id,
            TargetTotalPortfolioValueEGP = 750000m,
            TargetProfitPercent = 20m,
            Notes = "Updated"
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.TargetTotalPortfolioValueEGP.Should().Be(750000m);
        result.TargetProfitPercent.Should().Be(20m);
        result.Notes.Should().Be("Updated");
    }

    [Fact]
    public async Task UpdateAnnualGoal_WhenGoalBelongsToOtherUser_ReturnsNull()
    {
        // Arrange
        var goal = CreateGoal(Guid.NewGuid(), 2026);
        _context.AnnualGoals.Add(goal);
        await _context.SaveChangesAsync();

        var handler = new UpdateAnnualGoalCommandHandler(_context, _mockCurrentUserService.Object);
        var command = new UpdateAnnualGoalCommand
        {
            Id = goal.Id,
            TargetTotalPortfolioValueEGP = 999999m
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAnnualGoal_WhenGoalNotFound_ReturnsNull()
    {
        // Arrange
        var handler = new UpdateAnnualGoalCommandHandler(_context, _mockCurrentUserService.Object);
        var command = new UpdateAnnualGoalCommand
        {
            Id = Guid.NewGuid(),
            TargetTotalPortfolioValueEGP = 100000m
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeleteAnnualGoalCommandHandler

    [Fact]
    public async Task DeleteAnnualGoal_WhenGoalExists_DeletesAndReturnsTrue()
    {
        // Arrange
        var goal = CreateGoal(_userId, 2026);
        _context.AnnualGoals.Add(goal);
        await _context.SaveChangesAsync();
        var goalId = goal.Id;

        var handler = new DeleteAnnualGoalCommandHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new DeleteAnnualGoalCommand(goalId), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
        var deletedGoal = await _context.AnnualGoals.FirstOrDefaultAsync(g => g.Id == goalId);
        deletedGoal.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAnnualGoal_WhenGoalBelongsToOtherUser_ReturnsFalse()
    {
        // Arrange
        var goal = CreateGoal(Guid.NewGuid(), 2026);
        _context.AnnualGoals.Add(goal);
        await _context.SaveChangesAsync();

        var handler = new DeleteAnnualGoalCommandHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new DeleteAnnualGoalCommand(goal.Id), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAnnualGoal_WhenGoalNotFound_ReturnsFalse()
    {
        // Arrange
        var handler = new DeleteAnnualGoalCommandHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new DeleteAnnualGoalCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helpers

    private static AnnualGoal CreateGoal(Guid userId, int year) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Year = year,
        TargetTotalPortfolioValueEGP = 500000m,
        TargetProfitPercent = 15m,
        Notes = $"Goal for {year}"
    };

    #endregion
}
