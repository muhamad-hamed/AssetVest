# User Controllers Implementation

## Overview

Complete ASP.NET Core Web API implementation for user management following Clean Architecture principles with comprehensive CRUD operations, authentication support, and full test coverage.

## Architecture

### Layers

#### Domain Layer (`AssetVest.Domain`)
- **User Entity**: Inherits from `AuditableEntity` with automatic audit tracking
  - Properties: Id, FirstName, LastName, Email, PasswordHash, IsActive
  - Relationships: RefreshTokens, Assets, AnnualGoals, StockProfitGoals

#### Application Layer (`AssetVest.Application`)
- **DTOs**:
  - `UserDto`: Response DTO for user data (excludes sensitive fields)
  - `CreateUserRequest`: Input DTO for creating new users
  - `UpdateUserRequest`: Input DTO for updating existing users
  - `ChangePasswordRequest`: Input DTO for password changes
  
- **Service Interface**: `IUserService` defining all user operations

#### Infrastructure Layer (`AssetVest.Infrastructure`)
- **UserService**: Service implementation with EF Core operations
  - Uses BCrypt for secure password hashing
  - Implements email uniqueness validation
  - Handles soft deletes via DbContext

#### API Layer (`AssetVest.Api`)
- **UsersController**: RESTful API endpoints with proper HTTP semantics
- **GlobalExceptionHandlerMiddleware**: RFC 7807 Problem Details responses
- API versioning enabled (v1.0)
- OpenAPI/Swagger documentation

## API Endpoints

### Base URL: `/api/v1/users`

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| GET | `/api/v1/users` | Get all users | ✅ |
| GET | `/api/v1/users/{id}` | Get user by ID | ✅ |
| GET | `/api/v1/users/me` | Get current authenticated user | ✅ |
| GET | `/api/v1/users/by-email/{email}` | Get user by email | ✅ |
| POST | `/api/v1/users` | Create a new user | ✅ |
| PUT | `/api/v1/users/{id}` | Update user | ✅ |
| DELETE | `/api/v1/users/{id}` | Delete user (soft delete) | ✅ |
| POST | `/api/v1/users/{id}/change-password` | Change user password | ✅ |
| POST | `/api/v1/users/{id}/toggle-active` | Toggle user active status | ✅ |

## Request/Response Examples

### Create User
```http
POST /api/v1/users
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "password": "SecurePassword123!"
}
```

**Response (201 Created)**
```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@example.com",
  "isActive": true,
  "createdAt": "2026-05-31T12:00:00Z",
  "updatedAt": null
}
```

### Update User
```http
PUT /api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6
Content-Type: application/json

{
  "firstName": "John",
  "lastName": "Smith",
  "email": "john.smith@example.com"
}
```

### Change Password
```http
POST /api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/change-password
Content-Type: application/json

{
  "currentPassword": "OldPassword123!",
  "newPassword": "NewSecurePassword456!"
}
```

## Features

### Security
- ✅ BCrypt password hashing (BCrypt.Net-Next 4.2.0)
- ✅ JWT authentication via `[Authorize]` attribute
- ✅ Current user extraction from JWT claims (`ICurrentUserService`)
- ✅ Password validation (minimum 8 characters)

### Validation
- ✅ Data Annotations validation on all DTOs
- ✅ Email format validation
- ✅ Unique email constraint enforcement
- ✅ Required field validation

### Error Handling
- ✅ Global exception handler middleware
- ✅ RFC 7807 Problem Details responses
- ✅ Proper HTTP status codes (200, 201, 204, 400, 401, 404, 500)
- ✅ Detailed error messages for validation failures

### Database
- ✅ Soft delete support (IsDeleted flag)
- ✅ Automatic audit tracking (CreatedAt, CreatedBy, UpdatedAt, UpdatedBy, DeletedAt, DeletedBy)
- ✅ Global query filters for soft deletes
- ✅ EF Core with PostgreSQL

