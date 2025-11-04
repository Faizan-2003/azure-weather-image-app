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
    Write-Host "✗ Expected 401, got $($response.StatusCode)" -ForegroundColor Red
} catch [System.Net.WebException] {
    if ($_.Exception.Response.StatusCode -eq 401 -or $_.Exception.Response.StatusCode -eq "Unauthorized") {
        Write-Host "✓ Correctly rejected with 401 Unauthorized" -ForegroundColor Green
    } else {
        Write-Host "✗ Got error: $($_.Exception.Message)" -ForegroundColor Red
    }
}
Write-Host ""

# Test 2: Start a New Job
Write-Host "Test 2: Starting a new weather image job..." -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$API_URL/api/job/start" -Method POST -Headers @{"X-API-Key"=$API_KEY} -ContentType "application/json" -ErrorAction Stop
    $jobData = $response.Content | ConvertFrom-Json
    
    if ($response.StatusCode -eq 200 -and $jobData.jobId) {
        Write-Host "✓ Job started successfully!" -ForegroundColor Green
        Write-Host "  Job ID: $($jobData.jobId)" -ForegroundColor Gray
        Write-Host "  Status: $($jobData.status)" -ForegroundColor Gray
        Write-Host "  Estimated Images: $($jobData.estimatedImagesCount)" -ForegroundColor Gray
        $JOB_ID = $jobData.jobId
    } else {
        Write-Host "✗ Failed to start job" -ForegroundColor Red
        Write-Host $response.Content
    }
} catch {
    Write-Host "✗ Error starting job: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "  Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
}
Write-Host ""

# Test 3: Check Job Status
if ($JOB_ID) {
    Write-Host "Test 3: Checking job status..." -ForegroundColor Yellow
    Start-Sleep -Seconds 2
    
    try {
        $response = Invoke-WebRequest -Uri "$API_URL/api/job/$JOB_ID" -Method GET -Headers @{"X-API-Key"=$API_KEY} -ErrorAction Stop
        $statusData = $response.Content | ConvertFrom-Json
        
        Write-Host "✓ Job status retrieved!" -ForegroundColor Green
        Write-Host "  Job ID: $($statusData.jobId)" -ForegroundColor Gray
        Write-Host "  Status: $($statusData.status)" -ForegroundColor Gray
        Write-Host "  Total Images: $($statusData.totalImages)" -ForegroundColor Gray
        Write-Host "  Processed: $($statusData.processedImages)" -ForegroundColor Gray
        Write-Host "  Failed: $($statusData.failedImages)" -ForegroundColor Gray
        
        if ($statusData.images -and $statusData.images.Count -gt 0) {
            Write-Host "  Sample Image URLs:" -ForegroundColor Gray
            $statusData.images | Select-Object -First 3 | ForEach-Object {
                Write-Host "    - $($_.stationName): $($_.imageUrl)" -ForegroundColor Gray
            }
        }
    } catch {
        Write-Host "✗ Error checking status: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
    
    # Test 4: Wait and Check Again
    Write-Host "Test 4: Waiting 5 seconds and checking progress..." -ForegroundColor Yellow
    Start-Sleep -Seconds 5
    
    try {
        $response = Invoke-WebRequest -Uri "$API_URL/api/job/$JOB_ID" -Method GET -Headers @{"X-API-Key"=$API_KEY} -ErrorAction Stop
        $statusData = $response.Content | ConvertFrom-Json
        
        Write-Host "✓ Updated job status:" -ForegroundColor Green
        Write-Host "  Status: $($statusData.status)" -ForegroundColor Gray
        Write-Host "  Processed: $($statusData.processedImages) / $($statusData.totalImages)" -ForegroundColor Gray
        
        if ($statusData.status -eq "Completed") {
            Write-Host "  🎉 Job completed successfully!" -ForegroundColor Green
        } elseif ($statusData.status -eq "Processing") {
            Write-Host "  ⏳ Job still processing..." -ForegroundColor Yellow
        }
    } catch {
        Write-Host "✗ Error checking status: $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "=== Tests Complete ===" -ForegroundColor Cyan
