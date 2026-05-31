using AssetVest.Application.Commands.Assets.CreateAsset;
using AssetVest.Application.DTOs.Assets;
using AssetVest.Application.Ports;
using AssetVest.Domain.Entities;
using AssetVest.Domain.Enums;
using AssetVest.Infrastructure.Persistence;
using MediatR;

namespace AssetVest.Infrastructure.Handlers.Commands.Assets;

public class CreateAssetCommandHandler(ApplicationDbContext context, ICurrentUserService currentUserService) 
    : IRequestHandler<CreateAssetCommand, AssetDto>
{
    public async Task<AssetDto> Handle(CreateAssetCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var asset = new Asset
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = request.Name,
            AssetType = request.AssetType,
            BaseCurrency = request.BaseCurrency,
            InitialValueEGP = request.InitialValueEGP,
            CurrentValueEGP = request.CurrentValueEGP,
            Notes = request.Notes
        };

        // Create type-specific detail entity
        switch (request.AssetType)
        {
            case AssetType.Stocks:
                asset.StockDetail = new StockDetail
                {
                    Id = Guid.NewGuid(),
                    AssetId = asset.Id,
                    StockSymbol = request.StockDetail!.StockSymbol,
                    Exchange = request.StockDetail.Exchange,
                    NumberOfUnits = request.StockDetail.NumberOfUnits,
                    PurchasePricePerUnitEGP = request.StockDetail.PurchasePricePerUnitEGP,
                    CurrentPricePerUnitEGP = request.StockDetail.CurrentPricePerUnitEGP
                };
                break;

            case AssetType.ForeignCurrency:
                asset.CurrencyDetail = new CurrencyDetail
                {
                    Id = Guid.NewGuid(),
                    AssetId = asset.Id,
                    CurrencyCode = request.CurrencyDetail!.CurrencyCode,
                    InitialAmount = request.CurrencyDetail.InitialAmount,
                    CurrentFxRateToEGP = request.CurrencyDetail.CurrentFxRateToEGP,
                    CurrentValueEGP = request.CurrencyDetail.InitialAmount * request.CurrencyDetail.CurrentFxRateToEGP
                };
                break;

            case AssetType.Gold:
                asset.GoldDetail = new GoldDetail
                {
                    Id = Guid.NewGuid(),
                    AssetId = asset.Id,
                    WeightGrams = request.GoldDetail!.WeightGrams,
                    Karat = request.GoldDetail.Karat,
                    PurchasePricePerGramEGP = request.GoldDetail.PurchasePricePerGramEGP,
                    CurrentPricePerGramEGP = request.GoldDetail.CurrentPricePerGramEGP
                };
                break;

            case AssetType.RealEstate:
                asset.RealEstateDetail = new RealEstateDetail
                {
                    Id = Guid.NewGuid(),
                    AssetId = asset.Id,
                    Description = request.RealEstateDetail!.Description,
                    Location = request.RealEstateDetail.Location,
                    AreaSqm = request.RealEstateDetail.AreaSqm,
                    PurchaseValueEGP = request.RealEstateDetail.PurchaseValueEGP,
                    CurrentEstimatedValueEGP = request.RealEstateDetail.CurrentEstimatedValueEGP
                };
                break;

            case AssetType.MutualFunds:
                asset.MutualFundDetail = new MutualFundDetail
                {
                    Id = Guid.NewGuid(),
                    AssetId = asset.Id,
                    FundName = request.MutualFundDetail!.FundName,
                    ManagementCompany = request.MutualFundDetail.ManagementCompany,
                    FundType = request.MutualFundDetail.FundType,
                    NumberOfUnits = request.MutualFundDetail.NumberOfUnits,
                    PurchaseNAVPerUnit = request.MutualFundDetail.PurchaseNAVPerUnit,
                    CurrentNAVPerUnit = request.MutualFundDetail.CurrentNAVPerUnit
                };
                break;

            case AssetType.Crypto:
                asset.CryptoDetail = new CryptoDetail
                {
                    Id = Guid.NewGuid(),
                    AssetId = asset.Id,
                    Symbol = request.CryptoDetail!.Symbol,
                    NumberOfUnits = request.CryptoDetail.NumberOfUnits,
                    PurchasePricePerUnitUSD = request.CryptoDetail.PurchasePricePerUnitUSD,
                    CurrentPricePerUnitUSD = request.CryptoDetail.CurrentPricePerUnitUSD,
                    UsdToEgpRate = request.CryptoDetail.UsdToEgpRate
                };
                break;

            case AssetType.Bonds:
                asset.BondsDetail = new BondsDetail
                {
                    Id = Guid.NewGuid(),
                    AssetId = asset.Id,
                    Issuer = request.BondsDetail!.Issuer,
                    FaceValueEGP = request.BondsDetail.FaceValueEGP,
                    CouponRatePercent = request.BondsDetail.CouponRatePercent,
                    MaturityDate = request.BondsDetail.MaturityDate,
                    PurchasePriceEGP = request.BondsDetail.PurchasePriceEGP
                };
                break;
        }

        asset.RecalculateProfit();

        context.Assets.Add(asset);
        await context.SaveChangesAsync(cancellationToken);

        return MapToDto(asset);
    }

    private static AssetDto MapToDto(Asset asset)
    {
        var dto = new AssetDto
        {
            Id = asset.Id,
            UserId = asset.UserId,
            Name = asset.Name,
            AssetType = asset.AssetType,
            BaseCurrency = asset.BaseCurrency,
            InitialValueEGP = asset.InitialValueEGP,
            CurrentValueEGP = asset.CurrentValueEGP,
            ProfitEGP = asset.ProfitEGP,
            ProfitPercent = asset.ProfitPercent,
            Notes = asset.Notes,
            CreatedAt = asset.CreatedAt,
            UpdatedAt = asset.UpdatedAt
        };

        // Map type-specific details
        if (asset.StockDetail is not null)
        {
            dto = dto with
            {
                StockDetail = new StockDetailDto
                {
                    StockSymbol = asset.StockDetail.StockSymbol,
                    Exchange = asset.StockDetail.Exchange,
                    NumberOfUnits = asset.StockDetail.NumberOfUnits,
                    PurchasePricePerUnitEGP = asset.StockDetail.PurchasePricePerUnitEGP,
                    CurrentPricePerUnitEGP = asset.StockDetail.CurrentPricePerUnitEGP
                }
            };
        }
        else if (asset.CurrencyDetail is not null)
        {
            dto = dto with
            {
                CurrencyDetail = new CurrencyDetailDto
                {
                    CurrencyCode = asset.CurrencyDetail.CurrencyCode,
                    InitialAmount = asset.CurrencyDetail.InitialAmount,
                    CurrentFxRateToEGP = asset.CurrencyDetail.CurrentFxRateToEGP,
                    CurrentValueEGP = asset.CurrencyDetail.CurrentValueEGP
                }
            };
        }
        else if (asset.GoldDetail is not null)
        {
            dto = dto with
            {
                GoldDetail = new GoldDetailDto
                {
                    WeightGrams = asset.GoldDetail.WeightGrams,
                    Karat = asset.GoldDetail.Karat,
                    PurchasePricePerGramEGP = asset.GoldDetail.PurchasePricePerGramEGP,
                    CurrentPricePerGramEGP = asset.GoldDetail.CurrentPricePerGramEGP
                }
            };
        }
        else if (asset.RealEstateDetail is not null)
        {
            dto = dto with
            {
                RealEstateDetail = new RealEstateDetailDto
                {
                    Description = asset.RealEstateDetail.Description,
                    Location = asset.RealEstateDetail.Location,
                    AreaSqm = asset.RealEstateDetail.AreaSqm,
                    PurchaseValueEGP = asset.RealEstateDetail.PurchaseValueEGP,
                    CurrentEstimatedValueEGP = asset.RealEstateDetail.CurrentEstimatedValueEGP
                }
            };
        }
        else if (asset.MutualFundDetail is not null)
        {
            dto = dto with
            {
                MutualFundDetail = new MutualFundDetailDto
                {
                    FundName = asset.MutualFundDetail.FundName,
                    ManagementCompany = asset.MutualFundDetail.ManagementCompany,
                    FundType = asset.MutualFundDetail.FundType,
                    NumberOfUnits = asset.MutualFundDetail.NumberOfUnits,
                    PurchaseNAVPerUnit = asset.MutualFundDetail.PurchaseNAVPerUnit,
                    CurrentNAVPerUnit = asset.MutualFundDetail.CurrentNAVPerUnit
                }
            };
        }
        else if (asset.CryptoDetail is not null)
        {
            dto = dto with
            {
                CryptoDetail = new CryptoDetailDto
                {
                    Symbol = asset.CryptoDetail.Symbol,
                    NumberOfUnits = asset.CryptoDetail.NumberOfUnits,
                    PurchasePricePerUnitUSD = asset.CryptoDetail.PurchasePricePerUnitUSD,
                    CurrentPricePerUnitUSD = asset.CryptoDetail.CurrentPricePerUnitUSD,
                    UsdToEgpRate = asset.CryptoDetail.UsdToEgpRate
                }
            };
        }
        else if (asset.BondsDetail is not null)
        {
            dto = dto with
            {
                BondsDetail = new BondsDetailDto
                {
                    Issuer = asset.BondsDetail.Issuer,
                    FaceValueEGP = asset.BondsDetail.FaceValueEGP,
                    CouponRatePercent = asset.BondsDetail.CouponRatePercent,
                    MaturityDate = asset.BondsDetail.MaturityDate,
                    PurchasePriceEGP = asset.BondsDetail.PurchasePriceEGP
                }
            };
        }

        return dto;
    }
}
