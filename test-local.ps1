$API_URL = "http://localhost:7071"
$API_KEY = "test-api-key-12345"
$BAD_KEY = "wrong-key"

Write-Host "=== Weather Image API Tests ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Multi-Queue Architecture:" -ForegroundColor Gray
Write-Host "  1. HTTP Request -> StartJobFunction -> job-start-queue" -ForegroundColor Gray
Write-Host "  2. JobInitiatorFunction (queue trigger) -> fetches weather stations" -ForegroundColor Gray
Write-Host "  3. Fan-out to image-processing-queue (40 messages)" -ForegroundColor Gray
Write-Host "  4. ProcessImageFunction (40 parallel executions)" -ForegroundColor Gray
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
        Write-Host "  -> Job message sent to job-start-queue" -ForegroundColor Gray
        Write-Host "  -> JobInitiatorFunction will process and fan out to image-processing-queue" -ForegroundColor Gray
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
            Write-Host "  Images Generated: 0 (job-start-queue -> JobInitiatorFunction -> image-processing-queue)" -ForegroundColor Yellow
        }
    } catch {
        Write-Host "[FAIL] Error checking status: $($_.Exception.Message)" -ForegroundColor Red
    }
    Write-Host ""
    
    # Test 4: Monitor Until Completion
    Write-Host "Test 4: Monitoring job until completion..." -ForegroundColor Yellow
    Write-Host "  (Press Ctrl+C to stop monitoring)" -ForegroundColor Gray
    Write-Host ""
    
    $checkCount = 0
    $maxChecks = 60  # Maximum 3 minutes (60 checks * 3 seconds)
    
    do {
        Start-Sleep -Seconds 3
        $checkCount++
        
        try {
            $statusData = Invoke-RestMethod -Uri "$API_URL/api/job/$JOB_ID" -Method GET -Headers @{"X-API-Key"=$API_KEY} -ErrorAction Stop
            
            # Show progress indicator
            $progressBar = ""
            if ($statusData.totalStations -gt 0) {
                $percentage = [math]::Round(($statusData.processedStations / $statusData.totalStations) * 100)
                $progressBar = " [$percentage%]"
            }
            
            Write-Host "  Check $checkCount : Status=$($statusData.status) | Processed=$($statusData.processedStations)/$($statusData.totalStations)$progressBar | Images=$($statusData.images.Count)" -ForegroundColor Cyan
            
            # Check if completed
            if ($statusData.status -eq "Completed") {
                Write-Host ""
                Write-Host "  [SUCCESS] Job completed successfully!" -ForegroundColor Green
                Write-Host "  Total Images Generated: $($statusData.images.Count)" -ForegroundColor Green
                Write-Host ""
                Write-Host "  Sample Images:" -ForegroundColor Cyan
                $statusData.images | Select-Object -First 5 | ForEach-Object {
                    Write-Host "    - $($_.stationName): $($_.temperature)°C, $($_.weatherDescription)" -ForegroundColor Gray
                }
                if ($statusData.images.Count -gt 5) {
                    Write-Host "    ... and $($statusData.images.Count - 5) more" -ForegroundColor Gray
                }
                break
            }
            
            # Timeout check
            if ($checkCount -ge $maxChecks) {
                Write-Host ""
                Write-Host "  [TIMEOUT] Max monitoring time reached (3 minutes)" -ForegroundColor Yellow
                Write-Host "  Job Status: $($statusData.status)" -ForegroundColor Yellow
                Write-Host "  Progress: $($statusData.processedStations)/$($statusData.totalStations)" -ForegroundColor Yellow
                break
            }
            
        } catch {
            Write-Host "  [ERROR] Failed to check status: $($_.Exception.Message)" -ForegroundColor Red
            break
        }
        
    } while ($true)
} else {
    Write-Host "[SKIP] Test 3 & 4 skipped - no job ID available" -ForegroundColor Yellow
}

Write-Host ""
Write-Host "=== Tests Complete ===" -ForegroundColor Cyan
Write-Host ""