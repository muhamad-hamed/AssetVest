# AssetVest — Domain Model

**Last Updated:** 2026-05-30  
**Status:** In Progress — open questions noted

---

## Bounded Contexts

```
┌─ Identity ──────────┐   ┌─ Portfolio ─────────────────────────┐   ┌─ Market ────────────┐
│  User               │   │  Asset (+ detail tables per type)   │   │  FxRate             │
│  RefreshToken       │   │  AssetValueHistory                  │   │  FxRateHistory      │
└─────────────────────┘   │  StockDetail                        │   └─────────────────────┘
                          │  CurrencyDetail                     │
                          │  GoldDetail                         │   ┌─ Goals ─────────────┐
                          │  RealEstateDetail                   │   │  AnnualGoal         │
                          │  MutualFundDetail                   │   │  AssetTypeAlloc...  │
                          │  BondsDetail                        │   │  StockProfitGoal    │
                          │  CryptoDetail                       │   └─────────────────────┘
                          └──────────────────────────────────── ┘
```

---

## Base Class: AuditableEntity

All entities inherit from this base class.

| Field | Type | Notes |
|-------|------|-------|
| CreatedAt | DateTime | UTC, set on insert |
| CreatedBy | Guid? | UserId from JWT; null = self-register |
| UpdatedAt | DateTime? | UTC, set on update |
| UpdatedBy | Guid? | UserId |
| DeletedAt | DateTime? | UTC, soft delete timestamp |
| DeletedBy | Guid? | UserId who soft-deleted |
| IsDeleted | bool | Default false; EF global query filter applied |

---

## Identity Context

### `User`

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| FirstName | string | |
| LastName | string | |
| Email | string | Unique index |
| PasswordHash | string | bcrypt |
| IsActive | bool | Account enabled flag |
| + AuditableEntity | | |

### `RefreshToken`

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| UserId | Guid | FK → User |
| TokenHash | string | SHA-256 hashed token |
| ExpiresAt | DateTime | |
| RevokedAt | DateTime? | null = active |
| ReplacedByTokenId | Guid? | token rotation chain |
| CreatedAt | DateTime | |

---

## Portfolio Context

### `Asset` (root aggregate)

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| UserId | Guid | FK → User |
| Name | string | Display name |
| AssetType | enum | See AssetType below |
| BaseCurrency | string | ISO code: EGP, SAR, USD… |
| InitialValueEGP | decimal | Locked at creation |
| CurrentValueEGP | decimal | Updated on each valuation |
| ProfitEGP | decimal | Computed: current − initial |
| ProfitPercent | decimal | Computed: profit / initial × 100 |
| Notes | string? | |
| + AuditableEntity | | |

### `AssetType` Enum

```
Stocks
ForeignCurrency
Gold
RealEstate
Crypto
Bonds
Cash
MutualFunds
Other
```

### `StockDetail`

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| AssetId | Guid | FK → Asset (1:1) |
| StockSymbol | string | e.g. COMI.CA, HRHO.CA |
| Exchange | string? | e.g. EGX, NYSE |
| NumberOfUnits | decimal | |
| PurchasePricePerUnitEGP | decimal | Average cost basis |
| CurrentPricePerUnitEGP | decimal | Latest price |

### `CurrencyDetail`

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| AssetId | Guid | FK → Asset (1:1) |
| CurrencyCode | string | ISO: SAR, USD, EUR |
| InitialAmount | decimal | Amount in foreign currency |
| CurrentFxRateToEGP | decimal | Latest FX rate |
| CurrentValueEGP | decimal | InitialAmount × CurrentFxRateToEGP |

### `GoldDetail`

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| AssetId | Guid | FK → Asset (1:1) |
| WeightGrams | decimal | |
| Karat | int | 18, 21, or 24 |
| PurchasePricePerGramEGP | decimal | |
| CurrentPricePerGramEGP | decimal | |

### `RealEstateDetail`

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| AssetId | Guid | FK → Asset (1:1) |
| Description | string | |
| Location | string? | |
| AreaSqm | decimal? | |
| PurchaseValueEGP | decimal | |
| CurrentEstimatedValueEGP | decimal | Manual update |

