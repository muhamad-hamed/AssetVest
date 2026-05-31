using AssetVest.Application.DTOs.Assets;
using AssetVest.Domain.Entities;

namespace AssetVest.Infrastructure.Handlers.Queries.Assets;

internal static class AssetMappingExtensions
{
    public static AssetDto ToDto(this Asset asset)
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
