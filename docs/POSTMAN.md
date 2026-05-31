# AssetVest API - Postman Collection

Complete Postman collection for testing all AssetVest API endpoints.

## 📦 Files

- **AssetVest-API.postman_collection.json** - Complete API collection with 20+ requests
- **AssetVest-Local.postman_environment.json** - Local development environment

## 🚀 Quick Start

### 1. Import into Postman

1. Open Postman
2. Click **Import** button (top left)
3. Drag and drop both files:
   - `AssetVest-API.postman_collection.json`
   - `AssetVest-Local.postman_environment.json`
4. Select **AssetVest - Local** environment (top right dropdown)

### 2. Start the API Server

```powershell
# Start Docker services (if not running)
docker-compose up -d

# Run the API
dotnet run --project src/AssetVest.Api
```

API should be running at: **http://localhost:5000**

### 3. Test the Flow

#### Step 1: Register a New User
- Folder: **Authentication → Register**
- This will automatically save the `accessToken`, `refreshToken`, and `userId` to environment variables

#### Step 2: Test Authenticated Endpoints
All other requests will use the saved `accessToken` automatically via Bearer authentication.

---

## 📋 Collection Overview

### Authentication (3 requests)
- ✅ **Register** - Create new account
- ✅ **Login** - Authenticate with email/password  
- ✅ **Refresh Token** - Get new access token

### Users (9 requests)
- Get All Users
- Get Current User (`/me`)
- Get User by ID
- Get User by Email
- Create User
- Update User
- Change Password
- Toggle Active Status
- Delete User (soft delete)

### Assets (10 requests)
- Get All Assets
- Get Asset by ID
- Get Assets by Type
- **Create Asset - Stock** (example with stock details)
- **Create Asset - Foreign Currency**
- **Create Asset - Gold**
- **Create Asset - Real Estate**
- **Create Asset - Cryptocurrency**
- Update Asset
- Delete Asset (soft delete)

### Health (2 requests)
- Health Check (`/health`)
- Readiness Check (`/health/ready`)

---

## 🔐 Authentication Flow

### Automatic Token Management

The collection is configured to automatically:
1. ✅ Extract `accessToken` from Register/Login responses
2. ✅ Save it to environment variable
3. ✅ Use it in all subsequent requests (Bearer token)
4. ✅ Extract `userId` for user-specific operations
5. ✅ Extract `assetId` when creating assets

### Test Scripts Included

Each auth request has **Test Scripts** that:
```javascript
// Register/Login response
{
  "accessToken": "eyJhbGc...",      // Saved to {{accessToken}}
  "refreshToken": "base64string",  // Saved to {{refreshToken}}
  "expiresIn": 900,
  "user": {
    "id": "guid",                  // Saved to {{userId}}
    "email": "muhamaad.hamed@gmail.com",
    "firstName": "Mohamed",
    "lastName": "Hamed"
  }
}
```

### Token Expiration

- **Access Token**: 15 minutes (900 seconds)
- **Refresh Token**: 7 days

When access token expires, use **Refresh Token** request to get a new one.

---

## 📝 Test Data Reference

### Default User (from Registration)
```json
{
  "firstName": "Mohamed",
  "lastName": "Hamed",
  "email": "muhamaad.hamed@gmail.com",
  "password": "H#test@2026"
}
```

### Asset Types Supported
```
Stocks
ForeignCurrency
Gold
RealEstate
Crypto
Bonds
Cash
MutualFunds
Other
```

### Sample Assets Included

1. **Stock** - Apple Inc. (AAPL) with symbol, exchange, quantity
2. **Foreign Currency** - USD holdings with amount
3. **Gold** - Physical gold with weight and purity
4. **Real Estate** - Apartment with location and size
5. **Cryptocurrency** - Bitcoin (BTC) with quantity

Each asset type has type-specific details attached.

---

## 🧪 Testing Workflow

### Recommended Testing Order

1. **Health Check** - Verify API is running
2. **Register** - Create your account (saves tokens automatically)
3. **Get Current User** (`/me`) - Verify authentication
4. **Create Assets** - Add various asset types
5. **Get All Assets** - See your portfolio
6. **Update/Delete** - Test CRUD operations

### Variables Reference

| Variable | Set By | Used By |
|----------|--------|---------|
| `{{baseUrl}}` | Environment | All requests |
| `{{accessToken}}` | Register/Login | All authenticated endpoints |
| `{{refreshToken}}` | Register/Login | Refresh Token request |
| `{{userId}}` | Register/Login | User-specific operations |
| `{{assetId}}` | Create Asset | Update/Delete Asset |

---

## 🔍 Testing Tips

### 1. Check Response Status
- ✅ 200 OK - Success
- ✅ 201 Created - Resource created
- ✅ 204 No Content - Success (no body)
- ❌ 400 Bad Request - Validation error
- ❌ 401 Unauthorized - Missing/invalid token
- ❌ 404 Not Found - Resource doesn't exist
- ❌ 429 Too Many Requests - Rate limit exceeded

### 2. Rate Limiting
- **Auth endpoints**: 5 requests per minute
- **API endpoints**: 100 requests per minute

If you hit rate limit, wait 60 seconds or restart the API.

### 3. Password Requirements
Must contain:
- Minimum 8 characters
- At least 1 uppercase letter
- At least 1 lowercase letter
- At least 1 number
- At least 1 special character

### 4. Soft Deletes
Delete operations set `isDeleted` flag, they don't remove records from database.

---

## 🐛 Troubleshooting

### Issue: "401 Unauthorized" on all requests

**Solution**: 
1. Run **Register** or **Login** request first
2. Check that `{{accessToken}}` variable is populated (Environment tab)
3. Access tokens expire after 15 minutes - use **Refresh Token** request

### Issue: "404 Not Found"

**Solution**: 
- Verify API is running: `http://localhost:5000/health`
- Check `{{baseUrl}}` in environment matches your API URL
- Ensure database migrations are applied

### Issue: "429 Too Many Requests"

**Solution**: 
- Wait 60 seconds for rate limit window to reset
- Or restart the API to reset counters

### Issue: Asset creation fails

**Solution**: 
- Ensure you're providing correct `assetType` (case-sensitive)
- For type-specific assets, include the detail object:
  - `Stocks` → `stockDetail`
  - `ForeignCurrency` → `currencyDetail`
  - `Gold` → `goldDetail`
  - Etc.

---

## 📊 Expected Responses

### Successful Register/Login
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "refreshToken": "uYW5kb20gc3RyaW5nIGhlcmU=...",
  "expiresIn": 900,
  "user": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "firstName": "Mohamed",
    "lastName": "Hamed",
    "email": "muhamaad.hamed@gmail.com",
    "isActive": true,
    "createdAt": "2026-05-31T14:30:00Z"
  }
}
```

### Validation Error
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "Validation Error",
  "status": 400,
  "detail": "Password must contain at least one uppercase letter, Password must contain at least one number"
}
```

---

## 🔗 Related Documentation

- [Database Setup Guide](DATABASE.md)
- [Setup Summary](SETUP_SUMMARY.md)
- [API Documentation](../src/AssetVest.Api/README.md) - Swagger UI when running

---

## 📧 Support

For API issues:
1. Check health endpoint: `GET /health`
2. View Seq logs: http://localhost:5341 (admin / Admin123!)
3. Check database: `docker exec -it assetvest-postgres psql -U postgres -d AssetVestDb`

---

**Last Updated**: May 31, 2026  
**API Version**: v1.0  
**Total Endpoints**: 24 (3 auth + 9 users + 10 assets + 2 health)
