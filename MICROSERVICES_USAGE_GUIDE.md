# Microservices API Gateway - Usage Guide

## Architecture Overview

This microservices architecture uses:
- **Ocelot API Gateway** as the single entry point for all external requests
- **Consul** for service discovery and health checking
- **Duende IdentityServer** for authentication and authorization (NOT registered in Consul)
- **JWT Bearer tokens** for securing API endpoints

## Service Architecture

```
External Clients (Web/Mobile/Postman)
                ↓
        API Gateway (Port 8080)
                ↓
    ┌─────────────────────────────────┐
    │        Consul Discovery         │
    │                                 │
    │  ✓ Catalog Service             │
    │  ✓ User Management Service     │
    │                                 │
    │  ✗ IdentityServer (Static)     │
    └─────────────────────────────────┘
```

## Quick Start

### 1. Start All Services
```bash
docker-compose -f docker-compose.production.yml up -d
```

### 2. Verify Services
```bash
# Check all containers are running
docker ps

# Check Consul UI
http://localhost:8500

# Check API Gateway health
http://localhost:8080/health

# Check IdentityServer discovery
http://localhost:5000/.well-known/openid_configuration
```

## API Gateway Endpoints

### Gateway Information
```http
GET http://localhost:8080/
GET http://localhost:8080/api/gateway/info
GET http://localhost:8080/health
GET http://localhost:8080/health-ui
```

### Service Routes (Through Gateway)

#### Catalog Service
```http
# Health check (no auth required)
GET http://localhost:8080/api/catalog/health

# Get products (requires auth)
GET http://localhost:8080/api/catalog/products
Authorization: Bearer {jwt_token}

# Get specific product (requires auth)
GET http://localhost:8080/api/catalog/products/1
Authorization: Bearer {jwt_token}

# Create product (requires auth)
POST http://localhost:8080/api/catalog/products
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "name": "New Product",
  "price": 99.99,
  "category": "Electronics"
}
```

#### User Management Service
```http
# Health check (no auth required)
GET http://localhost:8080/api/users/health

# Get user profiles (requires auth)
GET http://localhost:8080/api/users/profiles
Authorization: Bearer {jwt_token}

# Get specific user profile (requires auth)
GET http://localhost:8080/api/users/profiles/1
Authorization: Bearer {jwt_token}

# Update user profile (requires auth)
PUT http://localhost:8080/api/users/profiles/1
Authorization: Bearer {jwt_token}
Content-Type: application/json

{
  "firstName": "Updated",
  "lastName": "Name",
  "email": "updated@example.com"
}

# Get user preferences (requires auth)
GET http://localhost:8080/api/users/preferences/1
Authorization: Bearer {jwt_token}
```

#### Identity Service (Through Gateway)
```http
# Health check
GET http://localhost:8080/api/identity/health

# User registration
POST http://localhost:8080/api/identity/register
Content-Type: application/json

{
  "username": "testuser",
  "email": "test@example.com",
  "password": "SecurePassword123!"
}

# User login
POST http://localhost:8080/api/identity/login
Content-Type: application/json

{
  "username": "testuser",
  "password": "SecurePassword123!"
}

# Get user info (requires auth)
GET http://localhost:8080/api/identity/user-info
Authorization: Bearer {jwt_token}

# IdentityServer endpoints
GET http://localhost:8080/.well-known/openid_configuration
POST http://localhost:8080/connect/token
```

## Authentication Flow

### 1. Get Access Token (Client Credentials)
```http
POST http://localhost:8080/connect/token
Content-Type: application/x-www-form-urlencoded

grant_type=client_credentials&
client_id=postman-client&
client_secret=postman-secret&
scope=catalog.read catalog.write users.read users.write
```

### 2. Use Token for API Calls
```http
GET http://localhost:8080/api/catalog/products
Authorization: Bearer eyJhbGciOiJSUzI1NiIsImtpZCI6...
```

## Testing with Postman

### Collection Setup
1. **Base URL Variable**: `{{gateway_url}}` = `http://localhost:8080`
2. **Auth Variable**: `{{token}}` = Bearer token from auth request

### Pre-request Script for Token
```javascript
// Pre-request script to get token automatically
pm.sendRequest({
    url: pm.variables.get("gateway_url") + "/connect/token",
    method: 'POST',
    header: {
        'Content-Type': 'application/x-www-form-urlencoded'
    },
    body: {
        mode: 'urlencoded',
        urlencoded: [
            {key: 'grant_type', value: 'client_credentials'},
            {key: 'client_id', value: 'postman-client'},
            {key: 'client_secret', value: 'postman-secret'},
            {key: 'scope', value: 'catalog.read catalog.write users.read users.write'}
        ]
    }
}, function (err, response) {
    if (response.code === 200) {
        const token = response.json().access_token;
        pm.variables.set("token", "Bearer " + token);
    }
});
```

## Service Discovery Verification

### Check Consul Services
```bash
# Via Consul API
curl http://localhost:8500/v1/agent/services

# Expected response shows only business services:
# - catalog-service
# - user-management-service
# (IdentityServer should NOT be listed)
```

### Service Health Checks
```bash
# Check service health via Consul
curl http://localhost:8500/v1/health/service/catalog-service
curl http://localhost:8500/v1/health/service/user-management-service
```

## Security Features

### ✅ Implemented Security
- JWT Bearer token authentication
- Scope-based authorization
- Rate limiting (per service)
- Circuit breaker pattern
- Request correlation tracking
- HTTPS ready (configure for production)

### 🔒 Authentication Requirements
- **Public endpoints**: Health checks, IdentityServer discovery
- **Protected endpoints**: All business API endpoints require valid JWT
- **Scopes required**:
  - `catalog.read` - Read catalog data
  - `catalog.write` - Modify catalog data
  - `users.read` - Read user data
  - `users.write` - Modify user data

## Monitoring & Logging

### Health Checks
- **API Gateway**: `http://localhost:8080/health`
- **Health UI**: `http://localhost:8080/health-ui`
- **Individual Services**: Health checks via Consul

### Logs
- All services use Serilog with structured logging
- Logs are written to console and files
- Request correlation IDs for tracing

### Consul UI
- **URL**: `http://localhost:8500`
- **Features**: Service registry, health status, key-value store

## Troubleshooting

### Common Issues
1. **503/502 Errors**: Check service health in Consul
2. **401 Unauthorized**: Verify JWT token and scopes
3. **404 Not Found**: Check Ocelot route configuration
4. **Services not registering**: Check Consul connectivity

### Debug Commands
```bash
# Check container logs
docker logs api-gateway
docker logs catalog-service
docker logs user-management-service
docker logs identityserver

# Check service registration
curl http://localhost:8500/v1/agent/services

# Test service connectivity
docker exec api-gateway curl http://catalog-service/health
docker exec api-gateway curl http://user-management-service/health
```

## Production Considerations

### Security Hardening
- Enable HTTPS everywhere
- Use proper certificates (not developer signing)
- Implement proper client secrets management
- Add rate limiting and throttling
- Enable CORS properly for your domains

### Scalability
- Add load balancing for multiple service instances
- Implement distributed caching
- Add database persistence
- Use proper secret management (Azure Key Vault, etc.)

### Monitoring
- Add distributed tracing (Jaeger/Zipkin)
- Implement metrics collection (Prometheus)
- Add alerting and monitoring
- Use proper log aggregation (ELK stack)
