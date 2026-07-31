# AssetVest - Backend API

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

> **Note:** This is the **backend API** repository. The frontend application is maintained in a separate repository.

## 📖 Overview

AssetVest is a comprehensive investment portfolio management platform that enables users to track and manage diverse asset classes including stocks, currencies, gold, real estate, mutual funds, cryptocurrencies, bonds, and cash. Built with clean architecture principles and modern .NET 10 technologies.

### Key Features

- 🔐 **JWT Authentication** - Secure token-based auth with refresh token rotation and forgot/reset password flow
- 💼 **Multi-Asset Support** - Track 9 asset types: Stocks, ForeignCurrency, Gold, RealEstate, Crypto, Bonds, Cash, MutualFunds, Other
- 📊 **Asset Value Tracking** - Historical value tracking, profit/loss calculation per asset
- 🎯 **Annual Goal Management** - Set yearly portfolio targets with per-asset-type allocation goals
- 💱 **FX Rate Support** - Foreign exchange rates with history for multi-currency portfolios
- 📝 **Full Audit Logging** - Automatic audit trail (created/updated/deleted by whom, old & new values)
- 🔄 **CQRS + MediatR** - Command Query Responsibility Segregation throughout
- 🛡️ **Rate Limiting** - Auth endpoints: 5 req/min; API endpoints: 100 req/min (sliding window)
- 🔒 **Soft Delete** - All entities use soft delete with global query filters
- 🐳 **Docker Ready** - Full stack via Docker Compose (API + PostgreSQL + Seq)

## 🏗️ Architecture

### Clean Architecture Layers

```
AssetVest/
├── src/
│   ├── AssetVest.Api/              # Presentation Layer
│   │   ├── Controllers/            # AuthController, UsersController, AssetsController, AnnualGoalsController
│   │   ├── Extensions/             # CurrentUserService (ICurrentUserService)
│   │   └── Middleware/             # GlobalExceptionHandlerMiddleware
│   ├── AssetVest.Application/      # Application Layer
│   │   ├── Commands/               # CQRS write commands + validators
│   │   ├── Queries/                # CQRS read queries
│   │   ├── DTOs/                   # Request/response objects
│   │   ├── Behaviors/              # MediatR pipeline behaviors (Logging, Validation)
│   │   └── Ports/                  # ICurrentUserService, ITokenService
│   ├── AssetVest.Domain/           # Domain Layer
│   │   ├── Entities/               # User, Asset, AnnualGoal, RefreshToken, detail tables...
│   │   ├── Enums/                  # AssetType, AssetValueSource, MutualFundType
│   │   └── Common/                 # AuditableEntity base class
│   └── AssetVest.Infrastructure/   # Infrastructure Layer
│       ├── Handlers/               # MediatR command & query handlers
│       ├── Persistence/            # EF Core DbContext, configurations, migrations
│       └── Services/               # TokenService (JWT + refresh token generation)
└── tests/
    ├── AssetVest.Application.Tests/
    ├── AssetVest.Domain.Tests/
    └── AssetVest.Integration.Tests/
```

### Database Schema

18 tables across logical domains:

| Domain | Tables |
|--------|--------|
| **Authentication** | Users, RefreshTokens |
| **Assets** | Assets + 7 detail tables (StockDetail, CurrencyDetail, GoldDetail, RealEstateDetail, MutualFundDetail, CryptoDetail, BondsDetail) |
| **Tracking** | AssetValueHistory, AuditLogs |
| **Goals** | AnnualGoals, AssetTypeAllocationGoals, StockProfitGoals |
| **FX** | FxRates, FxRateHistory |

All tables extend `AuditableEntity` which provides: `CreatedAt`, `CreatedBy`, `UpdatedAt`, `UpdatedBy`, `DeletedAt`, `DeletedBy`, `IsDeleted`.

## 🚀 Tech Stack

### Core Technologies
- **.NET 10.0** - Latest .NET framework
- **ASP.NET Core** - Web API with API versioning (`v1`)
- **Entity Framework Core 10.0.8** - ORM with Code-First migrations (PostgreSQL)
- **PostgreSQL 17** - Primary database
- **MediatR** - CQRS pipeline (commands, queries, behaviors)
- **FluentValidation** - Request validation via MediatR pipeline behavior

