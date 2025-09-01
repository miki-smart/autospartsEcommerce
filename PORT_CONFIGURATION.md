# Port Configuration and Service Discovery Summary

## 🔧 **Current Docker Port Configuration**

| Service | Container | Internal Port | External Port | Docker Mapping | Service Discovery |
|---------|-----------|---------------|---------------|----------------|-------------------|
| **API Gateway** | `api-gateway` | `8083` | `7000` | `7000:8083` | **Entry Point** |
| **Identity API** | `identity-api` | `8084` | `5001` | `5001:8084` | ❌ **NOT in Consul** |
| **Catalog API** | `catalog-api` | `8081` | `5002` | `5002:8081` | ✅ **Consul Discovery** |
| **Orders API** | `order-api` | `8082` | `5003` | `5003:8082` | ✅ **Consul Discovery** |
| **Consul** | `consul` | `8500` | `8500` | `8500:8500` | **Service Registry** |
| **PostgreSQL** | `postgres` | `5432` | `5432` | `5432:5432` | **Database** |
| **Redis** | `redis` | `6379` | `6379` | `6379:6379` | **Cache** |
| **RabbitMQ** | `rabbit-mq` | `5672/15672` | `5672/15672` | `5672:5672, 15672:15672` | **Message Queue** |

## 🌐 **Service Access Patterns**

### **External Client Access (Development)**
```
External Client (Postman/Browser)
    ↓ http://localhost:7000
API Gateway (Ocelot)
    ↓
┌─────────────────────────────────────────┐
│          Service Routing                │
├─────────────────────────────────────────┤
│ Identity: Static → identity-api:8084    │
│ Catalog:  Consul → catalog-api:8081     │
│ Orders:   Consul → order-api:8082       │
└─────────────────────────────────────────┘
```

### **Internal Container Communication**
```
api-gateway:8083 → identity-api:8084    (Static Routing)
api-gateway:8083 → catalog-api:8081     (Consul Discovery)
api-gateway:8083 → order-api:8082       (Consul Discovery)
api-gateway:8083 → consul:8500          (Service Discovery)
```

## 🔐 **Authentication Flow**

### **Identity Server Configuration**
- **Container**: `identity-api:8084`
- **External Access**: `localhost:5001` (for direct testing)
- **API Gateway Route**: `localhost:7000/api/identity/*`
- **Discovery Method**: Static routing (NOT registered in Consul)
- **Authority URL**: `http://identity-api:8084` (internal container network)

### **Business Services Configuration**
- **Catalog Service**: Registers as `catalog-api` in Consul
- **Orders Service**: Registers as `order-api` in Consul
- **Authentication**: All protected endpoints require JWT from Identity Server

## 🚀 **API Gateway Routes (via localhost:7000)**

### **Identity Service (Static Routing)**
```
GET   /api/identity/health           → identity-api:8084/health
POST  /api/identity/*               → identity-api:8084/api/*
GET   /.well-known/*                → identity-api:8084/.well-known/*
POST  /connect/token                → identity-api:8084/connect/token
```

### **Catalog Service (Consul Discovery)**
```
GET   /api/catalog/health           → consul → catalog-api:8081/health
GET   /api/catalog/*                → consul → catalog-api:8081/api/*
```

### **Orders Service (Consul Discovery)**
```
GET   /api/orders/health            → consul → order-api:8082/health
GET   /api/orders/*                 → consul → order-api:8082/api/*
```

## ⚡ **Quick Test Commands**

### **1. Start Services**
```bash
cd c:\Users\mikiy\autoparts-services\docker
docker-compose up -d
```

### **2. Verify Service Health**
```bash
# API Gateway (main entry point)
curl http://localhost:7000/health

# Individual services (direct access for testing)
curl http://localhost:5001/health    # Identity
curl http://localhost:5002/health    # Catalog  
curl http://localhost:5003/health    # Orders

# Consul UI
# http://localhost:8500
```

### **3. Test Service Discovery**
```bash
# Check which services are registered in Consul
curl http://localhost:8500/v1/agent/services

# Expected: Should show catalog-api and order-api
# Should NOT show identity-api (as it uses static routing)
```

### **4. Test API Gateway Routing**
```bash
# Test Identity service through gateway (no auth needed for health)
curl http://localhost:7000/api/identity/health

# Test Catalog service through gateway (no auth needed for health)
curl http://localhost:7000/api/catalog/health

# Test Orders service through gateway (no auth needed for health)
curl http://localhost:7000/api/orders/health
```

## 🔍 **Port Consistency Status**

### ✅ **Correct Configurations**
- API Gateway routes to Identity using correct port (8084)
- Consul discovery points to correct ports for business services
- All internal service communications use container network names and internal ports
- External port mappings allow direct service access for development/testing

### ⚠️ **Configuration Notes**
- Identity Service is correctly configured to NOT register with Consul
- API Gateway uses static routing for Identity Service (as intended)
- Business services (Catalog, Orders) correctly register with Consul for load balancing
- Authentication authority correctly points to `identity-api:8084` for JWT validation

## 🧪 **Development Testing Strategy**

### **Phase 1: Infrastructure Verification**
1. ✅ Start all containers
2. ✅ Verify health endpoints
3. ✅ Check Consul service registration
4. ✅ Test API Gateway routing

### **Phase 2: Authentication Testing**
1. 🔄 Get JWT token from Identity Service
2. 🔄 Test protected endpoints through API Gateway
3. 🔄 Verify JWT validation on business services

### **Phase 3: Service Integration Testing**
1. 🔄 Test cross-service communication
2. 🔄 Verify distributed logging and correlation
3. 🔄 Test circuit breaker and rate limiting
