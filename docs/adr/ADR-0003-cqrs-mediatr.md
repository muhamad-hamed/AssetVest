# ADR-0003 — Application Pattern: Full CQRS + MediatR

**Status:** Accepted  
**Date:** 2026-05-30

## Context

The Application layer needs a consistent pattern for handling business operations. Options considered: simple service classes, partial CQRS, or full CQRS with mediator pattern.

## Decision

Use **full CQRS with MediatR** (`MediatR` NuGet package).

- Commands: mutate state (Create, Update, Delete operations)
- Queries: read-only, return DTOs (never domain entities)
- All handlers live in `AssetVest.Application`
- Pipeline behaviors for cross-cutting concerns (validation, logging, audit)

## Structure

```
Application/
├── Commands/
│   ├── Assets/
│   │   ├── CreateAssetCommand.cs
│   │   └── CreateAssetCommandHandler.cs
│   └── Users/
├── Queries/
│   ├── Assets/
│   │   ├── GetAssetByIdQuery.cs
│   │   └── GetAssetByIdQueryHandler.cs
│   └── Users/
├── Behaviors/
│   ├── ValidationBehavior.cs
│   ├── LoggingBehavior.cs
│   └── AuditBehavior.cs
└── DTOs/
```

## Pipeline Behaviors (execution order)

1. `LoggingBehavior` — log command/query name + execution time
2. `ValidationBehavior` — FluentValidation before handler runs
3. `AuditBehavior` — record command intent for audit trail (commands only)

## Consequences

- Every use case = one command or query class + one handler class
- No business logic in controllers — Api layer dispatches only
- Cross-cutting concerns injected via pipeline, not inheritance
- Easy to unit test handlers in isolation
