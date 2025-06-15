# AutoParts Services Docker Management Script
param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("up", "down", "build", "logs", "status", "clean", "infrastructure", "services")]
    [string]$Action,
    
    [string]$Service = "",
    [switch]$Detached = $false
)

# Change to docker directory
$dockerDir = Join-Path $PSScriptRoot "..\docker"
Set-Location $dockerDir

function Write-Header {
    param([string]$Message)
    Write-Host "`n=== $Message ===" -ForegroundColor Cyan
}

function Write-Success {
    param([string]$Message)
    Write-Host $Message -ForegroundColor Green
}

function Write-Error {
    param([string]$Message)
    Write-Host $Message -ForegroundColor Red
}

switch ($Action) {
    "up" {
        Write-Header "Starting AutoParts Services"
        if ($Service) {
            Write-Host "Starting service: $Service"
            if ($Detached) {
                docker-compose up -d $Service
            } else {
                docker-compose up $Service
            }
        } else {
            Write-Host "Starting all services..."
            if ($Detached) {
                docker-compose up -d
            } else {
                docker-compose up
            }
        }
        Write-Success "Services started successfully!"
        Write-Host "`nService URLs:"
        Write-Host "- API Gateway: http://localhost:7000" -ForegroundColor Yellow
        Write-Host "- Identity API: http://localhost:5001" -ForegroundColor Yellow
        Write-Host "- Catalog API: http://localhost:5002" -ForegroundColor Yellow
        Write-Host "- Orders API: http://localhost:5003" -ForegroundColor Yellow        Write-Host "- PostgreSQL: localhost:5432" -ForegroundColor Yellow
    }
      "infrastructure" {
        Write-Header "Starting Infrastructure Services Only"
        Write-Host "Starting PostgreSQL, Redis, and RabbitMQ..."
        if ($Detached) {
            docker-compose up -d postgres redis rabbitmq
        } else {
            docker-compose up postgres redis rabbitmq
        }
        Write-Success "Infrastructure services started!"
        Write-Host "`nInfrastructure URLs:"
        Write-Host "- PostgreSQL: localhost:5432" -ForegroundColor Yellow
        Write-Host "- Redis: localhost:6379" -ForegroundColor Yellow
        Write-Host "- RabbitMQ Management: http://localhost:15672 (admin/admin123)" -ForegroundColor Yellow
    }
    
    "services" {
        Write-Header "Starting Application Services"
        Write-Host "Starting Identity, Catalog, Orders APIs and Gateway..."
        if ($Detached) {
            docker-compose up -d identity-migrator catalog-migrator orders-migrator identity-api catalog-api orders-api api-gateway
        } else {
            docker-compose up identity-migrator catalog-migrator orders-migrator identity-api catalog-api orders-api api-gateway
        }
        Write-Success "Application services started!"
        Write-Host "`nService URLs:"
        Write-Host "- API Gateway: http://localhost:7000" -ForegroundColor Yellow
        Write-Host "- Identity API: http://localhost:5001" -ForegroundColor Yellow
        Write-Host "- Catalog API: http://localhost:5002" -ForegroundColor Yellow
        Write-Host "- Orders API: http://localhost:5003" -ForegroundColor Yellow
    }
    
    "down" {
        Write-Header "Stopping AutoParts Services"
        docker-compose down
        Write-Success "All services stopped!"
    }
    
    "build" {
        Write-Header "Building AutoParts Services"
        if ($Service) {
            Write-Host "Building service: $Service"
            docker-compose build $Service
        } else {
            Write-Host "Building all services..."
            docker-compose build
        }
        Write-Success "Build completed!"
    }
    
    "logs" {
        Write-Header "Viewing Logs"
        if ($Service) {
            Write-Host "Showing logs for: $Service"
            docker-compose logs -f $Service
        } else {
            Write-Host "Showing logs for all services"
            docker-compose logs -f
        }
    }
    
    "status" {
        Write-Header "Service Status"
        docker-compose ps
        Write-Host "`nHealth Status:"
        docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" | Where-Object { $_ -match "autoparts|identity|catalog|orders|api-gateway|postgres|redis|rabbitmq" }
    }
      "infrastructure" {
        Write-Header "Starting Infrastructure Services Only"
        Write-Host "Starting PostgreSQL, Redis, and RabbitMQ..."
        if ($Detached) {
            docker-compose up -d postgres redis rabbitmq
        } else {
            docker-compose up postgres redis rabbitmq
        }
        Write-Success "Infrastructure services started!"
    }
    
    "services" {
        Write-Header "Starting Application Services"
        Write-Host "Starting Identity, Catalog, Orders APIs and Gateway..."
        if ($Detached) {
            docker-compose up -d identity-migrator catalog-migrator orders-migrator identity-api catalog-api orders-api api-gateway
        } else {
            docker-compose up identity-migrator catalog-migrator orders-migrator identity-api catalog-api orders-api api-gateway
        }
        Write-Success "Application services started!"
    }
    
    "clean" {
        Write-Header "Cleaning Up Docker Resources"
        Write-Host "Stopping all services..." -ForegroundColor Yellow
        docker-compose down
        
        Write-Host "Removing unused images..." -ForegroundColor Yellow
        docker image prune -f
        
        Write-Host "Removing unused volumes..." -ForegroundColor Yellow
        docker volume prune -f
        
        Write-Host "Removing unused networks..." -ForegroundColor Yellow
        docker network prune -f
        
        Write-Success "Cleanup completed!"
    }
}

# Show service endpoints if services are running
if ($Action -eq "status" -or $Action -eq "up") {
    Write-Host "`nQuick Health Checks:" -ForegroundColor Cyan
      $endpoints = @(
        @{Name="API Gateway"; Url="http://localhost:7000/health"},
        @{Name="Identity API"; Url="http://localhost:5001/health"},
        @{Name="Catalog API"; Url="http://localhost:5002/health"},
        @{Name="Orders API"; Url="http://localhost:5003/health"}
    )
    
    foreach ($endpoint in $endpoints) {
        try {
            $response = Invoke-WebRequest -Uri $endpoint.Url -TimeoutSec 5 -UseBasicParsing -ErrorAction Stop
            Write-Host "✓ $($endpoint.Name): Healthy" -ForegroundColor Green
        }
        catch {
            Write-Host "✗ $($endpoint.Name): Unhealthy" -ForegroundColor Red
        }
    }
}
