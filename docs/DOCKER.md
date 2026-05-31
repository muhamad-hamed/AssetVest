# Running AssetVest API with Docker Compose

Complete guide for running the entire AssetVest stack with Docker Compose.

## 🐳 What Gets Containerized

The docker-compose setup runs **3 services**:

1. **PostgreSQL 17** - Database on port 5432
2. **Seq** - Logging server on port 5341
3. **AssetVest API** - .NET API on port 5000

All services communicate on a private `assetvest-network` bridge network.

---

## 🚀 Quick Start

### Start All Services

```powershell
# Build and start all containers
docker-compose up -d --build

# View logs
docker-compose logs -f

# View API logs only
docker-compose logs -f api
```

The API will be available at: **http://localhost:5000**

### Stop All Services

```powershell
# Stop containers (keeps data)
docker-compose stop

# Stop and remove containers (keeps data)
docker-compose down

# Remove everything including data volumes
docker-compose down -v
```

---

## 📋 Service Details

### API Service (`assetvest-api`)

- **Port**: 5000 → 8080 (internal)
- **Health Check**: http://localhost:5000/health
- **Swagger**: http://localhost:5000/swagger (if enabled)
- **Auto-runs migrations** on startup via `docker-entrypoint.sh`
- **Depends on**: PostgreSQL (waits for healthy status)

### PostgreSQL Service (`assetvest-postgres`)

- **Port**: 5432
- **Database**: AssetVestDb
- **Credentials**: postgres / postgres
- **Health Check**: `pg_isready` every 5 seconds
- **Volume**: `postgres_data` (persists across restarts)

### Seq Service (`assetvest-seq`)

- **Port**: 5341 → 80 (internal)
- **Web UI**: http://localhost:5341
- **Credentials**: admin / M#seq@2026
- **Volume**: `seq_data` (persists across restarts)

---

## 🔧 Configuration

### Environment Variables (API)

The API container receives these environment variables from docker-compose.yml:

```yaml
# Connection string uses service name "postgres" instead of localhost
ConnectionStrings__DefaultConnection: "Host=postgres;Port=5432;Database=AssetVestDb;..."

# Seq URL uses service name
Serilog__WriteTo__1__Args__serverUrl: "http://seq:80"

# JWT configuration
Jwt__SecretKey: "DEVELOPMENT-SECRET-KEY-MIN-32-CHARS-CHANGE-IN-PRODUCTION-12345"
Jwt__Issuer: "AssetVest.Api"
Jwt__Audience: "AssetVest.Client"
Jwt__AccessTokenExpirationMinutes: "15"
Jwt__RefreshTokenExpirationDays: "7"
```

### Network Configuration

All services run on `assetvest-network`:
- Services communicate using service names (`postgres`, `seq`, `api`)
- No need for `127.0.0.1` or `localhost` between containers
- Ports exposed to host: 5432, 5341, 5000

---

## 🔍 Verification

### Check Service Status

```powershell
# List running containers
docker-compose ps

# Check API health
curl http://localhost:5000/health

# Check database readiness
curl http://localhost:5000/health/ready
```

Expected output:
```
NAME                IMAGE                    STATUS        PORTS
assetvest-postgres  postgres:17-alpine       Up (healthy)  0.0.0.0:5432->5432/tcp
assetvest-seq       datalust/seq:latest      Up            0.0.0.0:5341->80/tcp
assetvest-api       assetvest-api            Up (healthy)  0.0.0.0:5000->8080/tcp
```

### View Logs

```powershell
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api
docker-compose logs -f postgres
docker-compose logs -f seq

# Last 50 lines
docker-compose logs --tail=50 api
```

### Database Migrations

Migrations run **automatically** when the API container starts. Look for:

```
Starting AssetVest API...
Waiting for PostgreSQL to be ready...
Build succeeded.
Database migrations applied successfully
Starting API server...
```

To manually run migrations:
```powershell
docker-compose exec api dotnet ef database update \
  --project /src/src/AssetVest.Infrastructure \
  --startup-project /app \
  --no-build
```

---

## 🧪 Testing with Postman

Use the **AssetVest API** Postman collection with Docker:

1. Import collection and environment from `docs/` folder
2. Environment is already configured for `http://localhost:5000`
3. Run **Authentication → Register** to create test user
4. All subsequent requests use the saved token automatically

No changes needed - the collection works with both local (`dotnet run`) and Docker deployments!

---

## 🐛 Troubleshooting

### Issue: API Container Keeps Restarting

**Check logs:**
```powershell
docker-compose logs api
```

**Common causes:**
1. **Database not ready**: API waits for PostgreSQL health check
2. **Migration failed**: Check database connection string
3. **Port conflict**: Port 5000 already in use