### Best Practices
- ✅ Async/await throughout
- ✅ CancellationToken support
- ✅ Nullable reference types enabled
- ✅ Primary constructors (C# 12)
- ✅ Record types for DTOs (immutability)
- ✅ AsNoTracking for read-only queries
- ✅ Dependency injection
- ✅ Clean Architecture separation
- ✅ XML documentation comments

## Testing

### Unit Tests (`AssetVest.Application.Tests`)
Comprehensive unit tests for `UserService` using:
- xUnit test framework
- FluentAssertions for readable assertions
- Moq for mocking dependencies
- EF Core InMemory database for isolation

**Test Coverage**:
- ✅ GetByIdAsync (exists and not exists)
- ✅ GetByEmailAsync
- ✅ GetAllAsync with ordering
- ✅ CreateAsync with validation
- ✅ CreateAsync with duplicate email
- ✅ UpdateAsync with validation
- ✅ DeleteAsync (soft delete verification)
- ✅ ToggleActiveStatusAsync

### Integration Tests (`AssetVest.Integration.Tests`)
Integration tests for `UsersController` using:
- WebApplicationFactory for in-memory test server
- Real HTTP requests/responses
- Full middleware pipeline

**9/9 tests passing** ✅

## Dependencies Added

### Infrastructure
```xml
<PackageReference Include="BCrypt.Net-Next" Version="4.2.0" />
```

### Application.Tests
```xml
<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.8" />
<PackageReference Include="Moq" Version="4.20.72" />
```

## Configuration

### Dependency Injection
Services are registered in `InfrastructureServiceCollectionExtensions.cs`:
```csharp
services.AddScoped<IUserService, UserService>();
```

### Middleware Pipeline
Global exception handler is registered in `Program.cs`:
```csharp
app.UseGlobalExceptionHandler();
```

### Database Configuration
User entity configuration in `UserConfiguration.cs`:
- Email uniqueness constraint
- Cascade delete for RefreshTokens
- Restrict delete for related entities (Assets, Goals)

## Next Steps

1. **Authentication/Authorization**
   - Implement role-based authorization
   - Add password reset functionality
   - Implement email verification

2. **Enhanced Validation**
   - Add FluentValidation for complex business rules
   - Implement password strength policies
   - Add rate limiting for sensitive endpoints

3. **Performance**
   - Add response caching
   - Implement pagination for GetAll endpoint
   - Add distributed caching with Redis

4. **Observability**
   - Add structured logging with Serilog (already configured)
   - Implement health checks for database connectivity
   - Add metrics and monitoring

## Usage

1. **Build the solution**:
   ```bash
   dotnet build AssetVest.sln
   ```

2. **Run tests**:
   ```bash
   dotnet test AssetVest.sln
   ```

3. **Run the API**:
   ```bash
   dotnet run --project src/AssetVest.Api
   ```

4. **Access Swagger UI**:
   Navigate to `https://localhost:5001/swagger` (development only)

## Files Created/Modified

### Created
- `src/AssetVest.Application/DTOs/Users/UserDto.cs`
- `src/AssetVest.Application/DTOs/Users/CreateUserRequest.cs`
- `src/AssetVest.Application/DTOs/Users/UpdateUserRequest.cs`
- `src/AssetVest.Application/DTOs/Users/ChangePasswordRequest.cs`
- `src/AssetVest.Application/Services/IUserService.cs`
- `src/AssetVest.Infrastructure/Services/UserService.cs`
- `src/AssetVest.Api/Controllers/UsersController.cs`
- `src/AssetVest.Api/Middleware/GlobalExceptionHandlerMiddleware.cs`
- `tests/AssetVest.Application.Tests/Services/UserServiceTests.cs`
- `tests/AssetVest.Integration.Tests/Controllers/UsersControllerTests.cs`

### Modified
- `src/AssetVest.Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs`
- `src/AssetVest.Api/Program.cs`
- `tests/AssetVest.Application.Tests/AssetVest.Application.Tests.csproj`
- `src/AssetVest.Infrastructure/AssetVest.Infrastructure.csproj`

## Notes

- Password hashing uses BCrypt with default work factor (11)
- Soft deletes are handled automatically by the DbContext
- All endpoints require authentication (JWT Bearer token)
- Email addresses are case-sensitive for uniqueness checks
- User IDs are GUIDs generated on creation
