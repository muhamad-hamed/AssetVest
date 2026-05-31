---
name: AssetVest project context
description: Core facts about the AssetVest investment manager project — stack, constraints, and architectural decisions made
type: project
---

AssetVest is a greenfield investment manager and trade tracking system.

**Stack:**
- Backend: C# / .NET, clean architecture (Domain / Application / Infrastructure / Api)
- Frontend: Vue.js (added later), lives at src/frontend/ in the same monorepo root
- Database: Not yet decided (ADR-0001 pending)
- Auth: OAuth 2.0 / JWT required (Siemens API conventions); identity provider not yet chosen (ADR-0005 pending)

**Architectural constraints:**
- Siemens API Guidelines v2.5.1 apply: contract-first OpenAPI, camelCase JSON, kebab-case URLs, URI path versioning (/v1/), OAuth 2.0 Bearer
- Tenant isolation required: tenant_id extracted from JWT only, never from user input; RLS or equivalent must be designed in
- Clean architecture dependency rule enforced from day one (inner layers have no outward dependencies)

**Root layout:**
- src/backend/ — .NET solution (four projects: Domain, Application, Infrastructure, Api)
- src/frontend/ — Vue project (created later)
- docs/ — ADRs, architecture diagrams, API contracts, runbooks
- docker/ — compose files
- scripts/ — developer utilities

**Key open decisions (ADR candidates):**
- ADR-0001: Database engine
- ADR-0002: Tenant data isolation strategy
- ADR-0003: CQRS implementation depth
- ADR-0004: API versioning strategy
- ADR-0005: Auth/identity provider
- ADR-0006: Audit trail / event mechanism
- ADR-0007: Frontend API client code-generation
- ADR-0008: Monorepo tooling

**Why:** Greenfield project being planned from scratch. Architecture-first approach; no code written yet as of 2026-05-29.

**How to apply:** Use this context to inform all future design work, ADR writing, and implementation guidance. Always check whether pending ADR decisions have been resolved before making concrete recommendations that depend on them.
