# Correlation ID Validation Script for AutoParts E-commerce Platform
# This script validates that correlation IDs are working across all microservices

Write-Host "=== AutoParts E-commerce Platform - Correlation ID Validation ===" -ForegroundColor Cyan
Write-Host "This script validates correlation ID propagation across all microservices" -ForegroundColor Green
Write-Host ""

# Configuration
$ApiGatewayUrl = "http://localhost:5115"
$IdentityUrl = "http://localhost:5005"
$CatalogUrl = "http://localhost:5001"
$OrdersUrl = "http://localhost:5100"

# Custom correlation ID for testing
$TestCorrelationId = "test-validation-$(Get-Date -Format 'yyyyMMdd-HHmmss')"

Write-Host "Using test correlation ID: $TestCorrelationId" -ForegroundColor Yellow
Write-Host ""

# Function to make HTTP request with correlation ID
function Test-CorrelationId {
    param(
        [string]$Url,
        [string]$ServiceName,
        [string]$CorrelationId
    )
    
    Write-Host "Testing $ServiceName..." -ForegroundColor Cyan
    Write-Host "URL: $Url" -ForegroundColor Gray
    
    try {
        $headers = @{
            'X-Correlation-ID' = $CorrelationId
            'Accept' = 'application/json'
        }
        
        $response = Invoke-WebRequest -Uri $Url -Method GET -Headers $headers -ErrorAction Stop
        
        # Check if correlation ID is returned in response headers
        $returnedCorrelationId = $response.Headers['X-Correlation-ID']
        
        Write-Host "✅ Status: $($response.StatusCode)" -ForegroundColor Green
        Write-Host "✅ Sent Correlation ID: $CorrelationId" -ForegroundColor Green
        Write-Host "✅ Returned Correlation ID: $returnedCorrelationId" -ForegroundColor Green
        
        if ($returnedCorrelationId -eq $CorrelationId) {
            Write-Host "✅ Correlation ID propagation: SUCCESS" -ForegroundColor Green
        } else {
            Write-Host "❌ Correlation ID propagation: FAILED" -ForegroundColor Red
        }
        
        return $true
    }
    catch {
        Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    
    Write-Host ""
}

# Function to test without correlation ID (auto-generation)
function Test-AutoGeneration {
    param(
        [string]$Url,
        [string]$ServiceName
    )
    
    Write-Host "Testing auto-generation for $ServiceName..." -ForegroundColor Cyan
    
    try {
        $response = Invoke-WebRequest -Uri $Url -Method GET -ErrorAction Stop
        $generatedCorrelationId = $response.Headers['X-Correlation-ID']
        
        Write-Host "✅ Status: $($response.StatusCode)" -ForegroundColor Green
        Write-Host "✅ Auto-generated Correlation ID: $generatedCorrelationId" -ForegroundColor Green
        
        if (![string]::IsNullOrEmpty($generatedCorrelationId)) {
            Write-Host "✅ Auto-generation: SUCCESS" -ForegroundColor Green
        } else {
            Write-Host "❌ Auto-generation: FAILED" -ForegroundColor Red
        }
        
        return $true
    }
    catch {
        Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
        return $false
    }
    
    Write-Host ""
}

# Validation Tests
Write-Host "=== 1. API Gateway Correlation ID Tests ===" -ForegroundColor Magenta

# Test API Gateway diagnostic endpoint
Test-CorrelationId -Url "$ApiGatewayUrl/api/diagnostics/ping" -ServiceName "API Gateway Diagnostics" -CorrelationId $TestCorrelationId
Test-AutoGeneration -Url "$ApiGatewayUrl/api/diagnostics/ping" -ServiceName "API Gateway Diagnostics"

# Test API Gateway health endpoint
Test-CorrelationId -Url "$ApiGatewayUrl/health" -ServiceName "API Gateway Health" -CorrelationId $TestCorrelationId

Write-Host "=== 2. Direct Service Tests (if running) ===" -ForegroundColor Magenta

# Test Identity Service
Write-Host "Testing Identity Service..." -ForegroundColor Cyan
Test-CorrelationId -Url "$IdentityUrl/health" -ServiceName "Identity Service" -CorrelationId $TestCorrelationId

# Test Catalog Service  
Write-Host "Testing Catalog Service..." -ForegroundColor Cyan
Test-CorrelationId -Url "$CatalogUrl/api/products" -ServiceName "Catalog Service" -CorrelationId $TestCorrelationId

# Test Orders Service
Write-Host "Testing Orders Service..." -ForegroundColor Cyan
Test-CorrelationId -Url "$OrdersUrl/api/orders" -ServiceName "Orders Service" -CorrelationId $TestCorrelationId

Write-Host "=== 3. End-to-End Correlation ID Test ===" -ForegroundColor Magenta

# This would test a request that flows through multiple services
Write-Host "Testing end-to-end flow through API Gateway..." -ForegroundColor Cyan

# Test through API Gateway to backend services
$e2eTests = @(
    @{ Url = "$ApiGatewayUrl/api/catalog/products"; Service = "Catalog via Gateway" },
    @{ Url = "$ApiGatewayUrl/api/identity/health"; Service = "Identity via Gateway" },
    @{ Url = "$ApiGatewayUrl/api/orders/health"; Service = "Orders via Gateway" }
)

foreach ($test in $e2eTests) {
    Test-CorrelationId -Url $test.Url -ServiceName $test.Service -CorrelationId $TestCorrelationId
}

Write-Host "=== 4. Log Verification Instructions ===" -ForegroundColor Magenta
Write-Host "To verify correlation IDs in logs, check these locations:" -ForegroundColor Yellow
Write-Host "1. API Gateway: src/Gateway/ApiGateway/Logs/" -ForegroundColor Gray
Write-Host "2. Identity Service: src/Identity/Identity.API/logs/" -ForegroundColor Gray
Write-Host "3. Catalog Service: Check console output for Serilog entries" -ForegroundColor Gray
Write-Host ""
Write-Host "Look for log entries containing: $TestCorrelationId" -ForegroundColor Yellow

Write-Host "=== 5. Implementation Checklist ===" -ForegroundColor Magenta
Write-Host "✅ Custom CorrelationId middleware implemented" -ForegroundColor Green
Write-Host "✅ API Gateway integration complete" -ForegroundColor Green
Write-Host "⏳ Microservices correlation ID propagation (needs verification)" -ForegroundColor Yellow
Write-Host "⏳ Serilog integration in all services (needs verification)" -ForegroundColor Yellow

Write-Host ""
Write-Host "=== Validation Complete ===" -ForegroundColor Cyan
