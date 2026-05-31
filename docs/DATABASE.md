# AssetVest Database & Migrations Guide

## Table of Contents

- [Database Architecture](#database-architecture)
- [Local Development Setup](#local-development-setup)
- [Migration Workflow](#migration-workflow)
- [Production Deployment](#production-deployment)
- [Common Commands](#common-commands)
- [Troubleshooting](#troubleshooting)

---

## Database Architecture

### Technology Stack

- **Database**: PostgreSQL 17
- **ORM**: Entity Framework Core 10.0.8
- **Provider**: Npgsql.EntityFrameworkCore.PostgreSQL
- **Migration Tool**: `dotnet ef`

### Database Schema Overview

The AssetVest database contains **17 tables** organized into the following domains:

#### Core Entities

| Table | Purpose | Key Features |
|-------|---------|--------------|
| `Users` | User accounts | BCrypt password hash, soft delete, audit fields |
| `RefreshTokens` | JWT refresh token storage | SHA256 hash, token rotation, expiry tracking |

#### Asset Management

| Table | Purpose | Relationships |
|-------|---------|---------------|
| `Assets` | Core asset records | User FK, polymorphic type (9 types) |
| `StockDetail` | Stock-specific data | Asset FK (1:0..1) |
| `CurrencyDetail` | Foreign currency data | Asset FK (1:0..1) |
| `GoldDetail` | Gold investment data | Asset FK (1:0..1) |
| `RealEstateDetail` | Real estate data | Asset FK (1:0..1) |
| `MutualFundDetail` | Mutual fund data | Asset FK (1:0..1) |
| `CryptoDetail` | Cryptocurrency data | Asset FK (1:0..1) |
| `BondsDetail` | Bonds data | Asset FK (1:0..1) |
| `AssetValueHistory` | Time-series asset values | Asset FK (1:N) |

#### Goals & Planning

| Table | Purpose | Relationships |
|-------|---------|---------------|
| `AnnualGoals` | Yearly financial goals | User FK (1:N) |
| `AssetTypeAllocationGoals` | Target allocations per asset type | AnnualGoal FK (1:N) |
| `StockProfitGoals` | Stock-specific profit targets | User FK (1:N) |

#### Reference Data

| Table | Purpose | Update Frequency |
|-------|---------|------------------|
| `FxRates` | Current exchange rates | Daily (automated) |
| `FxRateHistory` | Historical exchange rates | Daily (append-only) |

#### System Tables

| Table | Purpose | Retention |
|-------|---------|-----------|
| `AuditLogs` | Change tracking (JSON) | All changes to auditable entities |
| `__EFMigrationsHistory` | Applied migrations log | EF Core metadata |

### Soft Delete Pattern

All `AuditableEntity` descendants support soft delete:

```csharp
// Global query filter applied automatically
.HasQueryFilter(e => !e.IsDeleted)
```

**Affected Entities**: `User`, `Asset`, `AnnualGoal`, `StockProfitGoal`

**Fields**:
- `IsDeleted` (bool, default: false)
- `DeletedAt` (DateTime?, nullable)
- `DeletedBy` (Guid?, nullable)

### Audit Fields

All `AuditableEntity` descendants track changes:

```
CreatedAt   (DateTime, UTC)
CreatedBy   (Guid?, from JWT "sub" claim)
UpdatedAt   (DateTime?, UTC)
UpdatedBy   (Guid?, from JWT "sub" claim)
```

**Automatic Population**: `ApplicationDbContext.SaveChangesAsync()` override

---

## Local Development Setup

### Prerequisites

- .NET 10 SDK
- Docker Desktop (for PostgreSQL + Seq)
- EF Core Tools: `dotnet tool install -g dotnet-ef`

### Quick Start

#### 1. Start Infrastructure Services

```powershell
# Start PostgreSQL + Seq in Docker
cd C:\Users\z004xytv\Documents\projects\AssetVest
docker-compose up -d

# Verify containers are running
docker ps

# Expected output:
# assetvest-postgres   postgres:17-alpine   Up   0.0.0.0:5432->5432/tcp
# assetvest-seq        datalust/seq:latest  Up   0.0.0.0:5341->80/tcp
```

#### 2. Wait for PostgreSQL to Initialize

```powershell
# Wait 10 seconds for first-time initialization
Start-Sleep -Seconds 10

# Or check health status
docker exec assetvest-postgres pg_isready -U postgres
# Expected: /var/run/postgresql:5432 - accepting connections
```

#### 3. Apply Database Migrations

```powershell
dotnet ef database update `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api

# Expected output:
# Build succeeded.
# Applying migration '20260531105857_InitialCreate'.
# Done.
```

#### 4. Verify Database Creation

```powershell
# Check database info
dotnet ef dbcontext info `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api

# Connect via psql (optional)
docker exec -it assetvest-postgres psql -U postgres -d AssetVestDb

# List tables
AssetVestDb=# \dt
# Should show 17 tables + __EFMigrationsHistory
```

### Docker Services Configuration

**File**: `docker-compose.yml` (root directory)

```yaml
services:
  postgres:
    image: postgres:17-alpine
    ports: ["5432:5432"]
    environment:
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
      POSTGRES_DB: AssetVestDb
    volumes:
      - postgres_data:/var/lib/postgresql/data
    healthcheck:
      test: ["CMD-SHELL", "pg_isready -U postgres"]
      interval: 5s

  seq:
    image: datalust/seq:latest
    ports: ["5341:80"]
    environment:
      ACCEPT_EULA: Y
      SEQ_FIRSTRUN_ADMINPASSWORD: Admin123!  # Required for first run
    volumes:
      - seq_data:/data
```

**Data Persistence**: Docker volumes (`postgres_data`, `seq_data`) persist data across container restarts.

**Connection String** (from `appsettings.json`):

```json
"ConnectionStrings": {
  "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=AssetVestDb;Username=postgres;Password=postgres;Include Error Detail=true;Timeout=30"
}
```

**Important**: Use `127.0.0.1` instead of `localhost` on Windows to avoid IPv6 connection issues. The Npgsql driver may try to connect to `[::1]:5432` (IPv6 loopback) which can cause timeouts on some systems.

### Managing Docker Services

```powershell
# Start services
docker-compose up -d

# Stop services (preserves data)
docker-compose stop

# Stop and remove containers (preserves data in volumes)
docker-compose down

# Stop and DELETE all data (WARNING: destructive)
docker-compose down -v

# View logs
docker-compose logs -f postgres
docker-compose logs -f seq

# Restart single service
docker-compose restart postgres
```

---

## Migration Workflow

### Understanding EF Core Migrations

**Migrations = Version Control for Database Schema**

- Each migration represents a set of schema changes
- Migrations are applied sequentially
- `__EFMigrationsHistory` table tracks which migrations have been applied

### Development Workflow

#### Step 1: Modify Entity Models

```csharp
// Example: Add a new property to User entity
public class User : AuditableEntity
{
    // ... existing properties
    public string? PhoneNumber { get; set; } // NEW
}
```

#### Step 2: Create Migration

```powershell
dotnet ef migrations add AddPhoneNumberToUser `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api `
  --output-dir Persistence/Migrations

# Generates 3 files:
# - TIMESTAMP_AddPhoneNumberToUser.cs           (Up/Down methods)
# - TIMESTAMP_AddPhoneNumberToUser.Designer.cs  (Metadata)
# - ApplicationDbContextModelSnapshot.cs        (Updated model)
```

#### Step 3: Review Generated Migration

```csharp
// TIMESTAMP_AddPhoneNumberToUser.cs
protected override void Up(MigrationBuilder migrationBuilder)
{
    migrationBuilder.AddColumn<string>(
        name: "PhoneNumber",
        table: "Users",
        type: "text",
        nullable: true);
}

protected override void Down(MigrationBuilder migrationBuilder)
{
    migrationBuilder.DropColumn(
        name: "PhoneNumber",
        table: "Users");
}
```

**Always review migrations before applying!**

#### Step 4: Apply Migration

```powershell
# Apply to local database
dotnet ef database update `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api

# Or apply specific migration
dotnet ef database update AddPhoneNumberToUser `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api
```

#### Step 5: Rollback if Needed

```powershell
# Rollback to previous migration
dotnet ef database update PreviousMigrationName `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api

# Rollback to initial state (empty database)
dotnet ef database update 0 `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api

# Remove last migration (only if NOT applied to database)
dotnet ef migrations remove `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api
```

### Migration Best Practices

#### ✅ Do

- **Small, focused migrations**: One logical change per migration
- **Descriptive names**: `AddEmailIndexToUsers`, not `Update1`
- **Review SQL**: Always check what EF generates
- **Test rollback**: Ensure `Down()` method works
- **Commit migrations**: Check into version control
- **Document breaking changes**: Add comments in migration file

#### ❌ Don't

- **Modify applied migrations**: Create a new migration instead
- **Delete migration files**: Use `ef migrations remove` if not applied
- **Skip testing**: Always test on local database first
- **Ignore warnings**: Address soft delete filter warnings if needed

### Handling Data Migrations

For migrations that require data transformation:

```csharp
protected override void Up(MigrationBuilder migrationBuilder)
{
    // 1. Add new column
    migrationBuilder.AddColumn<string>(
        name: "FullName",
        table: "Users",
        nullable: true);

    // 2. Migrate data
    migrationBuilder.Sql(@"
        UPDATE ""Users""
        SET ""FullName"" = ""FirstName"" || ' ' || ""LastName""
        WHERE ""FullName"" IS NULL;
    ");

    // 3. Make column required
    migrationBuilder.AlterColumn<string>(
        name: "FullName",
        table: "Users",
        nullable: false);
}
```

---

## Production Deployment

### Strategy: SQL Script Generation (Recommended)

**Why?** Production databases often have restricted access. DBA teams prefer reviewing and applying SQL scripts.

#### Step 1: Generate Idempotent SQL Script

```powershell
# Generate script for all pending migrations
dotnet ef migrations script `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api `
  --idempotent `
  --output deploy/migrations.sql

# Or for specific range
dotnet ef migrations script FromMigration ToMigration `
  --idempotent `
  --output deploy/migrations-v2.sql
```

**Idempotent** = Safe to run multiple times (uses `IF NOT EXISTS` checks)

#### Step 2: Review SQL Script

```sql
-- Example generated SQL
IF NOT EXISTS(SELECT * FROM [__EFMigrationsHistory] WHERE [MigrationId] = N'20260531105857_InitialCreate')
BEGIN
    CREATE TABLE "Users" (
        "Id" uuid NOT NULL,
        "FirstName" text NOT NULL,
        -- ... more columns
        CONSTRAINT "PK_Users" PRIMARY KEY ("Id")
    );
END;
```

#### Step 3: Provide to DBA/DevOps

- Include rollback plan
- Document breaking changes
- Specify maintenance window requirements
- Provide data backup verification steps

### Alternative: Runtime Migration (CI/CD)

For environments with automated deployments:

```csharp
// Program.cs (AssetVest.Api)
var app = builder.Build();

// Apply migrations on startup (production)
if (app.Environment.IsProduction())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    
    // Apply pending migrations
    await dbContext.Database.MigrateAsync();
}
```

**Pros**: Fully automated
**Cons**: Requires app to have database CREATE/ALTER permissions (security risk)

**Recommended**: Use for staging/QA, not production.

### Production Connection String

**Azure App Service Configuration**:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=assetvest-prod.postgres.database.azure.com;Port=5432;Database=AssetVestDb;Username=assetvestadmin@assetvest-prod;Password=***;SslMode=Require"
  }
}
```

**Best Practices**:
- ✅ Store in Azure Key Vault or App Configuration
- ✅ Use Managed Identity for authentication (Azure)
- ✅ Enable SSL/TLS (`SslMode=Require`)
- ✅ Separate read/write connection strings for scaling
- ❌ Never commit production credentials to source control

### Zero-Downtime Deployments

For breaking schema changes:

#### Approach 1: Expand-Contract Pattern

**Phase 1: Expand** (Deploy Week 1)
```sql
-- Add new column (nullable)
ALTER TABLE "Users" ADD COLUMN "Email2" text NULL;

-- Dual-write to both columns in application code
```

**Phase 2: Migrate Data** (Background job)
```sql
UPDATE "Users" SET "Email2" = "Email" WHERE "Email2" IS NULL;
```

**Phase 3: Contract** (Deploy Week 2)
```sql
-- Make new column required
ALTER TABLE "Users" ALTER COLUMN "Email2" SET NOT NULL;

-- Drop old column
ALTER TABLE "Users" DROP COLUMN "Email";

-- Rename new column
ALTER TABLE "Users" RENAME COLUMN "Email2" TO "Email";
```

#### Approach 2: Blue-Green Database

- Maintain two database instances
- Apply migration to "green" instance
- Switch app to "green" instance
- Fallback to "blue" if issues arise

---

## Common Commands

### Migration Commands

```powershell
# Create new migration
dotnet ef migrations add MigrationName `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api `
  --output-dir Persistence/Migrations

# Apply all pending migrations
dotnet ef database update `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api

# Apply specific migration
dotnet ef database update MigrationName `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api

# Rollback to migration
dotnet ef database update PreviousMigrationName `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api

# Rollback all migrations (empty database)
dotnet ef database update 0 `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api

# Remove last migration (if not applied)
dotnet ef migrations remove `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api

# List all migrations
dotnet ef migrations list `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api

# Generate SQL script
dotnet ef migrations script `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api `
  --idempotent `
  --output migrations.sql

# Generate SQL for specific range
dotnet ef migrations script FromMigration ToMigration `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api `
  --output partial.sql
```

### Database Inspection Commands

```powershell
# View DbContext information
dotnet ef dbcontext info `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api

# View DbContext details (verbose)
dotnet ef dbcontext info -v `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api

# Generate C# entity classes from existing database (reverse engineering)
dotnet ef dbcontext scaffold "Host=localhost;Database=AssetVestDb;Username=postgres;Password=postgres" `
  Npgsql.EntityFrameworkCore.PostgreSQL `
  --output-dir Models `
  --context-dir Data `
  --context AssetVestContext
```

### Docker Database Commands

```powershell
# Connect to PostgreSQL CLI
docker exec -it assetvest-postgres psql -U postgres -d AssetVestDb

# List databases
docker exec -it assetvest-postgres psql -U postgres -c "\l"

# List tables
docker exec -it assetvest-postgres psql -U postgres -d AssetVestDb -c "\dt"

# Describe table structure
docker exec -it assetvest-postgres psql -U postgres -d AssetVestDb -c "\d \"Users\""

# Count records
docker exec -it assetvest-postgres psql -U postgres -d AssetVestDb -c "SELECT COUNT(*) FROM \"Users\";"

# Backup database
docker exec assetvest-postgres pg_dump -U postgres AssetVestDb > backup_$(Get-Date -Format 'yyyyMMdd_HHmmss').sql

# Restore database
Get-Content backup.sql | docker exec -i assetvest-postgres psql -U postgres -d AssetVestDb

# Drop database (recreate fresh)
docker exec -it assetvest-postgres psql -U postgres -c "DROP DATABASE \"AssetVestDb\";"
docker exec -it assetvest-postgres psql -U postgres -c "CREATE DATABASE \"AssetVestDb\";"
```

---

## Troubleshooting

### Issue 1: Connection Timeout (IPv6/IPv4)

**Error**:
```
Npgsql.NpgsqlException: The operation has timed out
Failed to connect to [::1]:5432
```

**Cause**: On Windows, `localhost` may resolve to IPv6 `[::1]` which causes connection issues with Docker.

**Solution**:
```json
// Change in appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Host=127.0.0.1;Port=5432;..."  // Use 127.0.0.1, not localhost
}
```

### Issue 2: Seq Authentication Error

**Error**:
```
System.InvalidOperationException: No default admin password was supplied;
set `SEQ_FIRSTRUN_ADMINPASSWORD`
```

**Solution**:
```yaml
# docker-compose.yml
seq:
  environment:
    SEQ_FIRSTRUN_ADMINPASSWORD: Admin123!  # Add this line
```

Default credentials: **admin / Admin123!**

### Issue 3: Connection Refused (Port 5432)

**Error**:
```
Failed to connect to [::1]:5432
No connection could be made because the target machine actively refused it.
```

**Solution**:
```powershell
# Check if PostgreSQL container is running
docker ps | Select-String "assetvest-postgres"

# If not running, start it
docker-compose up -d postgres

# Wait for initialization
Start-Sleep -Seconds 10

# Test connection
docker exec assetvest-postgres pg_isready -U postgres
```

### Issue 2: Migration Already Applied

**Error**:
```
The migration '20260531105857_InitialCreate' has already been applied to the database.
```

**Solution**:
```powershell
# Check applied migrations
dotnet ef migrations list --project src/AssetVest.Infrastructure --startup-project src/AssetVest.Api

# If you want to reapply:
# 1. Rollback first
dotnet ef database update 0 --project src/AssetVest.Infrastructure --startup-project src/AssetVest.Api

# 2. Then apply again
dotnet ef database update --project src/AssetVest.Infrastructure --startup-project src/AssetVest.Api
```

### Issue 3: Pending Model Changes

**Error**:
```
The model has changed since the database was created.
```

**Solution**:
```powershell
# Create migration for the changes
dotnet ef migrations add PendingChanges `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api

# Review and apply
dotnet ef database update `
  --project src/AssetVest.Infrastructure `
  --startup-project src/AssetVest.Api
```

### Issue 4: EF Tools Version Mismatch

**Warning**:
```
The Entity Framework tools version '10.0.0' is older than that of the runtime '10.0.8'.
```

**Solution**:
```powershell
# Update EF tools globally
dotnet tool update -g dotnet-ef

# Verify version
dotnet ef --version
# Should show: 10.0.8 or higher
```

### Issue 5: Soft Delete Filter Warnings

**Warning**:
```
Entity 'Asset' has a global query filter defined and is the required end
of a relationship with the entity 'StockDetail'
```

**Explanation**: This warning occurs because `Asset` has a soft delete filter, but `StockDetail` doesn't. When an `Asset` is soft-deleted, the relationship to `StockDetail` becomes orphaned.

**This is expected behavior** in the current design. Detail entities are deleted via cascade when their parent `Asset` is hard-deleted or soft-deleted.

**To suppress warnings** (optional):
```csharp
// Add to each detail entity configuration
modelBuilder.Entity<StockDetail>()
    .HasQueryFilter(sd => sd.Asset != null && !sd.Asset.IsDeleted);
```

### Issue 6: Database Out of Sync

**Symptom**: Application throws SQL errors about missing columns/tables.

**Solution**:
```powershell
# 1. Check pending migrations
dotnet ef migrations list --project src/AssetVest.Infrastructure --startup-project src/AssetVest.Api

# 2. Apply all pending
dotnet ef database update --project src/AssetVest.Infrastructure --startup-project src/AssetVest.Api

# 3. If corrupted, reset database
docker-compose down -v  # WARNING: Deletes all data
docker-compose up -d
Start-Sleep -Seconds 10
dotnet ef database update --project src/AssetVest.Infrastructure --startup-project src/AssetVest.Api
```

### Issue 7: Docker Volume Permissions (Linux/macOS)

**Error**: Permission denied when writing to Docker volumes.

**Solution**:
```bash
# Fix volume ownership
docker-compose down
sudo chown -R $USER:$USER ~/.docker/volumes/
docker-compose up -d
```

---

## Appendix: Initial Migration Details

**Migration**: `20260531105857_InitialCreate`  
**Created**: May 31, 2026 at 10:58:57 AM  
**Size**: 31,747 bytes

### Tables Created

| Table | Columns | Indexes | Foreign Keys |
|-------|---------|---------|--------------|
| Users | 9 | PK, Email | - |
| RefreshTokens | 7 | PK, UserId, ExpiresAt | Users |
| Assets | 14 | PK, UserId, AssetType | Users |
| StockDetail | 6 | PK, AssetId | Assets |
| CurrencyDetail | 4 | PK, AssetId | Assets |
| GoldDetail | 5 | PK, AssetId | Assets |
| RealEstateDetail | 6 | PK, AssetId | Assets |
| MutualFundDetail | 5 | PK, AssetId | Assets |
| CryptoDetail | 5 | PK, AssetId | Assets |
| BondsDetail | 7 | PK, AssetId | Assets |
| AssetValueHistory | 5 | PK, AssetId, RecordedAt | Assets |
| AnnualGoals | 10 | PK, UserId, Year | Users |
| AssetTypeAllocationGoals | 5 | PK, AnnualGoalId | AnnualGoals |
| StockProfitGoals | 9 | PK, UserId | Users |
| FxRates | 6 | PK, CurrencyPair, EffectiveDate | - |
| FxRateHistory | 5 | PK, CurrencyPair, RecordedAt | - |
| AuditLogs | 7 | PK, UserId, Timestamp | - |

**Total Objects**: 17 tables, 17 primary keys, 11 foreign keys, 24 indexes

---

## Version History

| Date | Migration | Changes |
|------|-----------|---------|
| 2026-05-31 | InitialCreate | Initial schema with 17 tables |

---

**Last Updated**: May 31, 2026  
**Maintained By**: AssetVest Development Team