**Solution:**
```powershell
# Restart just the API
docker-compose restart api

# Rebuild from scratch
docker-compose down
docker-compose up -d --build
```

### Issue: "Connection refused" to PostgreSQL

**Cause**: Using `localhost` or `127.0.0.1` in connection string

**Solution**: Use service name `postgres`:
```
Host=postgres;Port=5432;Database=AssetVestDb;...
```

This is already configured in docker-compose.yml.

### Issue: Changes Not Reflected

**Cause**: Docker image cached

**Solution**: Rebuild images
```powershell
# Force rebuild
docker-compose up -d --build

# Or rebuild specific service
docker-compose build --no-cache api
docker-compose up -d api
```

### Issue: Database Data Lost

**Cause**: Volumes removed with `docker-compose down -v`

**Solution**: Use `docker-compose stop` or `docker-compose down` (without `-v`)

To backup data:
```powershell
# Export database
docker exec assetvest-postgres pg_dump -U postgres AssetVestDb > backup.sql

# Import database
docker exec -i assetvest-postgres psql -U postgres AssetVestDb < backup.sql
```

### Issue: Out of Disk Space

**Check Docker disk usage:**
```powershell
docker system df
```

**Clean up:**
```powershell
# Remove unused images
docker image prune -a

# Remove unused volumes
docker volume prune

# Nuclear option: clean everything
docker system prune -a --volumes
```

---

## 📊 Development Workflow

### Making Code Changes

```powershell
# 1. Edit your code
code src/AssetVest.Api/Controllers/

# 2. Rebuild and restart API
docker-compose up -d --build api

# 3. Watch logs
docker-compose logs -f api
```

### Adding New Migrations

```powershell
# Option 1: From host machine (recommended)
dotnet ef migrations add MigrationName \
  --project src/AssetVest.Infrastructure \
  --startup-project src/AssetVest.Api

# Restart API to apply
docker-compose restart api

# Option 2: Inside container
docker-compose exec api dotnet ef migrations add MigrationName \
  --project /src/src/AssetVest.Infrastructure \
  --startup-project /app
```

### Debugging Inside Container

```powershell
# Open shell in API container
docker-compose exec api bash

# Check environment variables
docker-compose exec api env

# Test database connection
docker-compose exec api dotnet ef database update --no-build
```

---

## 🔒 Production Considerations

⚠️ **This setup is for DEVELOPMENT only**

For production:

1. **Change Passwords**:
   ```yaml
   POSTGRES_PASSWORD: <strong-password>
   SEQ_FIRSTRUN_ADMINPASSWORD: <strong-password>
   Jwt__SecretKey: <256-bit-random-key>
   ```

2. **Use Environment Files**:
   ```powershell
   # Create .env file
   cp .env.example .env
   
   # Update docker-compose.yml
   env_file:
     - .env
   ```

3. **Remove Port Mappings**:
   ```yaml
   # Don't expose database externally
   postgres:
     expose:
       - "5432"  # Internal only
   ```

4. **Use Secrets Management**:
   - Azure Key Vault
   - AWS Secrets Manager
   - Kubernetes Secrets

5. **Enable HTTPS**:
   ```yaml
   api:
     environment:
       ASPNETCORE_URLS: https://+:443;http://+:80
     ports:
       - "443:443"
       - "80:80"
     volumes:
       - ./certs:/https:ro
   ```

6. **Add Reverse Proxy** (Nginx, Traefik):
   - SSL termination
   - Load balancing
   - Rate limiting

---

## 📁 File Structure

```
AssetVest/
├── Dockerfile                   # Multi-stage .NET build
├── docker-compose.yml           # Service definitions
├── docker-entrypoint.sh         # Startup script with migrations
├── .dockerignore               # Optimize build context
└── docs/
    └── DOCKER.md               # This file
```

---

## 🔗 Related Documentation

- [Database Setup](DATABASE.md) - Migration commands
- [Postman Collection](POSTMAN.md) - API testing
- [Setup Summary](SETUP_SUMMARY.md) - Project overview

---

## 📝 Common Commands

```powershell
# Start everything
docker-compose up -d

# View all logs
docker-compose logs -f

# Restart API only
docker-compose restart api

# Rebuild API from scratch
docker-compose up -d --build api

# Stop everything
docker-compose down

# Execute command in container
docker-compose exec api bash

# View database tables
docker-compose exec postgres psql -U postgres -d AssetVestDb -c "\dt"

# Check health
curl http://localhost:5000/health

# View API environment
docker-compose exec api env | grep -i connection
```

---

**Last Updated**: May 31, 2026  
**Docker Compose Version**: 3.8+  
**.NET Version**: 10.0  
**PostgreSQL Version**: 17-alpine
