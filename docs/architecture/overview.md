# AssetVest — Architecture Overview

**Last Updated:** 2026-05-30

---

## What Is AssetVest

Personal investment manager and trade tracking system. Tracks assets across multiple asset types (stocks, gold, foreign currency, mutual funds, real estate, crypto, bonds), computes profit/loss in EGP, stores value history, and supports annual financial goals.

Backend only. Frontend (Vue.js) will be a separate repository created later.

---

## Solution Structure

```
AssetVest/
├── src/
│   ├── AssetVest.Domain/           # Entities, value objects, domain events, enums
│   ├── AssetVest.Application/      # CQRS handlers, DTOs, ports, validators
│   ├── AssetVest.Infrastructure/   # EF Core, PostgreSQL, JWT, Serilog, FX client
│   └── AssetVest.Api/              # Controllers, middleware, Swagger, startup
├── tests/
│   ├── AssetVest.Domain.Tests/     # Unit tests — domain logic, value objects
│   ├── AssetVest.Application.Tests/# Unit tests — command/query handlers (mocked)
│   └── AssetVest.Integration.Tests/# Integration tests — real DB (Testcontainers)
├── docs/
│   ├── adr/                        # Architecture Decision Records
│   ├── architecture/               # Domain model, diagrams, this file
│   ├── api/                        # OpenAPI specs
│   └── runbooks/                   # Operational guides
├── docker/                         # Dockerfile, docker-compose
└── scripts/                        # DB migration scripts, build scripts
```

---

## Clean Architecture Layers

```
                    ┌─────────────────── ──┐
                    │   AssetVest.Api      │  HTTP, auth middleware, Swagger
                    │   (outermost)        │  depends on: Application
                    └──────────┬──────── ──┘
                               │
              ┌────────────────┴──────────────── ┐
              │                                  │
   ┌──────────▼───────── ─┐       ┌──────────────▼───────── ─┐
   │ AssetVest.Application│       │ AssetVest.Infrastructure │
   │  Commands, Queries   │       │  EF Core, PostgreSQL     │
   │  MediatR handlers    │       │  JWT generation          │
   │  Ports (interfaces)  │       │  Serilog sinks           │
   │  FluentValidation    │       │  FX rate HTTP client     │
   │  depends on: Domain  │       │  depends on: Application │
   └──────────┬────────── ┘       └──────────────────────────┘
              │
   ┌──────────▼────────── ┐
   │  AssetVest.Domain    │  Entities, value objects, enums
   │  (innermost)         │  NO dependencies (pure C#)
   └───────────────────── ┘
```

**Dependency rule:** Arrows point inward only. Domain knows nothing. Infrastructure implements ports defined in Application.

---

## Technology Stack

| Layer | Technology |
|-------|-----------|
| Runtime | .NET 10 / ASP.NET Core |
| ORM | Entity Framework Core 10 + Npgsql |
| Database | PostgreSQL |
| CQRS | MediatR |
| Validation | FluentValidation |
| Auth | ASP.NET Core JWT Bearer + custom token service |
| Logging | Serilog (Console, File, Seq sinks) |
| API Versioning | Asp.Versioning.Mvc (URL segment: `/api/v1/`) |
| API Docs | Swashbuckle (Swagger UI) |
| Testing | xUnit + FluentAssertions + Testcontainers |
| Dev DB UI | pgAdmin or DBeaver |
| Log UI (dev) | Seq (http://localhost:5341) |

---

## Key Architectural Decisions

| ADR | Decision | File |
|-----|----------|------|
| ADR-0001 | PostgreSQL | [ADR-0001](../adr/ADR-0001-postgresql.md) |
| ADR-0002 | Single-tenant | [ADR-0002](../adr/ADR-0002-single-tenant.md) |
| ADR-0003 | Full CQRS + MediatR | [ADR-0003](../adr/ADR-0003-cqrs-mediatr.md) |
| ADR-0004 | URL segment versioning | [ADR-0004](../adr/ADR-0004-url-versioning.md) |
| ADR-0005 | Self-issued JWT auth | [ADR-0005](../adr/ADR-0005-jwt-auth.md) |
| ADR-0006 | Serilog + EF Core audit | [ADR-0006](../adr/ADR-0006-audit-logging.md) |

---

## Default Currency

**Egyptian Pound (EGP)** is the system base currency. All asset values stored and displayed in EGP. Foreign currency assets converted via FX rates. All profit/loss calculations performed in EGP.

---

## MediatR Pipeline (execution order per request)

```
Request → LoggingBehavior → ValidationBehavior → AuditBehavior → Handler → Response
```

- `LoggingBehavior`: logs request name + elapsed ms
- `ValidationBehavior`: runs FluentValidation, throws if invalid (commands only)
- `AuditBehavior`: records command intent to AuditLog (commands only, skips queries)

---

## Authentication Flow

```
POST /api/v1/auth/login
  → validate credentials
  → issue access token (15 min JWT)
  → issue refresh token (7 days, stored hashed in DB)

POST /api/v1/auth/refresh
  → validate refresh token
  → rotate: revoke old, issue new pair

POST /api/v1/auth/logout
  → revoke refresh token
```

---

## Audit Trail

Two layers:

1. **MediatR AuditBehavior** — records command name, userId, timestamp, payload
2. **EF Core SaveChanges override** — records entity-level before/after values to `AuditLog` table

All entities soft-deleted only (`IsDeleted = true`). Global EF query filter excludes soft-deleted records automatically.

---

## Domain Model

Full entity definitions: [domain-model.md](./domain-model.md)
