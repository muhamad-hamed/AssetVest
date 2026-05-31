# AssetVest — Project Context & Business Information

**Last Updated:** 2026-05-31  
**Status:** In Development

---

## 1. Project Overview

AssetVest is a personal investment manager and trade tracking system. It tracks multiple asset types, calculates profit/loss in Egyptian Pounds (EGP), supports multi-currency assets with FX rate conversion, and provides annual goal tracking by asset type.

**Scope:** Single user (owner only). Not a SaaS product.  
**Default currency:** Egyptian Pounds (EGP)

---

## 2. Technology Stack

| Layer | Technology |
|-------|-----------|
| Backend | C# / .NET 10 |
| Architecture | Clean Architecture (4 layers) |
| ORM | Entity Framework Core + Npgsql |
| Database | PostgreSQL |
| CQRS | MediatR (full mediator pattern) |
| Validation | FluentValidation |
| Auth | JWT self-issued (HS256) |
| Logging | Serilog (Console + File + Seq) |
| API Versioning | URL segment (`/api/v1/`) |
| API Docs | Swashbuckle / Swagger |
| Testing | xUnit + FluentAssertions + NSubstitute + Testcontainers |

---

## 3. Solution Structure

```
AssetVest/
├── src/
│   ├── AssetVest.Domain/          # Entities, enums, value objects
│   ├── AssetVest.Application/     # Use cases, MediatR handlers, ports, DTOs
│   ├── AssetVest.Infrastructure/  # EF Core, PostgreSQL, JWT, Serilog
│   └── AssetVest.Api/             # Controllers, middleware, Program.cs
├── tests/
│   ├── AssetVest.Domain.Tests/
│   ├── AssetVest.Application.Tests/
│   └── AssetVest.Integration.Tests/
└── docs/
    ├── adr/                       # Architecture Decision Records
    ├── architecture/              # This file and C4 diagrams
    ├── api/                       # OpenAPI contracts
    └── runbooks/                  # Operational guides
```

### Dependency Rule (strictly enforced)
```
Domain ← Application ← Infrastructure
                     ← Api (composition root)
```

---

## 4. Domain Model

### 4.1 Identity

**User**
- `Id`, `FirstName`, `LastName`, `Email`, `PasswordHash`, `IsActive`
- Full audit columns (see section 5)

**RefreshToken**
- `Id`, `UserId`, `TokenHash`, `ExpiresAt`, `RevokedAt`, `ReplacedByTokenId`
- `IsActive` — computed: `RevokedAt == null && UtcNow < ExpiresAt`
- Max 5 active tokens per user (multi-device)

---

### 4.2 Portfolio

**Asset** (core entity)
- `Id`, `UserId`, `Name`, `AssetType`, `BaseCurrency` (default: EGP)
- `InitialValueEGP` — locked at creation
- `CurrentValueEGP` — updated on each valuation
- `ProfitEGP` — computed: `CurrentValueEGP - InitialValueEGP`
- `ProfitPercent` — computed: `ProfitEGP / InitialValueEGP * 100`
- `Notes`
- `RecalculateProfit()` domain method

**AssetType enum**
```
Stocks | ForeignCurrency | Gold | RealEstate | Crypto | Bonds | Cash | MutualFunds | Other
```

**Per-type detail tables (one-to-one with Asset):**

| Detail Table | Key Fields |
|-------------|-----------|
| `StockDetail` | Symbol, Exchange, NumberOfUnits, PurchasePricePerUnitEGP, CurrentPricePerUnitEGP |
| `CurrencyDetail` | CurrencyCode, InitialAmount, CurrentFxRateToEGP, CurrentValueEGP |
| `GoldDetail` | WeightGrams, Karat, PurchasePricePerGramEGP, CurrentPricePerGramEGP |
| `RealEstateDetail` | Description, Location, AreaSqm, PurchaseValueEGP, CurrentEstimatedValueEGP |
| `MutualFundDetail` | FundName, ManagementCompany, FundType, NumberOfUnits, PurchaseNAVPerUnit, CurrentNAVPerUnit |
| `CryptoDetail` | Symbol, NumberOfUnits, PurchasePricePerUnitUSD, CurrentPricePerUnitUSD, UsdToEgpRate |
| `BondsDetail` | Issuer, FaceValueEGP, CouponRatePercent, MaturityDate, PurchasePriceEGP |

**MutualFundType enum**
```
FixedIncome | Stocks | Gold | RealEstate | Mixed
```

