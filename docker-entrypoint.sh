#!/bin/bash
set -e

echo "Starting AssetVest API..."

# Wait for database to be ready
echo "Waiting for PostgreSQL to be ready..."
until dotnet ef database update --project /src/src/AssetVest.Infrastructure --startup-project /app --no-build 2>/dev/null; do
  echo "Database migration failed, retrying in 5 seconds..."
  sleep 5
done

echo "Database migrations applied successfully"

# Start the application
echo "Starting API server..."
exec dotnet AssetVest.Api.dll
