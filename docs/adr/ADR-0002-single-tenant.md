# ADR-0002 — Tenancy Strategy: Single-Tenant

**Status:** Accepted  
**Date:** 2026-05-30

## Context

AssetVest is a personal investment manager built for a single user (the owner). No requirement for multiple organizations or isolated tenant data exists at this time.

## Decision

Build as **single-tenant** application. No tenant isolation layer.

## Reasons

- Scope is a personal finance tool, not a SaaS product
- Eliminates tenant ID columns, row-level security, and tenant middleware complexity
- Simpler domain model and query logic
- All data is owned by a single authenticated user

## Consequences

- No `TenantId` on any entity
- User identity (from JWT) scopes all queries via `UserId`
- If multi-tenant requirement arises in future, migration path = add `TenantId` columns and tenant resolution middleware
