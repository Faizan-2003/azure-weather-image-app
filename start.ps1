# Start the Azure Functions application
# This script handles the build and starts the Functions host

Write-Host "=== Starting Azure Functions Application ===" -ForegroundColor Cyan
Write-Host ""

# Check if Azurite is running
Write-Host "Checking Azurite..." -ForegroundColor Yellow
$azuriteRunning = Get-Process -Name "azurite" -ErrorAction SilentlyContinue
if (-not $azuriteRunning) {
    Write-Host "WARNING: Azurite is not running. Starting Azurite..." -ForegroundColor Yellow
    Start-Process -FilePath "azurite" -ArgumentList "--silent", "--location", "./azurite" -WindowStyle Hidden
    Start-Sleep -Seconds 2
    Write-Host "SUCCESS: Azurite started" -ForegroundColor Green
} else {
    Write-Host "SUCCESS: Azurite is already running" -ForegroundColor Green
}
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
