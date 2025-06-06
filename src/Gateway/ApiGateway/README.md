# ApiGateway

The API Gateway routes and secures traffic to all backend microservices in the AutoParts Ecommerce platform.

## Features
- Centralized routing for all APIs (Orders, Catalog, Basket, Payment, Notification, Search, etc.)
- JWT authentication/authorization (integrates with IdentityServer)
- Request/response transformation
- Service discovery (if enabled)

## How to Run

1. **Configure Routing:**
   - Update `appsettings.Development.json` with backend service URLs (Orders, Catalog, Basket, Payment, Notification, Search, etc.).
   - Ensure `IdentityServer` URL is set for authentication.
2. **Run the Service:**
   - `dotnet run`

## Docker Support

### Build and Run
```powershell
cd src/Gateway/ApiGateway
# docker build -t autoparts-apigateway .
# docker run -p 5000:80 --env-file .env autoparts-apigateway
```

### Environment Variables
- `ASPNETCORE_ENVIRONMENT` (e.g., Development)
- Service URLs as needed (see `appsettings.json`)

## Configuration Required
- `appsettings.json` must contain correct URLs for all backend services and IdentityServer.
- JWT settings must match IdentityServer configuration.

## Functionality
- Entry point for all client and frontend requests
- Handles authentication and forwards user claims
- Routes requests to appropriate microservice
- Can perform request/response transformation and aggregation

## Notes
- Ensure IdentityServer is running and accessible for authentication.
- See `appsettings.json` for route configuration.
