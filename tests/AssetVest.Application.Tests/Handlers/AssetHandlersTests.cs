using AssetVest.Application.Commands.Assets.CreateAsset;
using AssetVest.Application.Commands.Assets.DeleteAsset;
using AssetVest.Application.Commands.Assets.UpdateAsset;
using AssetVest.Application.DTOs.Assets;
using AssetVest.Application.Ports;
using AssetVest.Application.Queries.Assets.GetAllAssets;
using AssetVest.Application.Queries.Assets.GetAssetById;
using AssetVest.Application.Queries.Assets.GetAssetsByType;
using AssetVest.Domain.Entities;
using AssetVest.Domain.Enums;
using AssetVest.Infrastructure.Handlers.Commands.Assets;
using AssetVest.Infrastructure.Handlers.Queries.Assets;
using AssetVest.Infrastructure.Persistence;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using Xunit;

namespace AssetVest.Application.Tests.Handlers;

public class AssetHandlersTests : IDisposable
{
    private readonly ApplicationDbContext _context;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;
    private readonly Guid _userId = Guid.NewGuid();

    public AssetHandlersTests()
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

    #region GetAllAssetsQueryHandler

    [Fact]
    public async Task GetAllAssets_ReturnsOnlyCurrentUsersAssets()
    {
        // Arrange
        var otherUserId = Guid.NewGuid();
        _context.Assets.AddRange(
            CreateAsset(_userId, "My Stock", AssetType.Stocks),
            CreateAsset(_userId, "My Gold", AssetType.Gold),
            CreateAsset(otherUserId, "Other User Stock", AssetType.Stocks)
        );
        await _context.SaveChangesAsync();

        var handler = new GetAllAssetsQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAllAssetsQuery(), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(a => a.UserId == _userId);
    }

