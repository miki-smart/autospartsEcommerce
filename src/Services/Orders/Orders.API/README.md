# Orders Service

Handles order management for the AutoParts Ecommerce platform.

## Features
- Create, update, and query orders
- CQRS and DDD architecture
- PostgreSQL persistence
- Integrated with IdentityServer for authentication/authorization

## Running Locally
```sh
dotnet run --project Orders.API.csproj
```

## Docker Support
Build and run with Docker:
```sh
docker build -t autoparts-orders .
docker run -p 5100:5100 autoparts-orders
```

## Configuration
- Edit `appsettings.json` for DB connection and service settings.
