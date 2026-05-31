using AssetVest.Domain.Enums;
using FluentValidation;

namespace AssetVest.Application.Commands.Assets.CreateAsset;

public class CreateAssetCommandValidator : AbstractValidator<CreateAssetCommand>
{
    public CreateAssetCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Asset name is required")
            .MaximumLength(200).WithMessage("Asset name cannot exceed 200 characters");

        RuleFor(x => x.AssetType)
            .IsInEnum().WithMessage("Invalid asset type");

        RuleFor(x => x.BaseCurrency)
            .NotEmpty().WithMessage("Base currency is required")
            .MaximumLength(3).WithMessage("Currency code must be 3 characters");

        RuleFor(x => x.InitialValueEGP)
            .GreaterThanOrEqualTo(0).WithMessage("Initial value must be >= 0");

        RuleFor(x => x.CurrentValueEGP)
            .GreaterThanOrEqualTo(0).WithMessage("Current value must be >= 0");

        RuleFor(x => x.Notes)
            .MaximumLength(1000).When(x => x.Notes != null)
            .WithMessage("Notes cannot exceed 1000 characters");

        // Stock-specific validation
        When(x => x.AssetType == AssetType.Stocks, () =>
        {
            RuleFor(x => x.StockDetail)
                .NotNull().WithMessage("Stock details are required for stock assets");
            
            When(x => x.StockDetail != null, () =>
            {
                RuleFor(x => x.StockDetail!.StockSymbol)
                    .NotEmpty().WithMessage("Stock symbol is required");
                RuleFor(x => x.StockDetail!.NumberOfUnits)
                    .GreaterThan(0).WithMessage("Number of units must be greater than 0");
                RuleFor(x => x.StockDetail!.PurchasePricePerUnitEGP)
                    .GreaterThan(0).WithMessage("Purchase price per unit must be greater than 0");
                RuleFor(x => x.StockDetail!.CurrentPricePerUnitEGP)
                    .GreaterThanOrEqualTo(0).WithMessage("Current price per unit must be >= 0");
            });
        });

        // Currency-specific validation
        When(x => x.AssetType == AssetType.ForeignCurrency, () =>
        {
            RuleFor(x => x.CurrencyDetail)
                .NotNull().WithMessage("Currency details are required for foreign currency assets");
            
            When(x => x.CurrencyDetail != null, () =>
            {
                RuleFor(x => x.CurrencyDetail!.CurrencyCode)
                    .NotEmpty().Length(3).WithMessage("Valid 3-letter currency code is required");
                RuleFor(x => x.CurrencyDetail!.InitialAmount)
                    .GreaterThan(0).WithMessage("Initial amount must be greater than 0");
                RuleFor(x => x.CurrencyDetail!.CurrentFxRateToEGP)
                    .GreaterThan(0).WithMessage("Current FX rate must be greater than 0");
            });
        });

        // Gold-specific validation
        When(x => x.AssetType == AssetType.Gold, () =>
        {
            RuleFor(x => x.GoldDetail)
                .NotNull().WithMessage("Gold details are required for gold assets");
            
            When(x => x.GoldDetail != null, () =>
            {
                RuleFor(x => x.GoldDetail!.WeightGrams)
                    .GreaterThan(0).WithMessage("Weight in grams must be greater than 0");
                RuleFor(x => x.GoldDetail!.Karat)
                    .InclusiveBetween(10, 24).WithMessage("Karat must be between 10 and 24");
                RuleFor(x => x.GoldDetail!.PurchasePricePerGramEGP)
                    .GreaterThan(0).WithMessage("Purchase price per gram must be greater than 0");
                RuleFor(x => x.GoldDetail!.CurrentPricePerGramEGP)
                    .GreaterThanOrEqualTo(0).WithMessage("Current price per gram must be >= 0");
            });
        });