**AssetValueHistory** (snapshot on every valuation update)
- `AssetId`, `ValueEGP`, `ProfitEGP`, `ProfitPercent`, `RecordedAt`
- `Source` enum: `Manual | FxUpdate | StockUpdate | System`

---

### 4.3 Market

**FxRate** (current live rate)
- `FromCurrency` (ISO code), `ToCurrency` (always EGP), `Rate`, `Source`, `FetchedAt`
- Unique index on `(FromCurrency, ToCurrency)`

**FxRateHistory** (all historical snapshots)
- `FromCurrency`, `ToCurrency`, `Rate`, `Source`, `RecordedAt`

> **Open question:** Auto-fetch FX rates from external API or manual entry only?  
> Decision pending — will affect Infrastructure background service design.

> **Open question:** Auto-fetch stock prices from EGX or manual entry?  
> Decision pending.

---

### 4.4 Goals

**AnnualGoal** (set at start of each year)
- `UserId`, `Year`, `TargetTotalPortfolioValueEGP`, `TargetProfitPercent`
- Unique per `(UserId, Year)`

**AssetTypeAllocationGoal** (% allocation per asset type within AnnualGoal)
- `AnnualGoalId`, `AssetType`, `TargetAllocationPercent`
- E.g. 40% Stocks, 30% Gold, 20% Bonds, 10% Cash
- Unique per `(AnnualGoalId, AssetType)`

**StockProfitGoal** (profit target per individual stock asset)
- `UserId`, `AssetId`, `Year`, `TargetProfitPercent`, `TargetProfitAmountEGP`
- Unique per `(AssetId, Year)`

---

## 5. Audit & Soft Delete

All entities inheriting `AuditableEntity` have:

| Column | Type | Purpose |
|--------|------|---------|
| `CreatedAt` | DateTime (UTC) | Set on insert |
| `CreatedBy` | Guid? | UserId from JWT |
| `UpdatedAt` | DateTime? (UTC) | Set on update |
| `UpdatedBy` | Guid? | UserId from JWT |
| `DeletedAt` | DateTime? (UTC) | Set on soft delete |
| `DeletedBy` | Guid? | UserId who deleted |
| `IsDeleted` | bool | Global query filter active |

**Soft delete** enforced via EF Core global query filter — no hard deletes in application code.

**AuditLog table** — captures every entity change:
- `EntityName`, `EntityId`, `Action` (Created/Updated/Deleted)
- `OldValues` (jsonb), `NewValues` (jsonb)
- `UserId`, `IpAddress`, `Timestamp`

---

## 6. Authentication

- JWT self-issued, algorithm HS256
- Access token: 15 minutes
- Refresh token: 7 days, stored hashed in DB
- Rotation on each refresh (old token revoked, new issued)
- Claims: `sub` (UserId), `email`, `jti`, `iat`, `exp`
- JWT signing key stored in environment variable / user secrets — never in `appsettings.json`

---

## 7. ADR Summary

| ADR | Decision |
|-----|---------|
| ADR-0001 | PostgreSQL as database |
| ADR-0002 | Single-tenant (personal tool, not SaaS) |
| ADR-0003 | Full CQRS + MediatR with pipeline behaviors |
| ADR-0004 | URL segment API versioning (`/api/v1/`) |
| ADR-0005 | JWT self-issued authentication |
| ADR-0006 | Serilog + Seq (dev) + MediatR behavior + EF Core SaveChanges audit |

Full ADR documents: `docs/adr/`

---

## 8. Open Questions (Pending Decisions)

| # | Question | Impact |
|---|---------|--------|
| OQ-1 | FX rate source — external API auto-fetch or manual? | Infrastructure background job design |
| OQ-2 | Stock price source — EGX API or manual? | Infrastructure background job design |
| OQ-3 | Goal progress tracking — periodic snapshot or compute on-the-fly? | Query design |

---

## 9. MediatR Pipeline Behaviors (planned)

Execution order per request:
1. `LoggingBehavior` — log command/query name + execution time
2. `ValidationBehavior` — FluentValidation before handler
3. `AuditBehavior` — record command intent (commands only)

---

## 10. Database Conventions

- Table names: `snake_case`
- Financial decimals: `precision(18, 4)`
- FX rates: `precision(18, 6)` (extra precision for conversion)
- Crypto units: `precision(18, 8)`
- All timestamps: UTC
- All IDs: `Guid` (UUID v4)
