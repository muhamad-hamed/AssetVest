# CQRS + MediatR Refactoring - Complete Implementation Guide

## Overview

Successfully refactored the User management system from a simple service pattern to full CQRS (Command Query Responsibility Segregation) with MediatR, following the architecture specified in [ADR-0003](../../../docs/adr/ADR-0003-cqrs-mediatr.md).

## Why Not in V1 Folder?

**Answer**: The project uses **attribute-based API versioning**, not folder-based versioning.

### Versioning Strategy (ADR-0004)
- Version is specified via `[ApiVersion("1.0")]` attribute on the controller
- Route template includes version placeholder: `[Route("api/v{version:apiVersion}/[controller]")]`
- This produces URLs like `/api/v1/users` automatically
- Controllers can support multiple versions with additional attributes
- No need for physical V1/V2 folders

### Benefits
- Single controller file can support multiple API versions
- Easier to deprecate old versions with `[ApiVersion("1.0", Deprecated = true)]`
- Better Swagger/OpenAPI integration (auto-generates docs per version)
- Cleaner project structure

---

## Architecture Changes

### Before (Service Pattern)
```
Application/
├── Services/
│   └── IUserService.cs          # Interface with all operations
└── DTOs/Users/                  # Request/Response DTOs

Infrastructure/
└── Services/
    └── UserService.cs           # Implementation with EF Core

Api/Controllers/
└── UsersController.cs           # Calls IUserService methods
```

### After (CQRS + MediatR)
```
Application/
├── Commands/Users/              # Write operations
│   ├── CreateUser/
│   │   ├── CreateUserCommand.cs           # Request (IRequest<UserDto>)
│   │   └── CreateUserCommandValidator.cs   # FluentValidation rules
│   ├── UpdateUser/
│   ├── DeleteUser/
│   ├── ChangePassword/
│   └── ToggleActiveStatus/
├── Queries/Users/               # Read operations
│   ├── GetAllUsers/
│   │   └── GetAllUsersQuery.cs            # Request (IRequest<List<UserDto>>)
│   ├── GetUserById/
│   ├── GetUserByEmail/
│   └── GetCurrentUser/
├── Behaviors/                   # Pipeline middleware
│   ├── ValidationBehavior.cs    # Auto-validates using FluentValidation
│   └── LoggingBehavior.cs       # Logs request/response with timing
└── DTOs/Users/                  # Response DTOs (unchanged)

Infrastructure/
└── Handlers/                    # Implementations
    ├── Commands/Users/
    │   ├── CreateUserCommandHandler.cs
    │   ├── UpdateUserCommandHandler.cs
    │   ├── DeleteUserCommandHandler.cs
    │   ├── ChangePasswordCommandHandler.cs
    │   └── ToggleUserActiveStatusCommandHandler.cs
    └── Queries/Users/
        ├── GetAllUsersQueryHandler.cs
        ├── GetUserByIdQueryHandler.cs
        ├── GetUserByEmailQueryHandler.cs
        └── GetCurrentUserQueryHandler.cs

Api/Controllers/
└── UsersController.cs           # Dispatches via MediatR (ISender)
```

---

## Key Components

### 1. Commands (Write Operations)

Commands represent **state-changing operations** (Create, Update, Delete).

#### Example: CreateUserCommand
```csharp
// Application/Commands/Users/CreateUser/CreateUserCommand.cs
public record CreateUserCommand : IRequest<UserDto>
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
    public required string Password { get; init; }
}
```

