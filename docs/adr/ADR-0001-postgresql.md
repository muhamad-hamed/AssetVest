# ADR-0001 — Database Engine: PostgreSQL

**Status:** Accepted  
**Date:** 2026-05-30

## Context

AssetVest requires a relational database to store investment portfolio data including assets, value history, FX rates, and user data. The database must handle decimal precision for financial calculations, support for JSON columns (optional future use), and time-series-like history tables.

## Decision

Use **PostgreSQL** as the primary database engine.

## Reasons

- Excellent decimal/numeric precision — critical for financial values
- Strong JSON/JSONB support for future flexibility
- EF Core support via `Npgsql.EntityFrameworkCore.PostgreSQL`
- Free and open source, no licensing cost
- Strong community and tooling ecosystem
- Handles time-series queries well (history tables)
- Default currency is EGP — no special encoding requirements

## Consequences

- Infrastructure layer uses `Npgsql` EF Core provider
- Migrations via EF Core `dotnet ef migrations`
- Local dev requires PostgreSQL instance (Docker recommended)
- Connection string stored in environment variable / user secrets (never committed)
