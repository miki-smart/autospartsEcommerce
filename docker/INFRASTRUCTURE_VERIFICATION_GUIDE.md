# Infrastructure Services Verification Guide

## Overview
This document provides a step-by-step guide to verify the health and status of all infrastructure services in the AutoParts E-commerce platform using Docker Compose.

## Prerequisites
- Docker and Docker Compose installed
- PowerShell terminal access
- Working directory: `e:\autoparts-ecommerce\autospartsEcommerce\docker`

---

## Step 1: Check Overall Service Status

### Command:
```powershell
docker-compose ps
```

### Purpose:
- Displays the status of all services defined in docker-compose.yml
- Shows container names, current state, health status, and port mappings

### Expected Outcome:
```
NAME        IMAGE                             COMMAND                  SERVICE     CREATED          STATUS                    PORTS
consul      hashicorp/consul:1.21             "docker-entrypoint.s…"   consul      13 minutes ago   Up 13 minutes (healthy)   8300-8302/tcp, 8600/tcp, 8301-8302/udp, 0.0.0.0:8500->8500/tcp, 0.0.0.0:8600->8600/udp
postgres    postgres:16                       "docker-entrypoint.s…"   postgres    13 minutes ago   Up 13 minutes (healthy)   0.0.0.0:5432->5432/tcp
rabbit-mq   rabbitmq:3.12-management-alpine   "docker-entrypoint.s…"   rabbit-mq   13 minutes ago   Up 13 minutes (healthy)   4369/tcp, 5671/tcp, 0.0.0.0:5672->5672/tcp, 15671/tcp, 15691-15692/tcp, 25672/tcp, 0.0.0.0:15672->15672/tcp
redis       redis:7-alpine                    "docker-entrypoint.s…"   redis       13 minutes ago   Up 13 minutes (healthy)   0.0.0.0:6379->6379/tcp
```

### Health Indicators:
- ✅ **STATUS**: All services should show "Up X minutes (healthy)"
- ✅ **PORTS**: Correct port mappings displayed
- ✅ **STATE**: All containers running

---

## Step 2: PostgreSQL Verification

### 2.1 Check PostgreSQL Logs
```powershell
docker-compose logs --tail=10 postgres
```

**Purpose**: Verify PostgreSQL startup sequence and ensure no errors
**Expected**: Should show database system ready to accept connections

### 2.2 Test PostgreSQL Connectivity
```powershell
docker exec postgres pg_isready -h localhost -p 5432
```

**Purpose**: Check if PostgreSQL is accepting connections
**Expected Output**: `localhost:5432 - accepting connections`

### 2.3 Verify Database Version
```powershell
docker exec postgres psql -U postgres -d AutoPartsDb -c "SELECT version();"
```

**Purpose**: Confirm PostgreSQL version and database accessibility
**Expected**: PostgreSQL version information displayed

### 2.4 List All Databases
```powershell
docker exec postgres psql -U postgres -c "\l"
```

**Purpose**: Verify all required databases were created by init script
**Expected Databases**:
- AutoPartsDb
- IdentityDb
- CatalogDb
- OrdersDb
- PaymentDb
- NotificationDb
- SearchDb
- InventoryDb
- LogisticsDb
- SellerDb

---

## Step 3: Redis Verification

### 3.1 Check Redis Logs
```powershell
docker-compose logs --tail=10 redis
```

**Purpose**: Verify Redis startup and configuration
**Expected**: Should show "Ready to accept connections tcp"

### 3.2 Test Redis Connectivity
```powershell
docker exec redis redis-cli ping
```

**Purpose**: Verify Redis is responding to commands
**Expected Output**: `PONG`

### 3.3 Test Redis Hostname Resolution
```powershell
docker exec redis redis-cli -h redis ping
```

**Purpose**: Verify Redis can be accessed via hostname within Docker network
**Expected Output**: `PONG`

---

## Step 4: RabbitMQ Verification

### 4.1 Check RabbitMQ Logs
```powershell
docker-compose logs --tail=10 rabbit-mq
```

**Purpose**: Verify RabbitMQ startup and plugin initialization
**Expected**: Should show "Server startup complete" with enabled plugins