### Security & Authentication
- **JWT (HS256)** - Access tokens, 15-minute lifetime, zero clock skew
- **BCrypt.Net** - Password hashing (register, change-password, reset-password)
- **SHA-256** - Refresh token and password-reset token hashing (plain token never stored)
- **Rate Limiting** - Fixed window (auth) + sliding window (api) via ASP.NET Core built-in

### Infrastructure
- **Docker & Docker Compose** - Full containerization
- **Serilog + Seq** - Structured logging with request logging middleware
- **Swagger / OpenAPI** - Interactive API docs (Development only)
- **Testcontainers** - Real PostgreSQL for integration tests

### Testing
- **xUnit** - Test framework
- **FluentAssertions** - Assertions
- **Testcontainers** - Integration testing with real databases

## 📦 Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)
- [Postman](https://www.postman.com/) *(optional)*

## ⚡ Quick Start

### Option A — Full Docker Stack (Recommended)

```bash
git clone https://github.com/muhamad-hamed/AssetVest.git
cd AssetVest

# Build & start API + PostgreSQL + Seq
docker compose up --build -d

# API:     http://localhost:5062
# Swagger: http://localhost:5062/swagger
# Seq:     http://localhost:5341
```

Migrations run automatically on startup via EF Core.

### Option B — Local Development

```bash
# 1. Start infrastructure only
docker compose up postgres seq -d

# 2. Apply migrations
dotnet ef database update \
  --project src/AssetVest.Infrastructure \
  --startup-project src/AssetVest.Api

# 3. Run API
dotnet run --project src/AssetVest.Api

# API available at http://localhost:5062
```

### Quick Test

```bash
# Health check
curl http://localhost:5062/health

# Register
curl -X POST http://localhost:5062/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{"firstName":"John","lastName":"Doe","email":"john@example.com","password":"SecureP@ss1"}'

# Login
curl -X POST http://localhost:5062/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"john@example.com","password":"SecureP@ss1"}'
```

## 🐳 Docker Services

| Service | Port | Description |
|---------|------|-------------|
| API | `5062` | REST API |
| Swagger | `5062/swagger` | Interactive API docs (dev only) |
| PostgreSQL | `5432` | Database |
| Seq | `5341` | Structured log viewer |

```bash
# View API logs
docker compose logs -f api

# Stop everything
docker compose down

# Stop and remove volumes
docker compose down -v
```

## 🔑 Authentication Flow

AssetVest uses **JWT Bearer + Refresh Token Rotation**:

```
POST /api/v1/auth/register        → { accessToken, refreshToken, expiresIn, user }
POST /api/v1/auth/login           → { accessToken, refreshToken, expiresIn, user }
POST /api/v1/auth/refresh         → { accessToken, refreshToken, expiresIn, user }
POST /api/v1/auth/logout          → revokes all refresh tokens for current user
POST /api/v1/auth/forgot-password → generates a reset token (returned in response for dev)
POST /api/v1/auth/reset-password  → consumes token, sets new password
```

Use the access token in all protected requests:
```
Authorization: Bearer <accessToken>
```

**Token lifetimes:**

| Token | Lifetime |
|-------|----------|
| Access Token | 15 minutes |
| Refresh Token | 7 days |
| Password Reset Token | 30 minutes |

**Forgot Password security notes:**
- Reset token stored as SHA-256 hash in DB; plain token only in memory/response
- User enumeration prevented — same generic response whether email exists or not
- Token consumed and cleared after first successful use

## 📚 API Endpoints

All endpoints are under `/api/v1/`.

### Auth — `POST /api/v1/auth/*` (anonymous)

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/auth/register` | Register new user |
| POST | `/auth/login` | Login, get tokens |
| POST | `/auth/refresh` | Refresh access token |
| POST | `/auth/logout` | Revoke all refresh tokens |
| POST | `/auth/forgot-password` | Request password reset token |
| POST | `/auth/reset-password` | Reset password with token |

### Users — `[Authorize]`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/users` | List all users |
| GET | `/users/me` | Get current authenticated user |
| GET | `/users/{id}` | Get user by ID |
| GET | `/users/by-email/{email}` | Get user by email |
| POST | `/users` | Create user |
| PUT | `/users/{id}` | Update user |
| DELETE | `/users/{id}` | Soft-delete user |
| POST | `/users/{id}/change-password` | Change password (requires current password) |
| POST | `/users/{id}/toggle-active` | Toggle user active status |

### Assets — `[Authorize]`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/assets` | List all assets (current user) |
| GET | `/assets/{id}` | Get asset by ID |
| GET | `/assets/by-type/{assetType}` | Filter assets by type |
| POST | `/assets` | Create asset |
| PUT | `/assets/{id}` | Update asset |
| DELETE | `/assets/{id}` | Soft-delete asset |

**Asset Types:** `Stocks`, `ForeignCurrency`, `Gold`, `RealEstate`, `Crypto`, `Bonds`, `Cash`, `MutualFunds`, `Other`

Each asset includes type-specific detail fields (e.g., ticker + shares for stocks, karat + weight for gold).
Asset response always includes `profitEGP` and `profitPercent` calculated fields.

### Annual Goals — `[Authorize]`

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/annual-goals` | List all goals (current user) |
| GET | `/annual-goals/{id}` | Get goal by ID |
| GET | `/annual-goals/by-year/{year}` | Get goal for a specific year |
| POST | `/annual-goals` | Create annual goal with allocation targets |
| PUT | `/annual-goals/{id}` | Update annual goal |
| DELETE | `/annual-goals/{id}` | Delete annual goal |

Annual goals support per-asset-type allocation percentage targets.

### Health

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/health` | Liveness check |
| GET | `/health/ready` | Readiness check (DB connectivity) |

## 🔧 Configuration

### Environment Variables

```bash
# Database
ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=AssetVestDb;Username=postgres;Password=postgres"

# JWT
Jwt__SecretKey="your-secret-key-minimum-32-characters"
Jwt__Issuer="AssetVest.Api"
Jwt__Audience="AssetVest.Client"
Jwt__AccessTokenExpirationMinutes="15"
Jwt__RefreshTokenExpirationDays="7"

# Logging
Seq__Url="http://localhost:5341"

# CORS (comma-separated or array in appsettings.json)
Cors__AllowedOrigins__0="http://localhost:3000"

# Rate Limiting (optional overrides)
RateLimiting__Auth__PermitLimit="5"
RateLimiting__Auth__WindowSeconds="60"
RateLimiting__Api__PermitLimit="100"
RateLimiting__Api__WindowSeconds="60"
```

### Development Credentials

| Service | Username | Password |
|---------|----------|----------|
| PostgreSQL | `postgres` | `postgres` |
| Seq | `admin` | `M#seq@2026` |

> ⚠️ **Change all credentials before production deployment.**

## 🧪 Testing

```bash
# Run all tests
dotnet test AssetVest.sln

# Run specific project
dotnet test tests/AssetVest.Integration.Tests
dotnet test tests/AssetVest.Application.Tests
dotnet test tests/AssetVest.Domain.Tests

# With coverage
dotnet test --collect:"XPlat Code Coverage"
```

Integration tests use **Testcontainers** — Docker must be running.

## 🗺️ Roadmap

- [ ] Email delivery for forgot-password (SMTP / SendGrid)
- [ ] Pagination for list endpoints
- [ ] Response caching with ETags
- [ ] FX rate auto-sync background service
- [ ] Advanced analytics and reporting
- [ ] Export to CSV / Excel
- [ ] CI/CD pipeline
- [ ] Production deployment guides
- [ ] Role-based access control (Admin / User separation)

## 🌐 Frontend Application

Frontend is developed and maintained in a **separate repository**. This repo contains only the backend API.

The frontend connects to this API using:
- Base URL: `http://localhost:5062` (development)
- JWT via `Authorization: Bearer <token>` headers
- RESTful JSON endpoints

**Frontend Repository:** *(link when available)*

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit using [Conventional Commits](https://www.conventionalcommits.org/):
   - `feat:` — new features
   - `fix:` — bug fixes
   - `docs:` — documentation
   - `refactor:` — refactoring
   - `test:` — tests
   - `chore:` — maintenance
4. Push: `git push origin feature/amazing-feature`
5. Open a Pull Request

## 📝 License

MIT License — see [LICENSE](LICENSE) for details.

## 👥 Author

**Mohamad Hamed**
- GitHub: [@muhamad-hamed](https://github.com/muhamad-hamed)
- Email: muhamaad.hamed@gmail.com

## 📞 Support

- Open an [Issue](https://github.com/muhamad-hamed/AssetVest/issues)
- Check the [docs/](docs/) folder
- Review [Swagger UI](http://localhost:5062/swagger) when running locally

---

**Built with ❤️ using .NET 10**

*Last Updated: July 31, 2026*
