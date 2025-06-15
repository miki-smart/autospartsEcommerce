# ApiGateway

The API Gateway is a robust, scalable, and configurable routing solution for the AutoParts Ecommerce platform's microservices architecture. Built with Ocelot and enhanced with enterprise-grade features for production use.

## Features

### Core Routing & Service Discovery
- **Ocelot-based routing** with comprehensive configuration for all backend microservices
- **Consul service discovery** for dynamic service registration and health monitoring
- **Load balancing** with multiple algorithms (Round Robin, Least Connection, etc.)
- **Circuit breaking** with Polly for resilience and fault tolerance

### Security & Authentication
- **JWT authentication/authorization** with configurable validation
- **CORS policy** management with configurable allowed origins
- **IP-based rate limiting** to prevent abuse and ensure fair usage

### Monitoring & Observability
- **Custom Correlation ID middleware** for distributed tracing across microservices
- **Centralized logging** with Serilog (Console, File, and Seq sinks)
- **Comprehensive health checks** for all services and dependencies
- **Health Checks UI** with real-time monitoring dashboard

### Performance & Reliability
- **Rate limiting** with memory-based storage
- **Circuit breaker patterns** for service resilience
- **Request/response transformation** capabilities

## Custom Correlation ID Middleware

The API Gateway includes a custom-built correlation ID middleware that replaces the third-party `CorrelationId` package. This provides:

- **Automatic correlation ID generation** for all incoming requests
- **Header propagation** - forwards existing correlation IDs from upstream services
- **Serilog integration** - automatically includes correlation ID in all log entries
- **HTTP response headers** - returns correlation ID to clients for tracking
- **HttpContext extension methods** - easy access to correlation ID throughout the request pipeline

### How It Works
1. **Incoming Request**: Checks for existing `X-Correlation-ID` header
2. **ID Generation**: Creates new correlation ID if none exists (12-character format)
3. **Context Storage**: Stores ID in `HttpContext.Items` for pipeline access
4. **Response Headers**: Adds correlation ID to response headers
5. **Logging Context**: Pushes correlation ID to Serilog context for automatic inclusion in logs

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