### `MutualFundDetail`

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| AssetId | Guid | FK → Asset (1:1) |
| FundName | string | |
| ManagementCompany | string? | |
| FundType | enum | FixedIncome / Stocks / Gold / RealEstate / Mixed |
| NumberOfUnits | decimal | |
| PurchaseNAVPerUnit | decimal | Net Asset Value at purchase |
| CurrentNAVPerUnit | decimal | Latest NAV |

### `CryptoDetail`

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| AssetId | Guid | FK → Asset (1:1) |
| Symbol | string | e.g. BTC, ETH |
| NumberOfUnits | decimal | |
| PurchasePricePerUnitUSD | decimal | |
| CurrentPricePerUnitUSD | decimal | |
| UsdToEgpRate | decimal | FX rate used for EGP conversion |

### `BondsDetail`

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| AssetId | Guid | FK → Asset (1:1) |
| Issuer | string | e.g. Government, Corporate |
| FaceValueEGP | decimal | |
| CouponRatePercent | decimal | Annual interest % |
| MaturityDate | DateTime | |
| PurchasePriceEGP | decimal | |

### `AssetValueHistory`

Snapshot created every time `Asset.CurrentValueEGP` changes.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| AssetId | Guid | FK → Asset |
| ValueEGP | decimal | |
| ProfitEGP | decimal | |
| ProfitPercent | decimal | |
| RecordedAt | DateTime | UTC |
| Source | enum | Manual / FxUpdate / StockUpdate / System |
| Notes | string? | |

---

## Market Context

### `FxRate` (current live rate)

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| FromCurrency | string | ISO: SAR, USD, EUR |
| ToCurrency | string | Always EGP |
| Rate | decimal | 1 FROM = X EGP |
| Source | string | "API" or "Manual" |
| FetchedAt | DateTime | UTC |

### `FxRateHistory`

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| FromCurrency | string | |
| ToCurrency | string | EGP |
| Rate | decimal | |
| Source | string | |
| RecordedAt | DateTime | UTC |

---

## Goals Context

### `AnnualGoal`

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| UserId | Guid | FK → User |
| Year | int | e.g. 2026 |
| TargetTotalPortfolioValueEGP | decimal | Total portfolio target |
| TargetProfitPercent | decimal? | Overall portfolio profit % target |
| Notes | string? | |
| + AuditableEntity | | |

### `AssetTypeAllocationGoal`

Target % allocation per asset type within an annual goal.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| AnnualGoalId | Guid | FK → AnnualGoal |
| AssetType | enum | AssetType value |
| TargetAllocationPercent | decimal | e.g. 40.0 = 40% |

> Sum of all TargetAllocationPercent per AnnualGoal should equal 100. Enforce in domain or application layer.

### `StockProfitGoal`

Per-stock profit target, set annually.

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | PK |
| UserId | Guid | FK → User |
| AssetId | Guid | FK → Asset (must be Stocks type) |
| Year | int | |
| TargetProfitPercent | decimal | e.g. 25.0 = 25% gain |
| TargetProfitAmountEGP | decimal? | Optional fixed amount target |
| + AuditableEntity | | |

---

## Open Questions

| # | Question | Impact |
|---|----------|--------|
| OQ-1 | FX rates — auto-fetch from external API or manual entry? | Affects Infrastructure (background job / hosted service) |
| OQ-2 | Stock price updates — manual only or auto-fetch (EGX API)? | Affects Infrastructure layer |
| OQ-3 | Goal progress — snapshot periodically or compute on query? | Affects query design and history tables |
| OQ-4 | Crypto asset — treat like stocks (units + price) or separate FX path? | CryptoDetail design above assumes USD-based price |

---

## Value Objects (candidates)

- `Money(amount: decimal, currency: string)` — wrap monetary values
- `Percentage(value: decimal)` — enforce 0–100 range
- `CurrencyCode(code: string)` — ISO 4217 validation
