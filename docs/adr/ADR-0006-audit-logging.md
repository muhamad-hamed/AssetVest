# ADR-0006 — Audit Trail and Structured Logging

**Status:** Accepted  
**Date:** 2026-05-30

## Context

AssetVest handles financial data. Full observability is required: who did what and when (audit), plus runtime behavior visibility (logging). Two complementary layers needed.

## Decision

### Structured Logging: Serilog

- Library: `Serilog` with `Microsoft.Extensions.Logging` as abstraction
- Sinks:
  - **Development**: Console + Seq (http://localhost:5341)
  - **Production**: Rolling file + Console (stdout for container)
- Log levels: `Debug` (dev), `Information` (prod minimum)
- Enrichers: `WithMachineName`, `WithEnvironmentName`, `WithCorrelationId`

### Audit Trail: Two-Layer Approach

**Layer 1 — MediatR AuditBehavior (intent level)**  
Intercepts all Commands before and after execution.  
Records: command type, userId, timestamp, serialized request payload, success/failure.

**Layer 2 — EF Core SaveChanges override (entity change level)**  
Intercepts all DB writes. Records before/after values for changed entities.  
Stored in `AuditLog` table in PostgreSQL.

### `AuditLog` Table

| Field | Type | Notes |
|-------|------|-------|
| Id | Guid | |
| UserId | Guid | who performed action |
| EntityName | string | e.g. "Asset" |
| EntityId | string | PK of changed entity |
| Action | string | Created / Updated / Deleted |
| OldValues | jsonb | nullable, previous state |
| NewValues | jsonb | new state |
| CommandName | string | MediatR command that triggered it |
| Timestamp | DateTime | UTC |
| IpAddress | string? | from HttpContext |

## Audit Columns on All Entities

All entities inherit from `AuditableEntity`:

```
CreatedAt    DateTime     (UTC, set on insert)
CreatedBy    Guid?        (UserId from JWT)
UpdatedAt    DateTime?    (UTC, set on update)
UpdatedBy    Guid?        (UserId from JWT)
DeletedAt    DateTime?    (UTC, soft delete)
DeletedBy    Guid?        (UserId who deleted)
IsDeleted    bool         (default false)
```

Soft delete enforced via EF Core global query filter: `HasQueryFilter(e => !e.IsDeleted)`

## Consequences

- No hard deletes in application code — always soft delete
- Full history of every entity change available in `AuditLog`
- Serilog pipeline configured in `Program.cs` before host build
- Seq required for local development (Docker Compose service)
- `ICurrentUserService` interface in Application layer, implemented in Api layer, injected into SaveChanges override
