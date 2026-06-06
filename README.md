# AssetVest - Backend API

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-17-4169E1?logo=postgresql&logoColor=white)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker&logoColor=white)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

> **Note:** This is the **backend API** repository. The frontend application is maintained in a separate repository.

## 📖 Overview

AssetVest is a comprehensive asset management platform that enables users to track and manage diverse investment portfolios including stocks, currencies, gold, real estate, mutual funds, cryptocurrencies, and bonds. Built with clean architecture principles and modern .NET technologies.

### Key Features

- 🔐 **JWT Authentication** - Secure token-based authentication with refresh token rotation
- 💼 **Multi-Asset Support** - Track stocks, currencies, gold, real estate, mutual funds, crypto, and bonds
- 📊 **Asset Value Tracking** - Historical value tracking and performance analytics
- 🎯 **Goal Management** - Set and track annual investment goals with asset type allocations
- 💱 **FX Rate Integration** - Foreign exchange rates for multi-currency portfolios
- 📝 **Audit Logging** - Complete audit trail for all operations
- 🔄 **CQRS Pattern** - Command Query Responsibility Segregation for scalability
- 🐳 **Docker Ready** - Containerized deployment with Docker Compose

## 🏗️ Architecture

### Clean Architecture Layers

```
AssetVest/
├── src/
│   ├── AssetVest.Api/              # Presentation Layer (REST API, Controllers, Middleware)
│   ├── AssetVest.Application/      # Application Layer (CQRS, Handlers, DTOs, Validators)
│   ├── AssetVest.Domain/           # Domain Layer (Entities, Value Objects, Interfaces)
│   └── AssetVest.Infrastructure/   # Infrastructure Layer (EF Core, Repositories, Services)
└── tests/
    ├── AssetVest.Application.Tests/
    ├── AssetVest.Domain.Tests/
    └── AssetVest.Integration.Tests/
```

### Database Schema

17 tables organized into logical domains:
- **Authentication**: Users, RefreshTokens
- **Assets**: Assets (polymorphic) with 6 detail tables (Stock, Currency, Gold, RealEstate, MutualFund, Crypto, Bonds)
- **Tracking**: AssetValueHistory, AuditLogs
- **Goals**: AnnualGoals, AssetTypeAllocationGoals, StockProfitGoals
- **FX**: FxRates, FxRateHistory

## 🚀 Tech Stack

### Core Technologies
- **.NET 10.0** - Latest .NET framework
- **ASP.NET Core** - Web API framework with API versioning
- **Entity Framework Core 10.0.8** - ORM with Code-First migrations
- **PostgreSQL 17** - Primary database
- **FluentValidation** - Request validation
- **MediatR** - CQRS implementation

### Security & Authentication
- **JWT (HS256)** - JSON Web Tokens
- **BCrypt** - Password hashing
- **SHA256** - Refresh token hashing

### Infrastructure
- **Docker & Docker Compose** - Containerization
- **Seq** - Structured logging and monitoring
- **Swagger/OpenAPI** - API documentation

### Testing
- **xUnit** - Testing framework
- **FluentAssertions** - Assertion library
- **Testcontainers** - Integration testing with real databases

## 📦 Prerequisites

