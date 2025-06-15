# AutoParts Services Complete Setup Script
param(
    [switch]$Clean = $false,
    [switch]$BuildOnly = $false,
    [switch]$InfrastructureOnly = $false
)

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

function Write-Warning {
    param([string]$Message)
    Write-Host $Message -ForegroundColor Yellow
}

# Change to docker directory
$dockerDir = Join-Path $PSScriptRoot "..\docker"
if (!(Test-Path $dockerDir)) {
    Write-Error "Docker directory not found at $dockerDir"
    exit 1
}

Set-Location $dockerDir
Write-Host "Working from directory: $(Get-Location)" -ForegroundColor White

# Check if Docker is running
try {
    $null = docker version 2>$null
    Write-Success "Docker is running"
} catch {
    Write-Error "Docker is not running. Please start Docker Desktop and try again."
    exit 1
}

if ($Clean) {
    Write-Header "Cleaning Up Existing Resources"
    docker-compose down -v --remove-orphans
    docker system prune -f
    docker volume prune -f
    Write-Success "Cleanup completed"
    
    if ($BuildOnly) {
        exit 0
    }
}

Write-Header "AutoParts Services Complete Setup"

# Step 1: Build all images
Write-Header "Building Docker Images"
Write-Host "This may take several minutes on first run..." -ForegroundColor Yellow

$buildResult = docker-compose build
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed. Please check the output above."
    exit 1
}
Write-Success "All Docker images built successfully"

if ($BuildOnly) {
    Write-Success "Build-only mode completed successfully"
    exit 0
}

# Step 2: Start infrastructure services
Write-Header "Starting Infrastructure Services"
Write-Host "Starting PostgreSQL, Consul, and Redis..." -ForegroundColor Yellow

docker-compose up -d postgres consul redis

# Wait for infrastructure to be healthy
Write-Host "Waiting for infrastructure services to be healthy..." -ForegroundColor Yellow
$maxWait = 120
$waited = 0

do {
    Start-Sleep 5
    $waited += 5
    $postgresHealth = docker inspect --format='{{.State.Health.Status}}' autoparts-postgres 2>$null
    $consulHealth = docker inspect --format='{{.State.Health.Status}}' autoparts-consul 2>$null
    $redisHealth = docker inspect --format='{{.State.Health.Status}}' autoparts-redis 2>$null
    
    Write-Host "Infrastructure status - PostgreSQL: $postgresHealth, Consul: $consulHealth, Redis: $redisHealth" -ForegroundColor Gray
    
    if ($postgresHealth -eq "healthy" -and $consulHealth -eq "healthy" -and $redisHealth -eq "healthy") {
        Write-Success "All infrastructure services are healthy"
        break
    }
    
    if ($waited -ge $maxWait) {
        Write-Error "Infrastructure services did not become healthy within $maxWait seconds"
        docker-compose logs postgres consul redis
        exit 1
    }
} while ($true)

if ($InfrastructureOnly) {
    Write-Success "Infrastructure-only mode completed successfully"
    Write-Host "`nInfrastructure URLs:" -ForegroundColor Cyan
    Write-Host "- Consul UI: http://localhost:8500" -ForegroundColor Yellow
    Write-Host "- PostgreSQL: localhost:5432 (user: postgres, password: 123456)" -ForegroundColor Yellow
    Write-Host "- Redis: localhost:6379" -ForegroundColor Yellow
    exit 0
}

# Step 3: Run database migrations
Write-Header "Running Database Migrations"
Write-Host "Starting migration containers..." -ForegroundColor Yellow

docker-compose up --exit-code-from identity-migrator identity-migrator
if ($LASTEXITCODE -ne 0) {
    Write-Error "Identity database migration failed"
    docker-compose logs identity-migrator
    exit 1
}
Write-Success "Identity database migration completed"

docker-compose up --exit-code-from catalog-migrator catalog-migrator
if ($LASTEXITCODE -ne 0) {
    Write-Error "Catalog database migration failed"
    docker-compose logs catalog-migrator
    exit 1
}
Write-Success "Catalog database migration completed"

docker-compose up --exit-code-from orders-migrator orders-migrator
if ($LASTEXITCODE -ne 0) {
    Write-Error "Orders database migration failed"
    docker-compose logs orders-migrator
    exit 1
}
Write-Success "Orders database migration completed"

