# ADR-0004 — API Versioning: URL Segment

**Status:** Accepted  
**Date:** 2026-05-30

## Context

The API needs a versioning strategy that supports evolving endpoints without breaking existing consumers. Options evaluated: URL segment, query string, HTTP header, and media type versioning.

## Decision

Use **URL segment versioning**: `/api/v1/assets`, `/api/v2/assets`

Implementation via `Asp.Versioning.Mvc` NuGet package.

## Reasons

- Most visible — version is explicit in the URL
- Best OpenAPI/Swagger tooling support
- Easy to test in browser or any HTTP client
- Clear in application logs and server logs
- Industry standard for REST APIs

## Implementation Notes

```csharp
// Controller attribute
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AssetsController : ControllerBase { }
```

- Default version: `1.0`
- Deprecated versions marked with `[ApiVersion("1.0", Deprecated = true)]`
- Swagger UI generates separate doc per version

## Consequences

- URL changes when version increments (expected behavior)
- Old versions can be maintained in parallel until sunset
- Query string, header versioning explicitly not used
