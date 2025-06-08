# AutoParts Services - Docker Integration Setup

This document provides instructions for running the complete AutoParts microservices architecture using Docker Compose.

## Prerequisites

- Docker Desktop installed and running
- .NET 8.0 SDK (for local development)
- PowerShell (for running scripts)

## Architecture Overview

The system consists of the following services:

### Infrastructure Services
- **PostgreSQL**: Database for all services
- **Consul**: Service discovery and configuration
- **Redis**: Caching (optional)

### Microservices
- **API Gateway** (Port 7000): Ocelot-based gateway with service management UI
- **Identity API** (Port 5001): Authentication and authorization service
- **Catalog API** (Port 5002): Product catalog management
- **Orders API** (Port 5003): Order processing and management

## Quick Start

### 1. Build and Start All Services

```powershell
# Navigate to the project root
cd C:\Users\mikiy\autoparts-services

# Start all services in detached mode
.\scripts\manage-services.ps1 -Action up -Detached
```

### 2. Verify Services are Running

```powershell
# Check service status
.\scripts\manage-services.ps1 -Action status

# Run integration tests
.\scripts\test-integration.ps1
```

### 3. Access Service Endpoints

- **API Gateway**: http://localhost:7000
- **API Gateway Health UI**: http://localhost:7000/health-ui
- **Service Management UI**: http://localhost:7000/api/servicemanagement
- **Identity API**: http://localhost:5001
- **Catalog API**: http://localhost:5002
- **Orders API**: http://localhost:5003
- **Consul UI**: http://localhost:8500

## Management Scripts

### Service Management Script

```powershell
# Start all services
.\scripts\manage-services.ps1 -Action up

# Start services in background
.\scripts\manage-services.ps1 -Action up -Detached

# Start specific service
.\scripts\manage-services.ps1 -Action up -Service "api-gateway"

# Stop all services
.\scripts\manage-services.ps1 -Action down

# Build all services
.\scripts\manage-services.ps1 -Action build

# View logs
.\scripts\manage-services.ps1 -Action logs

# View logs for specific service
.\scripts\manage-services.ps1 -Action logs -Service "api-gateway"

# Check service status
.\scripts\manage-services.ps1 -Action status

# Clean up Docker resources
.\scripts\manage-services.ps1 -Action clean
```

### Integration Testing

```powershell
# Test all service integrations
.\scripts\test-integration.ps1

# Test with custom base URL
.\scripts\test-integration.ps1 -BaseUrl "http://localhost:7000"
```

## Service Configuration

### Database Configuration

Each service uses a separate database:
- **IdentityDb**: Identity service data
- **CatalogDb**: Product catalog data
- **OrdersDb**: Order management data

All databases run on the same PostgreSQL instance with automatic initialization.

### Service Discovery

Services register themselves with Consul for:
- Health monitoring
- Service discovery
- Load balancing
- Configuration management

### API Gateway Features

The API Gateway provides:
- **Unified Entry Point**: Single endpoint for all microservices
- **Authentication**: JWT-based authentication with Identity service
- **Rate Limiting**: Request throttling and abuse prevention
- **Circuit Breaker**: Fault tolerance and resilience
- **Health Monitoring**: Real-time service health checks
- **Service Management**: Dynamic service configuration via web UI
- **Correlation ID**: Request tracing across services

## Troubleshooting

### Common Issues

1. **Port Conflicts**
   ```powershell
   # Check if ports are in use
   netstat -an | findstr "5000 5001 5002 5003 7000 8500"
   
   # Stop conflicting processes or modify docker-compose.yml ports
   ```

2. **Database Connection Issues**
   ```powershell
   # Check PostgreSQL container
   docker logs autoparts-postgres
   
   # Verify database initialization
   docker exec -it autoparts-postgres psql -U postgres -l
   ```

3. **Service Registration Issues**
   ```powershell
   # Check Consul
   docker logs autoparts-consul
   
   # View registered services
   # Visit http://localhost:8500
   ```

4. **Build Failures**
   ```powershell
   # Clean and rebuild
   .\scripts\manage-services.ps1 -Action clean
   .\scripts\manage-services.ps1 -Action build
   ```

### Viewing Logs

```powershell
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f api-gateway
docker-compose logs -f identity-api
docker-compose logs -f catalog-api
docker-compose logs -f orders-api
```

### Health Checks

Each service provides health endpoints:
- Individual: `http://localhost:{port}/health`
- Via Gateway: `http://localhost:7000/api/{service}/health`
- Dashboard: `http://localhost:7000/health-ui`

## Development Workflow

### Local Development

1. **Run Infrastructure Only**
   ```powershell
   docker-compose up postgres consul redis
   ```

2. **Run Services Locally**
   ```powershell
   # Terminal 1: API Gateway
   dotnet run --project src/Gateway/ApiGateway/ApiGateway.csproj
   
   # Terminal 2: Identity API
   dotnet run --project src/Identity/Identity.API/Identity.API.csproj
   
   # Terminal 3: Catalog API
   dotnet run --project src/Services/Catalog/Catalog.API/Catalog.API.csproj
   
   # Terminal 4: Orders API
   dotnet run --project src/Services/Orders/Orders.API/Orders.API.csproj
   ```

### Testing Service Communication

```powershell
# Test via API Gateway
Invoke-RestMethod -Uri "http://localhost:7000/api/catalog/health"
Invoke-RestMethod -Uri "http://localhost:7000/api/identity/health"
Invoke-RestMethod -Uri "http://localhost:7000/api/orders/health"

# Test correlation ID
$headers = @{"X-Correlation-ID" = [System.Guid]::NewGuid()}
Invoke-RestMethod -Uri "http://localhost:7000/api/servicemanagement" -Headers $headers
```

## Next Steps

1. **Add More Services**: Inventory, Payment, Notification, etc.
2. **Implement Authentication Flow**: Complete JWT token flow
3. **Add Monitoring**: Prometheus, Grafana, ELK stack
4. **Service Mesh**: Consider Istio or Linkerd for advanced networking
5. **CI/CD Pipeline**: Automated build and deployment

## Support

For issues and questions:
1. Check service logs
2. Verify service health endpoints
3. Review Consul service registration
4. Run integration tests
5. Check Docker container status
