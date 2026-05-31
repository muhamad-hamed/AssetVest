using AssetVest.Domain.Entities;
using AssetVest.Domain.Enums;
using FluentAssertions;
using Xunit;

namespace AssetVest.Domain.Tests.Entities;

public class AssetTests
{
    [Fact]
    public void RecalculateProfit_WithProfit_CalculatesCorrectly()
    {
        // Arrange
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Test Stock",
            AssetType = AssetType.Stocks,
            InitialValueEGP = 10000m,
            CurrentValueEGP = 12000m
        };

        // Act
        asset.RecalculateProfit();

        // Assert
        asset.ProfitEGP.Should().Be(2000m);
        asset.ProfitPercent.Should().Be(20m);
    }

    [Fact]
    public void RecalculateProfit_WithLoss_CalculatesCorrectly()
    {
        // Arrange
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Test Stock",
            AssetType = AssetType.Stocks,
            InitialValueEGP = 10000m,
            CurrentValueEGP = 8000m
        };

        // Act
        asset.RecalculateProfit();

        // Assert
        asset.ProfitEGP.Should().Be(-2000m);
        asset.ProfitPercent.Should().Be(-20m);
    }

    [Fact]
    public void RecalculateProfit_WithNoChange_ReturnsZero()
    {
        // Arrange
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Test Stock",
            AssetType = AssetType.Stocks,
            InitialValueEGP = 10000m,
            CurrentValueEGP = 10000m
        };

        // Act
        asset.RecalculateProfit();

        // Assert
        asset.ProfitEGP.Should().Be(0m);
        asset.ProfitPercent.Should().Be(0m);
    }

    [Fact]
    public void RecalculateProfit_WithZeroInitialValue_ReturnsZeroPercent()
    {
        // Arrange
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Test Asset",
            AssetType = AssetType.Cash,
            InitialValueEGP = 0m,
            CurrentValueEGP = 5000m
        };

        // Act
        asset.RecalculateProfit();

        // Assert
        asset.ProfitEGP.Should().Be(5000m);
        asset.ProfitPercent.Should().Be(0m); // Division by zero protection
    }

    [Fact]
    public void RecalculateProfit_RoundsPercentToTwoDecimals()
    {
        // Arrange
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Test Stock",
            AssetType = AssetType.Stocks,
            InitialValueEGP = 3m,
            CurrentValueEGP = 4m
        };

        // Act
        asset.RecalculateProfit();

        // Assert
        asset.ProfitEGP.Should().Be(1m);
        asset.ProfitPercent.Should().Be(33.33m); // 1/3 * 100 = 33.33
    }

    [Theory]
    [InlineData(1000, 1500, 500, 50)]
    [InlineData(1000, 900, -100, -10)]
    [InlineData(5000, 5250, 250, 5)]
    [InlineData(10000, 15000, 5000, 50)]
    public void RecalculateProfit_VariousScenarios_CalculatesCorrectly(
        decimal initial, decimal current, decimal expectedProfit, decimal expectedPercent)
    {
        // Arrange
        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Name = "Test Asset",
            AssetType = AssetType.Stocks,
            InitialValueEGP = initial,
            CurrentValueEGP = current
        };

        // Act
        asset.RecalculateProfit();

        // Assert
        asset.ProfitEGP.Should().Be(expectedProfit);
        asset.ProfitPercent.Should().Be(expectedPercent);
    }
}