**Key Points**:
- Defined as `record` for immutability
- Implements `IRequest<TResponse>` from MediatR
- Lives in **Application** layer (no dependencies on Infrastructure)
- Properties use `required` keyword (C# 11+)

#### Handler: CreateUserCommandHandler
```csharp
// Infrastructure/Handlers/Commands/Users/CreateUserCommandHandler.cs
public class CreateUserCommandHandler(ApplicationDbContext context) 
    : IRequestHandler<CreateUserCommand, UserDto>
{
    public async Task<UserDto> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        // Business logic + data access
        var user = new User { /* ... */ };
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
        return MapToDto(user);
    }
}
```

**Key Points**:
- Lives in **Infrastructure** layer (can access DbContext)
- Implements `IRequestHandler<TRequest, TResponse>`
- Uses primary constructor for DI
- Handles one specific command only (Single Responsibility Principle)

#### Validator: CreateUserCommandValidator
```csharp
// Application/Commands/Users/CreateUser/CreateUserCommandValidator.cs
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required")
            .MaximumLength(100);
        
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
        
        RuleFor(x => x.Password)
            .MinimumLength(8);
    }
}
```

**Key Points**:
- Uses FluentValidation library
- Auto-discovered by `AddValidatorsFromAssembly()`
- Executed by `ValidationBehavior` before handler runs
- Throws `ValidationException` if validation fails

---

### 2. Queries (Read Operations)

Queries represent **data retrieval operations** (no side effects).

#### Example: GetUserByIdQuery
```csharp
// Application/Queries/Users/GetUserById/GetUserByIdQuery.cs
public record GetUserByIdQuery(Guid UserId) : IRequest<UserDto?>;
```

**Key Points**:
- Even simpler than commands (often just an ID parameter)
- Returns `UserDto?` (nullable) when user might not exist
- Uses positional record syntax for single-parameter queries

#### Handler: GetUserByIdQueryHandler
```csharp
// Infrastructure/Handlers/Queries/Users/GetUserByIdQueryHandler.cs
public class GetUserByIdQueryHandler(ApplicationDbContext context) 
    : IRequestHandler<GetUserByIdQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        return await context.Users
            .AsNoTracking()  // Read-only query optimization
            .Where(u => u.Id == request.UserId)
            .Select(u => new UserDto { /* projection */ })
            .FirstOrDefaultAsync(cancellationToken);
    }
}
```

**Key Points**:
- Uses `AsNoTracking()` for read-only queries (better performance)
- Projects directly to DTO in SQL (no entity loading + mapping)
- Returns null if not found (handled by controller)

---

### 3. Pipeline Behaviors (Cross-Cutting Concerns)

Behaviors wrap **all** requests and add cross-cutting functionality.

#### ValidationBehavior
```csharp
// Application/Behaviors/ValidationBehavior.cs
public class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any()) return await next();

        var context = new ValidationContext<TRequest>(request);
        var results = await Task.WhenAll(validators.Select(v => v.ValidateAsync(context, cancellationToken)));
        
        var failures = results.SelectMany(r => r.Errors).Where(f => f != null).ToList();
        if (failures.Any())
            throw new ValidationException(failures);

        return await next();
    }
}
```

**Execution Flow**:
1. User sends request → Controller creates Command/Query
2. Controller calls `_sender.Send(command)`
3. **ValidationBehavior** runs first → validates request
4. **LoggingBehavior** runs second → logs start time
5. **Handler** executes → business logic
6. **LoggingBehavior** logs completion + duration
7. Response returns to controller

#### LoggingBehavior
```csharp
// Application/Behaviors/LoggingBehavior.cs
public class LoggingBehavior<TRequest, TResponse>(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();

        logger.LogInformation("Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            stopwatch.Stop();
            logger.LogInformation("Handled {RequestName} in {ElapsedMs}ms", requestName, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error handling {RequestName}", requestName);
            throw;
        }
    }
}
```

---

### 4. Controller Updates

#### Before (Service Pattern)
```csharp
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request)
    {
        var user = await userService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
    }
}
```

#### After (CQRS + MediatR)
```csharp
public class UsersController(ISender sender) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<UserDto>> Create([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CreateUserCommand
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email,
                Password = request.Password
            };

            var user = await sender.Send(command, cancellationToken);
            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Validation Error",
                Detail = string.Join(", ", ex.Errors.Select(e => e.ErrorMessage)),
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
```

**Key Changes**:
- Constructor injects `ISender` (from MediatR) instead of service
- Creates command object from request DTO
- Calls `sender.Send(command)` to dispatch to handler
- Catches `ValidationException` from ValidationBehavior

---

## Dependency Injection Setup

### Application Layer
```csharp
// Application/DependencyInjection/ApplicationServiceCollectionExtensions.cs
public static IServiceCollection AddApplication(this IServiceCollection services)
{
    services.AddMediatR(cfg =>
    {
        // Register all handlers from Application assembly
        cfg.RegisterServicesFromAssembly(typeof(ApplicationServiceCollectionExtensions).Assembly);

        // Register pipeline behaviors (ORDER MATTERS - executes in registration order)
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));      // 1st: Logging
        cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));  // 2nd: Validation
    });

    // Auto-discover all validators in Application assembly
    services.AddValidatorsFromAssembly(
        typeof(ApplicationServiceCollectionExtensions).Assembly,
        includeInternalTypes: true);

    return services;
}
```

### Infrastructure Layer
```csharp
// Infrastructure/DependencyInjection/InfrastructureServiceCollectionExtensions.cs
public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
{
    services.AddDbContext<ApplicationDbContext>(/*...*/);

    // Register all handlers from Infrastructure assembly
    services.AddMediatR(cfg =>
        cfg.RegisterServicesFromAssembly(typeof(InfrastructureServiceCollectionExtensions).Assembly));

    return services;
}
```

**Why Two AddMediatR Calls?**
- Application assembly has validators and behaviors
- Infrastructure assembly has handler implementations
- MediatR merges registrations from both assemblies

---

## Testing Strategy

### Before (Service Tests)
```csharp
public class UserServiceTests
{
    private readonly IUserService _userService;

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesUser()
    {
        var request = new CreateUserRequest { /* ... */ };
        var result = await _userService.CreateAsync(request);
        result.Should().NotBeNull();
    }
}
```

### After (Handler Tests)
```csharp
public class UserHandlersTests
{
    private readonly ApplicationDbContext _context;

    [Fact]
    public async Task CreateUserCommandHandler_WithValidData_CreatesUser()
    {
        // Arrange
        var handler = new CreateUserCommandHandler(_context);
        var command = new CreateUserCommand { /* ... */ };

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        var userInDb = await _context.Users.FindAsync(result.Id);
        userInDb.Should().NotBeNull();
    }
}
```

**Key Differences**:
- Test handlers directly (no need to mock entire service)
- Each handler tested in isolation
- Easier to test validation logic separately
- Integration tests remain unchanged (test via HTTP endpoints)

---

## Benefits of CQRS + MediatR

### 1. **Separation of Concerns**
- Each command/query = one responsibility
- Easy to find where specific logic lives
- No god-class services with 20+ methods

### 2. **Testability**
- Test each handler independently
- Mock only DbContext (not entire service layer)
- Validators testable in isolation

### 3. **Maintainability**
- Add new operations without touching existing code
- Each feature in its own folder
- Easy to apply patterns (authorization, caching, etc.) to specific handlers

### 4. **Pipeline Behaviors (Cross-Cutting Concerns)**
- Validation runs automatically for all commands
- Logging happens transparently
- Easy to add more behaviors (caching, retry, audit, etc.)

### 5. **Scalability**
- Queries can be optimized differently than commands
- Can separate read/write databases in future (CQRS full potential)
- Easy to add complexity only where needed

### 6. **Clean Architecture Compliance**
- Application layer defines contracts (commands/queries)
- Infrastructure implements details (handlers)
- API layer just dispatches requests
- No circular dependencies

---

## Command vs Query Decision Tree

```
Is this operation changing data?
│
├─ YES → Create a COMMAND
│  ├─ Returns void/bool for simple operations
│  ├─ Returns DTO for create operations
│  └─ Add validator if complex rules exist
│
└─ NO → Create a QUERY
   ├─ Returns single DTO or null
   ├─ Returns collection of DTOs
   └─ Use AsNoTracking() in handler
```

---

## Complete File Structure

```
Application/
├── Commands/
│   └── Users/
│       ├── CreateUser/
│       │   ├── CreateUserCommand.cs
│       │   └── CreateUserCommandValidator.cs
│       ├── UpdateUser/
│       │   ├── UpdateUserCommand.cs
│       │   └── UpdateUserCommandValidator.cs
│       ├── DeleteUser/
│       │   └── DeleteUserCommand.cs
│       ├── ChangePassword/
│       │   ├── ChangePasswordCommand.cs
│       │   └── ChangePasswordCommandValidator.cs
│       └── ToggleActiveStatus/
│           └── ToggleUserActiveStatusCommand.cs
├── Queries/
│   └── Users/
│       ├── GetAllUsers/
│       │   └── GetAllUsersQuery.cs
│       ├── GetUserById/
│       │   └── GetUserByIdQuery.cs
│       ├── GetUserByEmail/
│       │   └── GetUserByEmailQuery.cs
│       └── GetCurrentUser/
│           └── GetCurrentUserQuery.cs
├── Behaviors/
│   ├── ValidationBehavior.cs
│   └── LoggingBehavior.cs
├── DTOs/
│   └── Users/
│       ├── UserDto.cs
│       ├── CreateUserRequest.cs (still used for API contract)
│       ├── UpdateUserRequest.cs
│       └── ChangePasswordRequest.cs
└── DependencyInjection/
    └── ApplicationServiceCollectionExtensions.cs

Infrastructure/
├── Handlers/
│   ├── Commands/
│   │   └── Users/
│   │       ├── CreateUserCommandHandler.cs
│   │       ├── UpdateUserCommandHandler.cs
│   │       ├── DeleteUserCommandHandler.cs
│   │       ├── ChangePasswordCommandHandler.cs
│   │       └── ToggleUserActiveStatusCommandHandler.cs
│   └── Queries/
│       └── Users/
│           ├── GetAllUsersQueryHandler.cs
│           ├── GetUserByIdQueryHandler.cs
│           ├── GetUserByEmailQueryHandler.cs
│           └── GetCurrentUserQueryHandler.cs
├── Persistence/
│   └── ApplicationDbContext.cs
└── DependencyInjection/
    └── InfrastructureServiceCollectionExtensions.cs

Api/
└── Controllers/
    └── UsersController.cs (uses ISender, not IUserService)

Tests/
├── AssetVest.Application.Tests/
│   └── Handlers/
│       └── UserHandlersTests.cs (9 tests - all passing ✅)
└── AssetVest.Integration.Tests/
    └── Controllers/
        └── UsersControllerTests.cs (unchanged - tests via HTTP)
```

---

## Test Results

```bash
$ dotnet test AssetVest.sln --filter "FullyQualifiedName~UserHandlers"

Test summary: total: 9, failed: 0, succeeded: 9, skipped: 0
Build succeeded ✅
```

### Tests Covered
1. ✅ GetUserByIdQueryHandler - when user exists
2. ✅ GetUserByIdQueryHandler - when user doesn't exist
3. ✅ GetUserByEmailQueryHandler - when user exists
4. ✅ CreateUserCommandHandler - with valid data
5. ✅ CreateUserCommandHandler - with duplicate email (throws exception)
6. ✅ UpdateUserCommandHandler - with valid data
7. ✅ DeleteUserCommandHandler - soft delete verification
8. ✅ GetAllUsersQueryHandler - returns ordered list
9. ✅ ToggleUserActiveStatusCommandHandler - toggles status

---

## Migration Summary

### Removed Files
- ❌ `Application/Services/IUserService.cs`
- ❌ `Infrastructure/Services/UserService.cs`
- ❌ `Tests/UserServiceTests.cs`

### Added Files (23 new files)
- ✅ 5 Commands + 3 Validators
- ✅ 4 Queries
- ✅ 9 Handlers (5 command + 4 query)
- ✅ 2 Pipeline Behaviors
- ✅ 1 Test file (UserHandlersTests.cs)

### Modified Files
- ✅ `UsersController.cs` - uses ISender instead of IUserService
- ✅ `ApplicationServiceCollectionExtensions.cs` - registers behaviors
- ✅ `InfrastructureServiceCollectionExtensions.cs` - registers handlers

---

## Next Steps (Optional Enhancements)

### 1. Performance Optimization
- Add **CachingBehavior** for frequently accessed queries
- Implement **read replicas** for query-heavy operations

### 2. Advanced Patterns
- **AuditBehavior** - log all commands to audit table
- **RetryBehavior** - automatic retry for transient failures
- **AuthorizationBehavior** - check permissions before handler

### 3. Event Sourcing (Advanced)
- Publish domain events after successful commands
- Use MediatR notifications for side effects
- Example: Send email after user creation

### 4. Pagination & Filtering
- Add `PagedQuery<T>` base class for queries
- Return `PagedResult<T>` with total count
- Add LINQ expressions for dynamic filtering

---

## References

- [ADR-0003: CQRS + MediatR](../../../docs/adr/ADR-0003-cqrs-mediatr.md)
- [ADR-0004: URL Versioning](../../../docs/adr/ADR-0004-url-versioning.md)
- [MediatR Documentation](https://github.com/jbogard/MediatR)
- [FluentValidation Documentation](https://docs.fluentvalidation.net/)
