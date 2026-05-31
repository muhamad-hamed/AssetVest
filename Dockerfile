# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY src/AssetVest.Domain/AssetVest.Domain.csproj ./src/AssetVest.Domain/
COPY src/AssetVest.Application/AssetVest.Application.csproj ./src/AssetVest.Application/
COPY src/AssetVest.Infrastructure/AssetVest.Infrastructure.csproj ./src/AssetVest.Infrastructure/
COPY src/AssetVest.Api/AssetVest.Api.csproj ./src/AssetVest.Api/

# Restore dependencies (restore each project to avoid solution file issues)
RUN dotnet restore ./src/AssetVest.Api/AssetVest.Api.csproj

# Copy all source files
COPY src/ ./src/

# Build and publish
WORKDIR /src/src/AssetVest.Api
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development

# Healthcheck
HEALTHCHECK --interval=30s --timeout=3s --start-period=10s --retries=3 \
  CMD curl --fail http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "AssetVest.Api.dll"]
