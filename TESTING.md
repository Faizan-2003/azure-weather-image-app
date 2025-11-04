# Testing Guide - Weather Image Application

## 🚀 Quick Start

### Prerequisites

-   .NET 8 SDK installed
-   Azure Functions Core Tools installed
-   Azurite running (local storage emulator)

### Step 1: Start Azurite

Open a PowerShell terminal and run:

```powershell
azurite --silent --location ./azurite
```

Keep this terminal running. Azurite provides local Azure Storage emulation.
All Azurite data files will be stored in the `azurite/` folder.

### Step 2: Run the Application

Open another PowerShell terminal in the project folder:

```powershell
# Navigate to project (if needed)
cd "c:\Users\mf384\OneDrive\Desktop\azure-weather-image-app"

# Start the application (builds and runs automatically)
.\start.ps1
```

Or simply double-click `start.cmd` in Windows Explorer.

You should see output showing all available endpoints:

```
Functions:
    GetJobStatus: [GET] http://localhost:7071/api/job/{jobId}
    HealthCheck: [GET] http://localhost:7071/api/health
    ProcessImage: [QueueTrigger]
    StartJob: [POST] http://localhost:7071/api/job/start
    TestImageProcessing: [GET] http://localhost:7071/api/test/image
```

---

## 🧪 Testing Commands

All commands use API Key: `test-api-key-12345`

### Test 1: Health Check

```powershell
# Simple health check endpoint
Invoke-RestMethod -Uri "http://localhost:7071/api/health" -Headers @{"X-API-Key"="test-api-key-12345"}
```

**Expected Output:**

```json
{
    "status": "healthy",
    "timestamp": "2024-..."
}
```

---

### Test 2: Generate Test Image

```powershell
# Generate a single test image and save it
Invoke-WebRequest -Uri "http://localhost:7071/api/test/image" `
  -Headers @{"X-API-Key"="test-api-key-12345"} `
  -OutFile "test-image.jpg"
```

**Result:** Downloads `test-image.jpg` with weather data overlay

---

### Test 3: Start a Job

```powershell
# Start a new job and capture the response
$response = Invoke-RestMethod -Uri "http://localhost:7071/api/job/start" `
  -Method POST `
  -Headers @{"X-API-Key"="test-api-key-12345"; "Content-Type"="application/json"}

# Display job information
Write-Host "Job ID: $($response.jobId)" -ForegroundColor Green
Write-Host "Status: $($response.status)"
Write-Host "Total Stations: $($response.message)"

# Save jobId for later use
$jobId = $response.jobId
```

**Expected Output:**

```
Job ID: 5476dc80-da32-45b4-b650-edcff02f7aa1
Status: InProgress
Total Stations: Job started with 40 stations
```

---

### Test 4: Check Job Status

```powershell
# Check the status of your job (use the jobId from above)
$jobId = "5476dc80-da32-45b4-b650-edcff02f7aa1"  # Replace with your actual job ID

$status = Invoke-RestMethod -Uri "http://localhost:7071/api/job/$jobId" `
  -Headers @{"X-API-Key"="test-api-key-12345"}

# Display status
$status | Format-List

# Show progress
Write-Host "`nProgress: $($status.processedStations) / $($status.totalStations)" -ForegroundColor Cyan
Write-Host "Images Generated: $($status.images.Count)" -ForegroundColor Cyan
```

**Expected Output:**

```
JobId             : 5476dc80-da32-45b4-b650-edcff02f7aa1
Status            : Processing
TotalStations     : 40
ProcessedStations : 15
Images            : {...}
```

---

### Test 5: View Generated Images

```powershell
# Get job status and display image URLs
$status = Invoke-RestMethod -Uri "http://localhost:7071/api/job/$jobId" `
  -Headers @{"X-API-Key"="test-api-key-12345"}

# Display first 5 images with details
$status.images | Select-Object -First 5 | ForEach-Object {
    Write-Host "`nStation: $($_.stationName)" -ForegroundColor Yellow
    Write-Host "Weather: $($_.weatherDescription)"
    Write-Host "Temperature: $($_.temperature)°C"
    Write-Host "Image URL: $($_.imageUrl)"
}
```

---

### Test 6: Complete Test Flow

```powershell
# Complete end-to-end test
Write-Host "=== Starting Complete Test ===" -ForegroundColor Cyan

# 1. Start job
Write-Host "`n[1/4] Starting new job..." -ForegroundColor Yellow
$response = Invoke-RestMethod -Uri "http://localhost:7071/api/job/start" `
  -Method POST `
  -Headers @{"X-API-Key"="test-api-key-12345"; "Content-Type"="application/json"}
$jobId = $response.jobId
Write-Host "✓ Job started: $jobId" -ForegroundColor Green

