# Catalog Service

Handles product catalog management for the AutoParts Ecommerce platform.

## Features
- CRUD for products and categories
- CQRS and DDD architecture
- PostgreSQL persistence
- Integrated with IdentityServer for authentication/authorization

## Running Locally
```sh
dotnet run --project Catalog.API.csproj
```

## Docker Support
Build and run with Docker:
```sh
docker build -t autoparts-catalog .
docker run -p 5200:5200 autoparts-catalog
```

## Configuration
- Edit `appsettings.json` for DB connection and service settings.
