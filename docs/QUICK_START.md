# AssetVest API - Quick Start Guide

## ✅ Current Status

All services are **running successfully** in Docker!

### Running Services

| Service | Container | Port | Status |
|---------|-----------|------|--------|
| API | assetvest-api | **5062** | ✅ Healthy |
| PostgreSQL | assetvest-postgres | 5432 | ✅ Healthy |
| Seq Logs | assetvest-seq | 5341 | ✅ Running |

### Base URL

**http://localhost:5062**

## 🚀 Quick Commands

### Start/Stop Services

```powershell
# Start all services
docker-compose up -d

# Stop all services
docker-compose stop

# Stop and remove containers (keeps data)
docker-compose down

# View logs
docker-compose logs -f api

# Check status
docker-compose ps
```

### Test the API

```powershell
# Health check
curl http://localhost:5062/health

# Test endpoint (returns 401 without auth - this is correct!)
curl http://localhost:5062/api/v1/users
```

## 📝 Postman Collection

Your Postman collection is ready to use!

**Files:**
- `docs/AssetVest-API.postman_collection.json` - Complete API collection (24 endpoints)
- `docs/AssetVest-Local.postman_environment.json` - Environment variables

**Base URL:** Already configured to `http://localhost:5062` ✅

### How to Use

1. **Import to Postman**
   - Import both JSON files from the `docs/` folder
   - Select "AssetVest - Local" environment

2. **Start Testing**
   - Run **Authentication → Register** first
     - Uses your test data: Mohamed Hamed / muhamaad.hamed@gmail.com / H#test@2026
     - Automatically saves `accessToken`, `refreshToken`, and `userId`
   
3. **Test Other Endpoints**
   - All subsequent requests automatically use the saved token
   - Try **Users → Get Current User** (`/api/v1/users/me`)
   - Try **Assets → Create Asset - Stock**

## 📊 Database Status

**Database:** AssetVestDb  
**Tables:** 18 tables created ✅

```powershell
# View tables
docker-compose exec postgres psql -U postgres -d AssetVestDb -c "\dt"

# Connect to database
docker-compose exec postgres psql -U postgres -d AssetVestDb
```

## 🔐 Test User Data

**From Postman Register Request:**
```json
{
  "firstName": "Mohamed",
  "lastName": "Hamed",
  "email": "muhamaad.hamed@gmail.com",
  "password": "H#test@2026"
}
```

## 🧪 Testing Workflow

### Using Docker (Current Setup)

```powershell
# 1. Ensure services are running
docker-compose ps

# 2. Use Postman collection
#    - Import collection and environment
#    - Run Authentication → Register
#    - Test other endpoints

# 3. View logs in Seq
#    Open: http://localhost:5341
#    Login: admin / M#seq@2026
```

### Using Local Development

```powershell
# 1. Start infrastructure only
docker-compose up -d postgres seq

# 2. Run API locally
dotnet run --project src/AssetVest.Api

# 3. API will be on same port: http://localhost:5062
```

## 🎯 Port Reference

| What | Local Dev | Docker | Notes |
|------|-----------|--------|-------|
| **API** | 5062 | 5062 | Same port for both! ✅ |
| PostgreSQL | 5432 | 5432 | Shared database |
| Seq | 5341 | 5341 | Shared logs |

## 📁 Important Files

```
AssetVest/
├── docker-compose.yml              # Service definitions
├── Dockerfile                      # API container build
├── docs/
│   ├── AssetVest-API.postman_collection.json
│   ├── AssetVest-Local.postman_environment.json
│   ├── DATABASE.md                 # Database guide
│   ├── DOCKER.md                   # Docker guide
│   ├── POSTMAN.md                  # Postman guide
│   └── QUICK_START.md              # This file
└── src/AssetVest.Api/
    ├── appsettings.json            # Config (JWT, Seq, CORS)
    └── Properties/launchSettings.json # Port: 5062
```

## 🐛 Troubleshooting

### Issue: Can't connect to API

**Check:**
```powershell
# Is it running?
docker-compose ps

# Check logs
docker-compose logs api

# Restart if needed
docker-compose restart api
```

### Issue: Database connection error

**Solution:**
```powershell
# Check PostgreSQL
docker-compose logs postgres

# Verify healthy
docker-compose ps
# Should show: "Up X seconds (healthy)"
```

### Issue: 401 Unauthorized on all requests

**Solution:**
- Run **Authentication → Register** or **Login** in Postman first
- Check that `{{accessToken}}` variable is populated
- Access tokens expire after 15 minutes - use **Refresh Token** request

## 📚 Full Documentation

- **[DATABASE.md](DATABASE.md)** - Database setup, migrations, troubleshooting
- **[DOCKER.md](DOCKER.md)** - Docker commands, configuration, production tips
- **[POSTMAN.md](POSTMAN.md)** - Complete Postman collection guide
- **[SETUP_SUMMARY.md](SETUP_SUMMARY.md)** - Project overview and status

## ✨ Summary

You now have:
- ✅ Complete API running in Docker on port **5062**
- ✅ PostgreSQL with 18 tables migrated
- ✅ Seq logging at http://localhost:5341
- ✅ Postman collection with 24 endpoints
- ✅ Test user data ready to go

**Next step:** Open Postman and run **Authentication → Register**! 🚀

---

**Last Updated:** May 31, 2026  
**Stack:** .NET 10 / PostgreSQL 17 / Docker  
**API Version:** v1.0
