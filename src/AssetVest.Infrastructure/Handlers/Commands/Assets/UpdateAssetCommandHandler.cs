using AssetVest.Application.Commands.Assets.UpdateAsset;
using AssetVest.Application.DTOs.Assets;
using AssetVest.Application.Ports;
using AssetVest.Domain.Entities;
using AssetVest.Domain.Enums;
using AssetVest.Infrastructure.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AssetVest.Infrastructure.Handlers.Commands.Assets;

public class UpdateAssetCommandHandler(ApplicationDbContext context, ICurrentUserService currentUserService) 
    : IRequestHandler<UpdateAssetCommand, AssetDto?>
{
    public async Task<AssetDto?> Handle(UpdateAssetCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUserService.UserId ?? throw new UnauthorizedAccessException("User not authenticated");

        var asset = await context.Assets
            .Include(a => a.StockDetail)
            .Include(a => a.CurrencyDetail)
            .Include(a => a.GoldDetail)
            .Include(a => a.RealEstateDetail)
            .Include(a => a.MutualFundDetail)
            .Include(a => a.CryptoDetail)
            .Include(a => a.BondsDetail)
            .FirstOrDefaultAsync(a => a.Id == request.AssetId, cancellationToken);

        if (asset is null || asset.UserId != userId)
            return null;

        asset.Name = request.Name;
        asset.AssetType = request.AssetType;
        asset.BaseCurrency = request.BaseCurrency;
        asset.CurrentValueEGP = request.CurrentValueEGP;
        asset.Notes = request.Notes;

        // Update type-specific detail entity
        switch (request.AssetType)
        {
            case AssetType.Stocks:
                if (asset.StockDetail is null)
                {
                    asset.StockDetail = new StockDetail { Id = Guid.NewGuid(), AssetId = asset.Id };
                }
                asset.StockDetail.StockSymbol = request.StockDetail!.StockSymbol;
                asset.StockDetail.Exchange = request.StockDetail.Exchange;
                asset.StockDetail.NumberOfUnits = request.StockDetail.NumberOfUnits;
                asset.StockDetail.PurchasePricePerUnitEGP = request.StockDetail.PurchasePricePerUnitEGP;
                asset.StockDetail.CurrentPricePerUnitEGP = request.StockDetail.CurrentPricePerUnitEGP;
                break;

            case AssetType.ForeignCurrency:
                if (asset.CurrencyDetail is null)
                {
                    asset.CurrencyDetail = new CurrencyDetail { Id = Guid.NewGuid(), AssetId = asset.Id };
                }
                asset.CurrencyDetail.CurrencyCode = request.CurrencyDetail!.CurrencyCode;
                asset.CurrencyDetail.InitialAmount = request.CurrencyDetail.InitialAmount;
                asset.CurrencyDetail.CurrentFxRateToEGP = request.CurrencyDetail.CurrentFxRateToEGP;
                asset.CurrencyDetail.CurrentValueEGP = request.CurrencyDetail.InitialAmount * request.CurrencyDetail.CurrentFxRateToEGP;
                break;

            case AssetType.Gold:
                if (asset.GoldDetail is null)
                {
                    asset.GoldDetail = new GoldDetail { Id = Guid.NewGuid(), AssetId = asset.Id };
                }
                asset.GoldDetail.WeightGrams = request.GoldDetail!.WeightGrams;
                asset.GoldDetail.Karat = request.GoldDetail.Karat;
                asset.GoldDetail.PurchasePricePerGramEGP = request.GoldDetail.PurchasePricePerGramEGP;
                asset.GoldDetail.CurrentPricePerGramEGP = request.GoldDetail.CurrentPricePerGramEGP;
                break;

            case AssetType.RealEstate:
                if (asset.RealEstateDetail is null)
                {
                    asset.RealEstateDetail = new RealEstateDetail { Id = Guid.NewGuid(), AssetId = asset.Id };
                }
                asset.RealEstateDetail.Description = request.RealEstateDetail!.Description;
                asset.RealEstateDetail.Location = request.RealEstateDetail.Location;
                asset.RealEstateDetail.AreaSqm = request.RealEstateDetail.AreaSqm;
                asset.RealEstateDetail.PurchaseValueEGP = request.RealEstateDetail.PurchaseValueEGP;
                asset.RealEstateDetail.CurrentEstimatedValueEGP = request.RealEstateDetail.CurrentEstimatedValueEGP;
                break;

            case AssetType.MutualFunds:
                if (asset.MutualFundDetail is null)
                {
                    asset.MutualFundDetail = new MutualFundDetail { Id = Guid.NewGuid(), AssetId = asset.Id };
                }
                asset.MutualFundDetail.FundName = request.MutualFundDetail!.FundName;
                asset.MutualFundDetail.ManagementCompany = request.MutualFundDetail.ManagementCompany;
                asset.MutualFundDetail.FundType = request.MutualFundDetail.FundType;
                asset.MutualFundDetail.NumberOfUnits = request.MutualFundDetail.NumberOfUnits;
                asset.MutualFundDetail.PurchaseNAVPerUnit = request.MutualFundDetail.PurchaseNAVPerUnit;
                asset.MutualFundDetail.CurrentNAVPerUnit = request.MutualFundDetail.CurrentNAVPerUnit;
                break;

            case AssetType.Crypto:
                if (asset.CryptoDetail is null)
                {
                    asset.CryptoDetail = new CryptoDetail { Id = Guid.NewGuid(), AssetId = asset.Id };
                }
                asset.CryptoDetail.Symbol = request.CryptoDetail!.Symbol;
                asset.CryptoDetail.NumberOfUnits = request.CryptoDetail.NumberOfUnits;
                asset.CryptoDetail.PurchasePricePerUnitUSD = request.CryptoDetail.PurchasePricePerUnitUSD;
                asset.CryptoDetail.CurrentPricePerUnitUSD = request.CryptoDetail.CurrentPricePerUnitUSD;
                asset.CryptoDetail.UsdToEgpRate = request.CryptoDetail.UsdToEgpRate;
                break;

            case AssetType.Bonds:
                if (asset.BondsDetail is null)
                {
                    asset.BondsDetail = new BondsDetail { Id = Guid.NewGuid(), AssetId = asset.Id };
                }
                asset.BondsDetail.Issuer = request.BondsDetail!.Issuer;
                asset.BondsDetail.FaceValueEGP = request.BondsDetail.FaceValueEGP;
                asset.BondsDetail.CouponRatePercent = request.BondsDetail.CouponRatePercent;
                asset.BondsDetail.MaturityDate = request.BondsDetail.MaturityDate;
                asset.BondsDetail.PurchasePriceEGP = request.BondsDetail.PurchasePriceEGP;
                break;
        }

        asset.RecalculateProfit();
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
