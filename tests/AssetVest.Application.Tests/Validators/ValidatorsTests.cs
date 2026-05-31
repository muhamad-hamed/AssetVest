using AssetVest.Application.Commands.AnnualGoals.CreateAnnualGoal;
using AssetVest.Application.Commands.Assets.CreateAsset;
using AssetVest.Application.Commands.Auth.Login;
using AssetVest.Application.Commands.Auth.Register;
using AssetVest.Application.DTOs.AnnualGoals;
using AssetVest.Application.DTOs.Assets;
using AssetVest.Domain.Enums;
using FluentAssertions;
using FluentValidation.TestHelper;
using Xunit;

namespace AssetVest.Application.Tests.Validators;

public class AuthValidatorsTests
{
    #region RegisterCommandValidator

    private readonly RegisterCommandValidator _registerValidator = new();

    [Fact]
    public void Register_WithValidData_PassesValidation()
    {
        var command = new RegisterCommand
        {
            FirstName = "Mohamed",
            LastName = "Hamed",
            Email = "mohamed@example.com",
            Password = "H#test@2026"
        };

        var result = _registerValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Register_WithEmptyFirstName_Fails(string? firstName)
    {
        var command = new RegisterCommand
        {
            FirstName = firstName!,
            LastName = "Test",
            Email = "test@example.com",
            Password = "H#test@2026"
        };

        var result = _registerValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Register_WithEmptyEmail_Fails(string? email)
    {
        var command = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = email!,
            Password = "H#test@2026"
        };

        var result = _registerValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("notanemail")]
    [InlineData("@nodomain.com")]
    public void Register_WithInvalidEmail_Fails(string email)
    {
        var command = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = email,
            Password = "H#test@2026"
        };

        var result = _registerValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("short")]          // Too short
    [InlineData("nouppercase1!")]  // No uppercase
    [InlineData("NOLOWERCASE1!")]  // No lowercase
    [InlineData("NoNumber!!")]     // No digit
    [InlineData("NoSpecial1")]     // No special char
    public void Register_WithWeakPassword_Fails(string password)
    {
        var command = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = password
        };