# 2. Wait a bit
Write-Host "`n[2/4] Waiting 5 seconds for processing..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# 3. Check status
Write-Host "`n[3/4] Checking job status..." -ForegroundColor Yellow
$status = Invoke-RestMethod -Uri "http://localhost:7071/api/job/$jobId" `
  -Headers @{"X-API-Key"="test-api-key-12345"}
Write-Host "✓ Status: $($status.status)" -ForegroundColor Green
Write-Host "  Processed: $($status.processedStations) / $($status.totalStations)"

# 4. Wait and check again
Write-Host "`n[4/4] Waiting 10 more seconds..." -ForegroundColor Yellow
Start-Sleep -Seconds 10
$status = Invoke-RestMethod -Uri "http://localhost:7071/api/job/$jobId" `
  -Headers @{"X-API-Key"="test-api-key-12345"}
Write-Host "✓ Final Status: $($status.status)" -ForegroundColor Green
Write-Host "  Processed: $($status.processedStations) / $($status.totalStations)"
Write-Host "  Images: $($status.images.Count)" -ForegroundColor Green

Write-Host "`n=== Test Complete ===" -ForegroundColor Cyan
```

---

## 🧪 Automated Test Script

Run the included test script for comprehensive testing:

```powershell
.\test-local.ps1
```

This script tests:

-   ✓ Authentication (API key validation)
-   ✓ Job creation
-   ✓ Job status retrieval
-   ✓ Progress tracking
-   ✓ Image generation

---

## 🔍 Testing Authentication

### Test Unauthorized Access (Should Fail)

```powershell
# Try without API key - should get 401
try {
    Invoke-RestMethod -Uri "http://localhost:7071/api/job/start" -Method POST
} catch {
    Write-Host "✓ Correctly rejected: $($_.Exception.Message)" -ForegroundColor Green
}

# Try with wrong API key - should get 401
try {
    Invoke-RestMethod -Uri "http://localhost:7071/api/job/start" `
      -Method POST `
      -Headers @{"X-API-Key"="wrong-key"}
} catch {
    Write-Host "✓ Correctly rejected: $($_.Exception.Message)" -ForegroundColor Green
}
```

---

## 📊 Monitoring Job Progress

### Continuous Status Monitoring

```powershell
# Monitor job until completion
$jobId = "YOUR-JOB-ID-HERE"

do {
    $status = Invoke-RestMethod -Uri "http://localhost:7071/api/job/$jobId" `
      -Headers @{"X-API-Key"="test-api-key-12345"}

    Write-Host "Status: $($status.status) | Processed: $($status.processedStations)/$($status.totalStations) | Images: $($status.images.Count)" -ForegroundColor Cyan

    if ($status.status -ne "Completed" -and $status.status -ne "Failed") {
        Start-Sleep -Seconds 3
    }
} while ($status.status -eq "Processing" -or $status.status -eq "InProgress")

Write-Host "`n✓ Job finished with status: $($status.status)" -ForegroundColor Green
```

---

## 🌐 Using REST Client (VS Code)

If you have the REST Client extension installed in VS Code, open `api-requests.http` and click "Send Request" on any endpoint.

---

## 🐛 Troubleshooting

### Azurite Not Running

```
Error: "No connection could be made because the target machine actively refused it"
```

**Solution:** Start Azurite in a separate terminal:

```powershell
azurite --silent --location ./azurite
```

### Port Already in Use

```
Error: "Port 7071 is already in use"
```

**Solution:** Stop any running Functions host or change the port:

```powershell
func start --port 7072
```

Then update all test URLs to use `:7072`

### API Key Error

```
Error: "401 Unauthorized"
```

**Solution:** Make sure you include the API key header in all requests:

```powershell
-Headers @{"X-API-Key"="test-api-key-12345"}
```

### Job Not Found

```
Error: "404 Not Found"
```

**Solution:**

1. Make sure Azurite is running
2. Verify the jobId is correct
3. Check that Table Storage is accessible

---

## 📝 API Endpoint Reference

| Method | Endpoint           | Description                | Auth Required |
| ------ | ------------------ | -------------------------- | ------------- |
| GET    | `/api/health`      | Health check               | ✓             |
| GET    | `/api/test/image`  | Generate single test image | ✓             |
| POST   | `/api/job/start`   | Start new job              | ✓             |
| GET    | `/api/job/{jobId}` | Get job status             | ✓             |

**API Key Header:** `X-API-Key: test-api-key-12345`

---

## 🎯 Expected Results

### Successful Job Flow

1. **Start Job** → Returns jobId, status "InProgress"
2. **Immediate Check** → Shows 0-5 images processed
3. **After 10s** → Shows 10-20 images processed
4. **After 30s** → Shows 30-40 images processed, status "Completed"
5. **Final Check** → All 40 images available with SAS URLs

### Typical Processing Time

-   **Total Stations:** 40
-   **Processing Time:** 20-40 seconds (depending on API response times)
-   **Images per Second:** ~1-2

---

## ✅ Test Checklist

-   [ ] Azurite is running
-   [ ] Functions app is running (`func start`)
-   [ ] Health check returns 200 OK
-   [ ] Can generate test image
-   [ ] Can start a new job
-   [ ] Job returns valid jobId
-   [ ] Can retrieve job status
-   [ ] Status shows progress increasing
-   [ ] Images are generated with SAS URLs
-   [ ] Job completes successfully
-   [ ] Authentication blocks unauthorized requests

---

## 🚀 Next Steps

Once local testing is successful:

1. **Deploy to Azure** using `deploy.ps1`
2. **Update API Key** in Azure App Settings
3. **Test production endpoint** with same commands (update URL)
4. **Monitor** using Azure Portal → Function App → Monitor

For deployment instructions, see the **Deployment** section in `README.md`.