        // RealEstate-specific validation
        When(x => x.AssetType == AssetType.RealEstate, () =>
        {
            RuleFor(x => x.RealEstateDetail)
                .NotNull().WithMessage("Real estate details are required for real estate assets");
            
            When(x => x.RealEstateDetail != null, () =>
            {
                RuleFor(x => x.RealEstateDetail!.Description)
                    .NotEmpty().WithMessage("Description is required");
                RuleFor(x => x.RealEstateDetail!.PurchaseValueEGP)
                    .GreaterThan(0).WithMessage("Purchase value must be greater than 0");
                RuleFor(x => x.RealEstateDetail!.CurrentEstimatedValueEGP)
                    .GreaterThanOrEqualTo(0).WithMessage("Current estimated value must be >= 0");
                RuleFor(x => x.RealEstateDetail!.AreaSqm)
                    .GreaterThan(0).When(x => x.RealEstateDetail!.AreaSqm.HasValue)
                    .WithMessage("Area must be greater than 0");
            });
        });

        // MutualFund-specific validation
        When(x => x.AssetType == AssetType.MutualFunds, () =>
        {
            RuleFor(x => x.MutualFundDetail)
                .NotNull().WithMessage("Mutual fund details are required for mutual fund assets");
            
            When(x => x.MutualFundDetail != null, () =>
            {
                RuleFor(x => x.MutualFundDetail!.FundName)
                    .NotEmpty().WithMessage("Fund name is required");
                RuleFor(x => x.MutualFundDetail!.FundType)
                    .IsInEnum().WithMessage("Valid fund type is required");
                RuleFor(x => x.MutualFundDetail!.NumberOfUnits)
                    .GreaterThan(0).WithMessage("Number of units must be greater than 0");
                RuleFor(x => x.MutualFundDetail!.PurchaseNAVPerUnit)
                    .GreaterThan(0).WithMessage("Purchase NAV per unit must be greater than 0");
                RuleFor(x => x.MutualFundDetail!.CurrentNAVPerUnit)
                    .GreaterThanOrEqualTo(0).WithMessage("Current NAV per unit must be >= 0");
            });
        });

        // Crypto-specific validation
        When(x => x.AssetType == AssetType.Crypto, () =>
        {
            RuleFor(x => x.CryptoDetail)
                .NotNull().WithMessage("Crypto details are required for cryptocurrency assets");
            
            When(x => x.CryptoDetail != null, () =>
            {
                RuleFor(x => x.CryptoDetail!.Symbol)
                    .NotEmpty().WithMessage("Crypto symbol is required");
                RuleFor(x => x.CryptoDetail!.NumberOfUnits)
                    .GreaterThan(0).WithMessage("Number of units must be greater than 0");
                RuleFor(x => x.CryptoDetail!.PurchasePricePerUnitUSD)
                    .GreaterThan(0).WithMessage("Purchase price per unit (USD) must be greater than 0");
                RuleFor(x => x.CryptoDetail!.CurrentPricePerUnitUSD)
                    .GreaterThanOrEqualTo(0).WithMessage("Current price per unit (USD) must be >= 0");
                RuleFor(x => x.CryptoDetail!.UsdToEgpRate)
                    .GreaterThan(0).WithMessage("USD to EGP rate must be greater than 0");
            });
        });

        // Bonds-specific validation
        When(x => x.AssetType == AssetType.Bonds, () =>
        {
            RuleFor(x => x.BondsDetail)
                .NotNull().WithMessage("Bonds details are required for bond assets");
            
            When(x => x.BondsDetail != null, () =>
            {
                RuleFor(x => x.BondsDetail!.Issuer)
                    .NotEmpty().WithMessage("Issuer is required");
                RuleFor(x => x.BondsDetail!.FaceValueEGP)
                    .GreaterThan(0).WithMessage("Face value must be greater than 0");
                RuleFor(x => x.BondsDetail!.CouponRatePercent)
                    .GreaterThanOrEqualTo(0).WithMessage("Coupon rate must be >= 0");
                RuleFor(x => x.BondsDetail!.MaturityDate)
                    .GreaterThan(DateTime.UtcNow).WithMessage("Maturity date must be in the future");
                RuleFor(x => x.BondsDetail!.PurchasePriceEGP)
                    .GreaterThan(0).WithMessage("Purchase price must be greater than 0");
            });
        });
    }
}