        var result = _registerValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Theory]
    [InlineData("Strong@Pass1")]
    [InlineData("H#test@2026")]
    [InlineData("MyP@ssw0rd!")]
    public void Register_WithStrongPassword_PassesValidation(string password)
    {
        var command = new RegisterCommand
        {
            FirstName = "Test",
            LastName = "User",
            Email = "test@example.com",
            Password = password
        };

        var result = _registerValidator.TestValidate(command);
        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }

    #endregion

    #region LoginCommandValidator

    private readonly LoginCommandValidator _loginValidator = new();

    [Fact]
    public void Login_WithValidData_PassesValidation()
    {
        var command = new LoginCommand
        {
            Email = "user@example.com",
            Password = "anypassword"
        };

        var result = _loginValidator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Login_WithEmptyEmail_Fails()
    {
        var command = new LoginCommand
        {
            Email = "",
            Password = "password"
        };

        var result = _loginValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Login_WithInvalidEmail_Fails()
    {
        var command = new LoginCommand
        {
            Email = "not-an-email",
            Password = "password"
        };

        var result = _loginValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Login_WithEmptyPassword_Fails()
    {
        var command = new LoginCommand
        {
            Email = "test@example.com",
            Password = ""
        };

        var result = _loginValidator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    #endregion
}

public class AssetValidatorsTests
{
    private readonly CreateAssetCommandValidator _validator = new();

    [Fact]
    public void CreateAsset_WithValidStockData_PassesValidation()
    {
        var command = new CreateAssetCommand
        {
            Name = "COMI",
            AssetType = AssetType.Stocks,
            BaseCurrency = "EGP",
            InitialValueEGP = 10000m,
            CurrentValueEGP = 12000m,
            StockDetail = new CreateStockDetailRequest
            {
                StockSymbol = "COMI",
                Exchange = "EGX",
                NumberOfUnits = 100,
                PurchasePricePerUnitEGP = 100m,
                CurrentPricePerUnitEGP = 120m
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateAsset_WithEmptyName_Fails()
    {
        var command = new CreateAssetCommand
        {
            Name = "",
            AssetType = AssetType.Cash,
            InitialValueEGP = 1000m,
            CurrentValueEGP = 1100m
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void CreateAsset_WithNegativeInitialValue_Fails()
    {
        var command = new CreateAssetCommand
        {
            Name = "Test",
            AssetType = AssetType.Cash,
            InitialValueEGP = -100m,
            CurrentValueEGP = 100m
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.InitialValueEGP);
    }

    [Fact]
    public void CreateAsset_StockWithEmptySymbol_Fails()
    {
        var command = new CreateAssetCommand
        {
            Name = "Missing Symbol",
            AssetType = AssetType.Stocks,
            InitialValueEGP = 1000m,
            CurrentValueEGP = 1100m,
            StockDetail = new CreateStockDetailRequest
            {
                StockSymbol = "",
                NumberOfUnits = 100,
                PurchasePricePerUnitEGP = 50m,
                CurrentPricePerUnitEGP = 55m
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("StockDetail.StockSymbol");
    }

    [Fact]
    public void CreateAsset_StockWithZeroUnits_Fails()
    {
        var command = new CreateAssetCommand
        {
            Name = "Zero Units",
            AssetType = AssetType.Stocks,
            InitialValueEGP = 1000m,
            CurrentValueEGP = 1100m,
            StockDetail = new CreateStockDetailRequest
            {
                StockSymbol = "TEST",
                NumberOfUnits = 0,
                PurchasePricePerUnitEGP = 100m,
                CurrentPricePerUnitEGP = 110m
            }
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor("StockDetail.NumberOfUnits");
    }

    [Fact]
    public void CreateAsset_WithInvalidAssetType_Fails()
    {
        var command = new CreateAssetCommand
        {
            Name = "Invalid Type",
            AssetType = (AssetType)999,
            InitialValueEGP = 1000m,
            CurrentValueEGP = 1100m
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AssetType);
    }
}

public class AnnualGoalValidatorsTests
{
    private readonly CreateAnnualGoalCommandValidator _validator = new();

    [Fact]
    public void CreateGoal_WithValidData_PassesValidation()
    {
        var command = new CreateAnnualGoalCommand
        {
            Year = 2026,
            TargetTotalPortfolioValueEGP = 500000m,
            TargetProfitPercent = 15m,
            AllocationGoals =
            [
                new CreateAllocationGoalRequest { AssetType = AssetType.Stocks, TargetAllocationPercent = 40 },
                new CreateAllocationGoalRequest { AssetType = AssetType.Gold, TargetAllocationPercent = 30 }
            ]
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData(2019)]
    [InlineData(2101)]
    public void CreateGoal_WithInvalidYear_Fails(int year)
    {
        var command = new CreateAnnualGoalCommand
        {
            Year = year,
            TargetTotalPortfolioValueEGP = 500000m
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Year);
    }

    [Fact]
    public void CreateGoal_WithZeroTargetValue_Fails()
    {
        var command = new CreateAnnualGoalCommand
        {
            Year = 2026,
            TargetTotalPortfolioValueEGP = 0m
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetTotalPortfolioValueEGP);
    }

    [Fact]
    public void CreateGoal_WithNegativeTargetValue_Fails()
    {
        var command = new CreateAnnualGoalCommand
        {
            Year = 2026,
            TargetTotalPortfolioValueEGP = -100m
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.TargetTotalPortfolioValueEGP);
    }

    [Fact]
    public void CreateGoal_WithAllocationsExceeding100Percent_Fails()
    {
        var command = new CreateAnnualGoalCommand
        {
            Year = 2026,
            TargetTotalPortfolioValueEGP = 500000m,
            AllocationGoals =
            [
                new CreateAllocationGoalRequest { AssetType = AssetType.Stocks, TargetAllocationPercent = 60 },
                new CreateAllocationGoalRequest { AssetType = AssetType.Gold, TargetAllocationPercent = 50 }
            ]
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AllocationGoals);
    }

    [Fact]
    public void CreateGoal_WithAllocationExactly100Percent_PassesValidation()
    {
        var command = new CreateAnnualGoalCommand
        {
            Year = 2026,
            TargetTotalPortfolioValueEGP = 500000m,
            AllocationGoals =
            [
                new CreateAllocationGoalRequest { AssetType = AssetType.Stocks, TargetAllocationPercent = 50 },
                new CreateAllocationGoalRequest { AssetType = AssetType.Gold, TargetAllocationPercent = 50 }
            ]
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void CreateGoal_WithNegativeAllocationPercent_Fails()
    {
        var command = new CreateAnnualGoalCommand
        {
            Year = 2026,
            TargetTotalPortfolioValueEGP = 500000m,
            AllocationGoals =
            [
                new CreateAllocationGoalRequest { AssetType = AssetType.Stocks, TargetAllocationPercent = -10 }
            ]
        };

        var result = _validator.TestValidate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateGoal_WithInvalidAssetTypeInAllocation_Fails()
    {
        var command = new CreateAnnualGoalCommand
        {
            Year = 2026,
            TargetTotalPortfolioValueEGP = 500000m,
            AllocationGoals =
            [
                new CreateAllocationGoalRequest { AssetType = (AssetType)999, TargetAllocationPercent = 40 }
            ]
        };

        var result = _validator.TestValidate(command);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void CreateGoal_WithNotesTooLong_Fails()
    {
        var command = new CreateAnnualGoalCommand
        {
            Year = 2026,
            TargetTotalPortfolioValueEGP = 500000m,
            Notes = new string('x', 1001)
        };

        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Notes);
    }

    [Fact]
    public void CreateGoal_WithoutAllocations_PassesValidation()
    {
        var command = new CreateAnnualGoalCommand
        {
            Year = 2026,
            TargetTotalPortfolioValueEGP = 500000m
        };

        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }
}
