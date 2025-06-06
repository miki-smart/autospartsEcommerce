# Basket Service

Handles shopping cart operations for the AutoParts Ecommerce platform.

## Features
- Add/remove/update items in basket
- Redis persistence
- Integrated with IdentityServer for authentication/authorization

## Running Locally
```sh
dotnet run --project Basket.API.csproj
```

## Docker Support
Build and run with Docker:
```sh
docker build -t autoparts-basket .
docker run -p 5300:5300 autoparts-basket
```

## Configuration
- Edit `appsettings.json` for Redis connection and service settings.