    [Fact]
    public async Task GetAllAssets_WhenNoAssets_ReturnsEmptyList()
    {
        // Arrange
        var handler = new GetAllAssetsQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAllAssetsQuery(), CancellationToken.None);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAssets_WhenNotAuthenticated_ThrowsUnauthorized()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.UserId).Returns((Guid?)null);
        var handler = new GetAllAssetsQueryHandler(_context, _mockCurrentUserService.Object);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(new GetAllAssetsQuery(), CancellationToken.None));
    }

    #endregion

    #region GetAssetByIdQueryHandler

    [Fact]
    public async Task GetAssetById_WhenAssetExists_ReturnsAsset()
    {
        // Arrange
        var asset = CreateAsset(_userId, "Test Stock", AssetType.Stocks);
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        var handler = new GetAssetByIdQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAssetByIdQuery(asset.Id), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(asset.Id);
        result.Name.Should().Be("Test Stock");
    }

    [Fact]
    public async Task GetAssetById_WhenAssetBelongsToOtherUser_ReturnsNull()
    {
        // Arrange
        var asset = CreateAsset(Guid.NewGuid(), "Other User Asset", AssetType.Gold);
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        var handler = new GetAssetByIdQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAssetByIdQuery(asset.Id), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetAssetById_WhenAssetDoesNotExist_ReturnsNull()
    {
        // Arrange
        var handler = new GetAssetByIdQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAssetByIdQuery(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region GetAssetsByTypeQueryHandler

    [Fact]
    public async Task GetAssetsByType_ReturnsOnlyMatchingType()
    {
        // Arrange
        _context.Assets.AddRange(
            CreateAsset(_userId, "Stock 1", AssetType.Stocks),
            CreateAsset(_userId, "Stock 2", AssetType.Stocks),
            CreateAsset(_userId, "Gold 1", AssetType.Gold)
        );
        await _context.SaveChangesAsync();

        var handler = new GetAssetsByTypeQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAssetsByTypeQuery(AssetType.Stocks), CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(a => a.AssetType == AssetType.Stocks);
    }

    [Fact]
    public async Task GetAssetsByType_DoesNotReturnOtherUsersAssets()
    {
        // Arrange
        _context.Assets.AddRange(
            CreateAsset(_userId, "My Stock", AssetType.Stocks),
            CreateAsset(Guid.NewGuid(), "Other Stock", AssetType.Stocks)
        );
        await _context.SaveChangesAsync();

        var handler = new GetAssetsByTypeQueryHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new GetAssetsByTypeQuery(AssetType.Stocks), CancellationToken.None);

        // Assert
        result.Should().HaveCount(1);
        result[0].Name.Should().Be("My Stock");
    }

    #endregion

    #region CreateAssetCommandHandler

    [Fact]
    public async Task CreateAsset_WithValidStockData_CreatesAsset()
    {
        // Arrange
        var handler = new CreateAssetCommandHandler(_context, _mockCurrentUserService.Object);
        var command = new CreateAssetCommand
        {
            Name = "COMI",
            AssetType = AssetType.Stocks,
            BaseCurrency = "EGP",
            InitialValueEGP = 10000m,
            CurrentValueEGP = 12000m,
            Notes = "Commercial International Bank",
            StockDetail = new CreateStockDetailRequest
            {
                StockSymbol = "COMI",
                Exchange = "EGX",
                NumberOfUnits = 100,
                PurchasePricePerUnitEGP = 100m,
                CurrentPricePerUnitEGP = 120m
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("COMI");
        result.AssetType.Should().Be(AssetType.Stocks);
        result.UserId.Should().Be(_userId);
        result.InitialValueEGP.Should().Be(10000m);
        result.CurrentValueEGP.Should().Be(12000m);
        result.StockDetail.Should().NotBeNull();
        result.StockDetail!.StockSymbol.Should().Be("COMI");
        result.StockDetail.NumberOfUnits.Should().Be(100);
    }

    [Fact]
    public async Task CreateAsset_WhenNotAuthenticated_ThrowsUnauthorized()
    {
        // Arrange
        _mockCurrentUserService.Setup(x => x.UserId).Returns((Guid?)null);
        var handler = new CreateAssetCommandHandler(_context, _mockCurrentUserService.Object);
        var command = new CreateAssetCommand
        {
            Name = "Test",
            AssetType = AssetType.Gold,
            InitialValueEGP = 1000m,
            CurrentValueEGP = 1100m
        };

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(
            () => handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task CreateAsset_SetsCorrectUserId()
    {
        // Arrange
        var handler = new CreateAssetCommandHandler(_context, _mockCurrentUserService.Object);
        var command = new CreateAssetCommand
        {
            Name = "Cash Reserve",
            AssetType = AssetType.Cash,
            InitialValueEGP = 50000m,
            CurrentValueEGP = 55000m
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var assetInDb = await _context.Assets.FindAsync(result.Id);
        assetInDb.Should().NotBeNull();
        assetInDb!.UserId.Should().Be(_userId);
    }

    #endregion

    #region UpdateAssetCommandHandler

    [Fact]
    public async Task UpdateAsset_WithValidData_UpdatesAsset()
    {
        // Arrange
        var asset = CreateAsset(_userId, "Old Name", AssetType.Stocks);
        asset.StockDetail = new StockDetail
        {
            Id = Guid.NewGuid(),
            AssetId = asset.Id,
            StockSymbol = "OLD",
            NumberOfUnits = 50,
            PurchasePricePerUnitEGP = 80m,
            CurrentPricePerUnitEGP = 90m
        };
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        var handler = new UpdateAssetCommandHandler(_context, _mockCurrentUserService.Object);
        var command = new UpdateAssetCommand
        {
            AssetId = asset.Id,
            Name = "New Name",
            AssetType = AssetType.Stocks,
            CurrentValueEGP = 15000m,
            StockDetail = new CreateStockDetailRequest
            {
                StockSymbol = "NEW",
                Exchange = "EGX",
                NumberOfUnits = 100,
                PurchasePricePerUnitEGP = 100m,
                CurrentPricePerUnitEGP = 150m
            }
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("New Name");
        result.CurrentValueEGP.Should().Be(15000m);
        result.StockDetail.Should().NotBeNull();
        result.StockDetail!.StockSymbol.Should().Be("NEW");
    }

    [Fact]
    public async Task UpdateAsset_WhenAssetBelongsToOtherUser_ReturnsNull()
    {
        // Arrange
        var asset = CreateAsset(Guid.NewGuid(), "Other Asset", AssetType.Gold);
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        var handler = new UpdateAssetCommandHandler(_context, _mockCurrentUserService.Object);
        var command = new UpdateAssetCommand
        {
            AssetId = asset.Id,
            Name = "Hacked",
            AssetType = AssetType.Gold,
            CurrentValueEGP = 99999m
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task UpdateAsset_WhenAssetNotFound_ReturnsNull()
    {
        // Arrange
        var handler = new UpdateAssetCommandHandler(_context, _mockCurrentUserService.Object);
        var command = new UpdateAssetCommand
        {
            AssetId = Guid.NewGuid(),
            Name = "Ghost",
            AssetType = AssetType.Stocks,
            CurrentValueEGP = 1000m
        };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeNull();
    }

    #endregion

    #region DeleteAssetCommandHandler

    [Fact]
    public async Task DeleteAsset_WhenAssetExists_DeletesAndReturnsTrue()
    {
        // Arrange
        var asset = CreateAsset(_userId, "To Delete", AssetType.Cash);
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        var handler = new DeleteAssetCommandHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new DeleteAssetCommand(asset.Id), CancellationToken.None);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsset_WhenAssetBelongsToOtherUser_ReturnsFalse()
    {
        // Arrange
        var asset = CreateAsset(Guid.NewGuid(), "Other Asset", AssetType.Cash);
        _context.Assets.Add(asset);
        await _context.SaveChangesAsync();

        var handler = new DeleteAssetCommandHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new DeleteAssetCommand(asset.Id), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAsset_WhenAssetNotFound_ReturnsFalse()
    {
        // Arrange
        var handler = new DeleteAssetCommandHandler(_context, _mockCurrentUserService.Object);

        // Act
        var result = await handler.Handle(new DeleteAssetCommand(Guid.NewGuid()), CancellationToken.None);

        // Assert
        result.Should().BeFalse();
    }

    #endregion

    #region Helpers

    private static Asset CreateAsset(Guid userId, string name, AssetType type) => new()
    {
        Id = Guid.NewGuid(),
        UserId = userId,
        Name = name,
        AssetType = type,
        BaseCurrency = "EGP",
        InitialValueEGP = 10000m,
        CurrentValueEGP = 11000m
    };

    #endregion
}