### 4.2 Test RabbitMQ Management Interface
```powershell
Invoke-WebRequest -Uri "http://localhost:15672" -UseBasicParsing | Select-Object -ExpandProperty StatusCode
```

**Purpose**: Verify RabbitMQ management UI is accessible
**Expected Output**: `200`

### 4.3 Check RabbitMQ Status
```powershell
docker exec rabbit-mq rabbitmqctl status
```

**Purpose**: Get detailed RabbitMQ cluster and service status
**Expected**: Detailed status showing:
- Runtime information
- Enabled plugins (management, prometheus, etc.)
- Memory usage
- Listeners on correct ports (5672, 15672, 15692, 25672)

---

## Step 5: Consul Verification

### 5.1 Check Consul Logs
```powershell
docker-compose logs --tail=10 consul
```

**Purpose**: Verify Consul startup and leader election
**Expected**: Should show raft log verification messages

### 5.2 Test Consul API
```powershell
Invoke-WebRequest -Uri "http://localhost:8500/v1/status/leader" -UseBasicParsing | Select-Object -ExpandProperty Content
```

**Purpose**: Verify Consul API is responding and leader is elected
**Expected Output**: IP address and port of the leader (e.g., `"172.18.0.2:8300"`)

---

## Step 6: Network Connectivity Verification

### 6.1 List Docker Networks
```powershell
docker network ls
```

**Purpose**: Verify the autoparts network exists
**Expected**: Should show `docker_autoparts-network` or `autospartsecommerce_autoparts-network`

### 6.2 Inspect Network Configuration
```powershell
docker network inspect docker_autoparts-network
```

**Purpose**: Verify all containers are connected to the same network
**Expected**: All four containers listed with assigned IP addresses:
- consul: 172.18.0.2
- postgres: 172.18.0.3
- rabbit-mq: 172.18.0.4
- redis: 172.18.0.5

### 6.3 Test DNS Resolution Between Services
```powershell
docker exec consul nslookup postgres
docker exec consul nslookup redis
docker exec consul nslookup rabbit-mq
```

**Purpose**: Verify inter-service DNS resolution works
**Expected**: Each command should return the correct IP address for the target service

---

## Step 7: Generate Final Status Report

### Command:
```powershell
docker-compose ps --format "table {{.Name}}\t{{.State}}\t{{.Status}}\t{{.Ports}}"
```

**Purpose**: Generate a clean summary table of all services
**Expected**: Formatted table showing all services as "running" and "healthy"

---

## Health Check Indicators Summary

### ✅ Healthy Infrastructure Signs:
1. **All services show "healthy" status** in docker-compose ps
2. **PostgreSQL**: 
   - Accepts connections via pg_isready
   - All databases present
   - No connection errors in logs
3. **Redis**: 
   - Responds to PING commands
   - No connection timeouts
4. **RabbitMQ**: 
   - Management UI returns HTTP 200
   - rabbitmqctl status shows all listeners active
   - All required plugins enabled
5. **Consul**: 
   - API returns leader information
   - No raft election errors in logs
6. **Network**: 
   - All containers on same network
   - DNS resolution works between services

### ❌ Warning Signs to Watch For:
- Services showing "unhealthy" status
- Connection refused errors
- Missing databases in PostgreSQL
- Redis/RabbitMQ not responding to commands
- Consul showing no leader elected
- Network connectivity issues between containers

---

## Troubleshooting Commands

### Restart Individual Service:
```powershell
docker-compose restart <service-name>
```

### View Full Logs:
```powershell
docker-compose logs <service-name>
```

### Rebuild Service:
```powershell
docker-compose up -d --force-recreate <service-name>
```

### Check Container Resource Usage:
```powershell
docker stats
```

---

## Infrastructure Service Details

| Service | Default Port | Management Port | Health Check Endpoint | Default Credentials |
|---------|--------------|-----------------|----------------------|-------------------|
| PostgreSQL | 5432 | N/A | pg_isready | postgres/123456 |
| Redis | 6379 | N/A | redis-cli ping | N/A |
| RabbitMQ | 5672 | 15672 | http://localhost:15672 | admin/admin123 |
| Consul | 8500 | 8500 | http://localhost:8500/v1/status/leader | N/A |

This verification guide ensures all infrastructure services are properly configured, healthy, and ready to support the AutoParts E-commerce application services.