# Step 4: Start application services
Write-Header "Starting Application Services"
Write-Host "Starting Identity API, Catalog API, Orders API, and API Gateway..." -ForegroundColor Yellow

docker-compose up -d identity-api catalog-api orders-api api-gateway

# Wait for services to be healthy
Write-Host "Waiting for application services to be healthy..." -ForegroundColor Yellow
$maxWait = 180
$waited = 0

do {
    Start-Sleep 10
    $waited += 10
    
    $identityHealth = docker inspect --format='{{.State.Health.Status}}' identity-api 2>$null
    $catalogHealth = docker inspect --format='{{.State.Health.Status}}' catalog-api 2>$null
    $ordersHealth = docker inspect --format='{{.State.Health.Status}}' orders-api 2>$null
    $gatewayHealth = docker inspect --format='{{.State.Health.Status}}' api-gateway 2>$null
    
    Write-Host "Services status - Identity: $identityHealth, Catalog: $catalogHealth, Orders: $ordersHealth, Gateway: $gatewayHealth" -ForegroundColor Gray
    
    if ($identityHealth -eq "healthy" -and $catalogHealth -eq "healthy" -and $ordersHealth -eq "healthy" -and $gatewayHealth -eq "healthy") {
        Write-Success "All application services are healthy"
        break
    }
    
    if ($waited -ge $maxWait) {
        Write-Warning "Some services may not be fully healthy yet, but continuing..."
        break
    }
} while ($true)

# Step 5: Verify services
Write-Header "Verifying Service Integration"

$endpoints = @(
    @{Name="API Gateway Health"; Url="http://localhost:7000/health"},
    @{Name="Identity API Health"; Url="http://localhost:5001/health"},
    @{Name="Catalog API Health"; Url="http://localhost:5002/health"},
    @{Name="Orders API Health"; Url="http://localhost:5003/health"},
    @{Name="Service Management API"; Url="http://localhost:7000/api/servicemanagement"}
)

$successCount = 0
foreach ($endpoint in $endpoints) {
    try {
        $response = Invoke-WebRequest -Uri $endpoint.Url -TimeoutSec 10 -UseBasicParsing -ErrorAction Stop
        Write-Host "✓ $($endpoint.Name): $($response.StatusCode)" -ForegroundColor Green
        $successCount++
    }
    catch {
        Write-Host "✗ $($endpoint.Name): $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Header "Setup Complete!"

if ($successCount -eq $endpoints.Count) {
    Write-Success "🎉 All services are running and healthy!"
} else {
    Write-Warning "⚠️  Some services may need additional time to start"
}

Write-Host "`nService URLs:" -ForegroundColor Cyan
Write-Host "- API Gateway: http://localhost:7000" -ForegroundColor Yellow
Write-Host "- API Gateway Health UI: http://localhost:7000/health-ui" -ForegroundColor Yellow
Write-Host "- Identity API: http://localhost:5001" -ForegroundColor Yellow
Write-Host "- Catalog API: http://localhost:5002" -ForegroundColor Yellow
Write-Host "- Orders API: http://localhost:5003" -ForegroundColor Yellow
Write-Host "- Consul UI: http://localhost:8500" -ForegroundColor Yellow
Write-Host "- Service Management: http://localhost:7000/api/servicemanagement" -ForegroundColor Yellow

Write-Host "`nNext Steps:" -ForegroundColor Cyan
Write-Host "1. Visit http://localhost:7000/health-ui to see service health dashboard" -ForegroundColor White
Write-Host "2. Test API endpoints through the gateway at http://localhost:7000" -ForegroundColor White
Write-Host "3. Use the Service Management API to dynamically manage routes" -ForegroundColor White
Write-Host "4. Check service logs with: docker-compose logs -f [service-name]" -ForegroundColor White

Write-Host "`nManagement Commands:" -ForegroundColor Cyan
Write-Host "- View logs: ..\scripts\manage-services.ps1 logs" -ForegroundColor White
Write-Host "- Stop all: ..\scripts\manage-services.ps1 down" -ForegroundColor White
Write-Host "- Service status: ..\scripts\manage-services.ps1 status" -ForegroundColor White
