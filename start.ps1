Write-Host "=== Starting Azure Functions Application ===" -ForegroundColor Cyan
Write-Host ""

# Check if Azurite is running
Write-Host "Checking Azurite..." -ForegroundColor Yellow

# Check if Azurite process exists (check for node.exe running azurite)
$azuriteRunning = Get-Process | Where-Object { $_.ProcessName -eq "node" -and $_.CommandLine -like "*azurite*" } -ErrorAction SilentlyContinue

if (-not $azuriteRunning) {
    # Also check by port 10000 (Blob service)
    $portInUse = Get-NetTCPConnection -LocalPort 10000 -State Listen -ErrorAction SilentlyContinue
    
    if (-not $portInUse) {
        Write-Host "Azurite not running. Starting Azurite..." -ForegroundColor Yellow
        Write-Host "  - Blob Storage: http://127.0.0.1:10000" -ForegroundColor Gray
        Write-Host "  - Queue Storage: http://127.0.0.1:10001 (job-start-queue, image-processing-queue)" -ForegroundColor Gray
        Write-Host "  - Table Storage: http://127.0.0.1:10002 (JobStatus)" -ForegroundColor Gray
        
        # Start Azurite in a new PowerShell window (so you can see logs if needed)
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "azurite --silent --location ./azurite"
        
        Write-Host "Waiting for Azurite to start..." -ForegroundColor Gray
        Start-Sleep -Seconds 3
        Write-Host "SUCCESS: Azurite started in new window" -ForegroundColor Green
    } else {
        Write-Host "SUCCESS: Azurite is already running (port 10000 in use)" -ForegroundColor Green
    }
} else {
    Write-Host "SUCCESS: Azurite is already running" -ForegroundColor Green
}
Write-Host "  - Two queues: job-start-queue -> image-processing-queue" -ForegroundColor Gray
Write-Host ""

# Build the project
Write-Host "Building project..." -ForegroundColor Yellow
$buildResult = dotnet build ssp.csproj --verbosity quiet
if ($LASTEXITCODE -ne 0) {
    Write-Host "ERROR: Build failed" -ForegroundColor Red
    exit 1
}
Write-Host "SUCCESS: Build successful" -ForegroundColor Green
Write-Host ""

# Start Functions host
Write-Host "Starting Azure Functions host..." -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop" -ForegroundColor Gray
Write-Host ""

Set-Location "bin\Debug\net8.0"
func start