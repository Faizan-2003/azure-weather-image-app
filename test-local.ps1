# Weather Image API Test Script (PowerShell)
# Tests the locally running Azure Functions app

$API_URL = "http://localhost:7071"
$API_KEY = "test-api-key-12345"
$BAD_KEY = "wrong-key"

Write-Host "=== Weather Image API Tests ===" -ForegroundColor Cyan
Write-Host ""

# Test 1: Unauthorized Access
Write-Host "Test 1: Testing unauthorized access (should return 401)..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_URL/api/job/start" -Method POST -Headers @{"X-API-Key"=$BAD_KEY} -ContentType "application/json" -ErrorAction Stop
    Write-Host "[FAIL] Expected 401, got $($response.StatusCode)" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401 -or $_.Exception.Message -like "*401*" -or $_.Exception.Message -like "*Unauthorized*") {
        Write-Host "[PASS] Correctly rejected with 401 Unauthorized" -ForegroundColor Green
    } else {
        Write-Host "[FAIL] Got error: $($_.Exception.Message)" -ForegroundColor Red
    }
}
Write-Host ""

# Test 2: Start a New Job
Write-Host "Test 2: Starting a new weather image job..." -ForegroundColor Yellow
$JOB_ID = $null
try {
    $response = Invoke-RestMethod -Uri "$API_URL/api/job/start" -Method POST -Headers @{"X-API-Key"=$API_KEY; "Content-Type"="application/json"} -ErrorAction Stop
    
    if ($response.jobId) {
        Write-Host "[PASS] Job started successfully!" -ForegroundColor Green
        Write-Host "  Job ID: $($response.jobId)" -ForegroundColor Cyan
        Write-Host "  Status: $($response.status)" -ForegroundColor Cyan
        Write-Host "  Message: $($response.message)" -ForegroundColor Cyan
        $JOB_ID = $response.jobId
    } else {
        Write-Host "[FAIL] Failed to start job - no jobId returned" -ForegroundColor Red
        Write-Host ($response | ConvertTo-Json)
    }
} catch {
    Write-Host "[FAIL] Error starting job: $($_.Exception.Message)" -ForegroundColor Red
}
Write-Host ""

# Test 3: Check Job Status
if ($JOB_ID) {
    Write-Host "Test 3: Checking job status..." -ForegroundColor Yellow
    Start-Sleep -Seconds 2
    
    try {
        $statusData = Invoke-RestMethod -Uri "$API_URL/api/job/$JOB_ID" -Method GET -Headers @{"X-API-Key"=$API_KEY} -ErrorAction Stop
        
        Write-Host "[PASS] Job status retrieved!" -ForegroundColor Green
        Write-Host "  Job ID: $($statusData.jobId)" -ForegroundColor Cyan
        Write-Host "  Status: $($statusData.status)" -ForegroundColor Cyan
        Write-Host "  Total Stations: $($statusData.totalStations)" -ForegroundColor Cyan
        Write-Host "  Processed: $($statusData.processedStations)" -ForegroundColor Cyan
        
        if ($statusData.images -and $statusData.images.Count -gt 0) {
            Write-Host "  Images Generated: $($statusData.images.Count)" -ForegroundColor Green
            Write-Host "  Sample Images:" -ForegroundColor Cyan
            $statusData.images | Select-Object -First 3 | ForEach-Object {
                Write-Host "    - $($_.stationName): $($_.temperature)C, $($_.weatherDescription)" -ForegroundColor Gray
            }
        } else {
            Write-Host "  Images Generated: 0 (processing...)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "[FAIL] Error checking status: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
    
    # Test 4: Wait and Check Again
    Write-Host "Test 4: Waiting 10 seconds and checking progress..." -ForegroundColor Yellow
    Start-Sleep -Seconds 10
    
    try {
        $statusData = Invoke-RestMethod -Uri "$API_URL/api/job/$JOB_ID" -Method GET -Headers @{"X-API-Key"=$API_KEY} -ErrorAction Stop
        
        Write-Host "[PASS] Updated job status:" -ForegroundColor Green
        Write-Host "  Status: $($statusData.status)" -ForegroundColor Cyan
        Write-Host "  Processed: $($statusData.processedStations) / $($statusData.totalStations)" -ForegroundColor Cyan
        Write-Host "  Images: $($statusData.images.Count)" -ForegroundColor Cyan
        
        if ($statusData.status -eq "Completed") {
            Write-Host "  [SUCCESS] Job completed successfully!" -ForegroundColor Green
        } elseif ($statusData.status -eq "InProgress" -or $statusData.status -eq "Processing") {
            Write-Host "  [INFO] Job still processing..." -ForegroundColor Yellow
        }
    } catch {
        Write-Host "[FAIL] Error checking status: $($_.Exception.Message)" -ForegroundColor Red
    }
} else {
    Write-Host "[SKIP] Test 3 & 4 skipped - no job ID available" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Tests Complete ===" -ForegroundColor Cyan
Write-Host ""