- [.NET SDK 10.0+](https://dotnet.microsoft.com/download)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)
- [Postman](https://www.postman.com/) (optional, for API testing)

## ⚡ Quick Start

### 1. Clone the Repository

```bash
git clone https://github.com/muhamad-hamed/AssetVest.git
cd AssetVest
```

### 2. Start Infrastructure

```bash
# Start PostgreSQL and Seq using Docker Compose
docker-compose up -d

# Verify services are running
docker-compose ps
```

### 3. Run Database Migrations

```bash
dotnet ef database update \
  --project src/AssetVest.Infrastructure \
  --startup-project src/AssetVest.Api
```

### 4. Start the API

```bash
# Run the API
dotnet run --project src/AssetVest.Api

# API will be available at http://localhost:5062
```

### 5. Test the API

Import the Postman collection from `docs/AssetVest-API.postman_collection.json` or use curl:

```bash
# Health check
curl http://localhost:5062/health

# Register a new user
curl -X POST http://localhost:5062/api/v1/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "firstName": "John",
    "lastName": "Doe",
    "email": "john.doe@example.com",
    "password": "SecureP@ssw0rd"
  }'
```

## 🐳 Docker Deployment

### Full Stack with Docker

```bash
# Build and start all services (API + PostgreSQL + Seq)
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop services
docker-compose down
```

### Services & Ports

| Service | Port | Description |
|---------|------|-------------|
| API | 5062 | REST API endpoints |
| PostgreSQL | 5432 | Database |
| Seq | 5341 | Structured logging UI |
| Swagger | 5062/swagger | API documentation |

## 📚 Documentation

Comprehensive documentation is available in the `docs/` folder:

- **[QUICK_START.md](docs/QUICK_START.md)** - Fast setup guide with common commands
- **[SETUP_SUMMARY.md](docs/SETUP_SUMMARY.md)** - Complete project overview and status
- **[DATABASE.md](docs/DATABASE.md)** - Database schema, migrations, and troubleshooting
- **[DOCKER.md](docs/DOCKER.md)** - Docker configuration and deployment
- **[POSTMAN.md](docs/POSTMAN.md)** - Postman collection usage guide
- **[USER_CONTROLLERS.md](docs/USER_CONTROLLERS.md)** - User management API reference

### Architecture Documentation

- **[Architecture Diagrams](docs/architecture/)** - System architecture and design patterns
- **[ADRs](docs/adr/)** - Architecture Decision Records
- **[API Specifications](docs/api/)** - Detailed API contracts
- **[Runbooks](docs/runbooks/)** - Operational procedures

## 🔑 API Authentication

AssetVest uses JWT-based authentication:

1. **Register** a new user: `POST /api/v1/auth/register`
2. **Login** to get tokens: `POST /api/v1/auth/login`
3. Use the **access token** in the `Authorization: Bearer <token>` header
4. **Refresh** expired tokens: `POST /api/v1/auth/refresh`

**Token Lifetimes:**
- Access Token: 15 minutes
- Refresh Token: 7 days

## 🧪 Testing

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/AssetVest.Integration.Tests

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📊 API Endpoints Overview

### Authentication
- `POST /api/v1/auth/register` - Register new user
- `POST /api/v1/auth/login` - User login
- `POST /api/v1/auth/refresh` - Refresh access token

### Users
- `GET /api/v1/users/me` - Get current user profile
- `PUT /api/v1/users/me` - Update profile
- `DELETE /api/v1/users/me` - Delete account

### Assets
- `GET /api/v1/assets` - List all assets
- `GET /api/v1/assets/{id}` - Get asset details
- `POST /api/v1/assets` - Create asset
- `PUT /api/v1/assets/{id}` - Update asset
- `DELETE /api/v1/assets/{id}` - Delete asset

**Supported Asset Types:**
- Stock
- Currency
- Gold
- Real Estate
- Mutual Fund
- Cryptocurrency
- Bonds

*For complete API reference, see [Swagger UI](http://localhost:5062/swagger) when running locally.*

## 🔧 Configuration

### Environment Variables

Key configuration options (set in `appsettings.json` or environment variables):

```bash
# Database
ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=AssetVestDb;Username=postgres;Password=postgres"

# JWT
Jwt__SecretKey="your-secret-key-min-32-chars"
Jwt__Issuer="AssetVest.Api"
Jwt__Audience="AssetVest.Client"
Jwt__AccessTokenExpirationMinutes="15"
Jwt__RefreshTokenExpirationDays="7"

# Logging
Seq__Url="http://localhost:5341"
```

### Development Credentials

| Service | Username | Password |
|---------|----------|----------|
| PostgreSQL | postgres | postgres |
| Seq | admin | M#seq@2026 |

⚠️ **Change these in production!**

## 🌐 Frontend Application

The frontend for AssetVest is developed and maintained in a **separate repository**. This repository contains **only the backend API** services.

For the complete user interface and client application, please refer to the frontend repository:

**Frontend Repository:** *(Link to frontend repo when available)*

### Integration

The frontend communicates with this API using:
- Base URL: `http://localhost:5062` (development)
- JWT authentication via `Authorization` headers
- RESTful JSON endpoints
- Swagger documentation for API contract reference

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'feat: add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Commit Convention

Follow [Conventional Commits](https://www.conventionalcommits.org/):
- `feat:` - New features
- `fix:` - Bug fixes
- `docs:` - Documentation changes
- `refactor:` - Code refactoring
- `test:` - Adding tests
- `chore:` - Maintenance tasks

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 👥 Author

**Mohamad Hamed**
- GitHub: [@muhamad-hamed](https://github.com/muhamad-hamed)
- Email: muhamaad.hamed@gmail.com

## 📞 Support

For issues and questions:
- Create an [Issue](https://github.com/muhamad-hamed/AssetVest/issues)
- Check existing [Documentation](docs/)
- Review [Troubleshooting Guide](docs/QUICK_START.md#-troubleshooting)

## 🗺️ Roadmap

- [ ] Complete CQRS for remaining entities (AnnualGoal, AssetTypeAllocationGoal, AssetValueHistory)
- [ ] Implement pagination for list endpoints
- [ ] Add response caching with ETags
- [ ] FX rate auto-sync background service
- [ ] Advanced reporting and analytics
- [ ] Export to CSV/Excel
- [ ] CI/CD pipeline
- [ ] Production deployment guides

---

**Built with ❤️ using .NET 10**

*Last Updated: June 6, 2026*
