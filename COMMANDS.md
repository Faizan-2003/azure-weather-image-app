# Command Reference - Copy & Paste Ready

## 🏃 Quick Commands

### First Time Setup

```powershell
# Navigate to project
cd "c:\Users\mf384\OneDrive\Desktop\ssp-assignment"

# Restore packages
dotnet restore ssp.csproj

# Build project
dotnet build ssp.csproj

# Start Azurite (in separate terminal)
azurite --silent

# Run the application
func start
```

### Testing the API (PowerShell)

```powershell
# Test image generation
Invoke-WebRequest -Uri "http://localhost:7071/api/test/image" `
  -Headers @{"X-API-Key"="test-api-key-12345"} `
  -OutFile "test-image.jpg"

# Start a job
$response = Invoke-RestMethod -Uri "http://localhost:7071/api/job/start" `
  -Method POST `
  -Headers @{"X-API-Key"="test-api-key-12345"; "Content-Type"="application/json"}

Write-Host "Job ID: $($response.jobId)"

# Wait a bit
Start-Sleep -Seconds 5

# Check status
$jobId = $response.jobId
Invoke-RestMethod -Uri "http://localhost:7071/api/job/$jobId" `
  -Headers @{"X-API-Key"="test-api-key-12345"}
```

### Testing the API (Git Bash / WSL)

```bash
# Test image generation
curl -X GET "http://localhost:7071/api/test/image" \
  -H "X-API-Key: test-api-key-12345" \
  --output test-image.jpg

# Start a job
curl -X POST "http://localhost:7071/api/job/start" \
  -H "X-API-Key: test-api-key-12345" \
  -H "Content-Type: application/json"

# Check status (replace JOB_ID)
curl -X GET "http://localhost:7071/api/job/JOB_ID" \
  -H "X-API-Key: test-api-key-12345"
```

### Run Automated Tests

```bash
bash test-local.sh
bash test-features.sh
```

## 🚀 Deployment Commands

### Deploy to Azure

```powershell
# Full deployment
.\deploy.ps1 `
  -ResourceGroupName "rg-weather-image-app" `
  -Location "westeurope" `
  -ApiKey "your-secure-api-key-here"
```

### With Unsplash API Key

```powershell
.\deploy.ps1 `
  -ResourceGroupName "rg-weather-image-app" `
  -Location "westeurope" `
  -ApiKey "your-secure-api-key-here" `
  -UnsplashAccessKey "your-unsplash-key"
```

### Manual Deployment Steps

```powershell
# Login to Azure
az login

# Create resource group
az group create --name rg-weather-image --location westeurope

# Deploy Bicep
az deployment group create `
  --name weather-deployment `
  --resource-group rg-weather-image `
  --template-file deploy/main.bicep `
  --parameters apiKey="your-key"

# Build and publish
dotnet publish --configuration Release --output publish

# Package
Compress-Archive -Path "publish\*" -DestinationPath deploy.zip -Force

# Deploy (replace FUNCTION_APP_NAME)
az functionapp deployment source config-zip `
  --resource-group rg-weather-image `
  --name FUNCTION_APP_NAME `
  --src deploy.zip
```

## 🔧 Troubleshooting Commands

### Check Azurite Status

```powershell
# List Azurite processes
Get-Process azurite

# Kill Azurite
taskkill /F /IM azurite.exe

# Restart Azurite
azurite --silent
```

### Clean and Rebuild

```powershell
# Clean
dotnet clean

# Remove bin and obj
Remove-Item -Path bin,obj -Recurse -Force

# Restore and build
dotnet restore ssp.csproj
dotnet build ssp.csproj
```

### Check Azure Function App Logs

```powershell
# Stream logs (replace FUNCTION_APP_NAME)
az webapp log tail `
  --name FUNCTION_APP_NAME `
  --resource-group rg-weather-image
```

### View Storage Explorer Data

```powershell
# Install Azure Storage Explorer (if not installed)
winget install Microsoft.AzureStorageExplorer

# Launch and connect to Azurite:
# - Local & Attached > Storage Accounts > Emulator - Default Ports
```

## 📦 Git Commands

### Initial Setup

```powershell
# Initialize repo
git init

# Add all files
git add .

# First commit
git commit -m "Initial commit: Complete weather image app"

# Connect to GitHub (replace YOUR_USERNAME)
git remote add origin https://github.com/YOUR_USERNAME/ssp-assignment.git

# Push
git branch -M main
git push -u origin main
```

### Update and Push Changes

```powershell
# Stage changes
git add .

# Commit
git commit -m "Your commit message"

# Push
git push
```

### Check Status

```powershell
# See what's changed
git status

# See commit history
git log --oneline

# See remote URL
git remote -v
```

## 🧪 Verify Installation

### Check Prerequisites

```powershell
# Check .NET version (should be 8.0 or higher)
dotnet --version

# Check Azure Functions Core Tools
func --version

# Check Azure CLI
az --version

# Check Node.js (for Azurite)
node --version

# Check if Azurite is installed
npm list -g azurite
```

### Install Missing Tools

```powershell
# Install .NET 8 SDK
winget install Microsoft.DotNet.SDK.8

# Install Azure Functions Core Tools
npm install -g azure-functions-core-tools@4

# Install Azurite
npm install -g azurite

# Install Azure CLI
winget install Microsoft.AzureCLI

# Install Git (if not installed)
winget install Git.Git
```

## 📊 Monitor Application

### Watch Function Logs

```powershell
# In the terminal where func start is running, you'll see:
# - Incoming HTTP requests
# - Queue message processing
# - Any errors or warnings
```

### Check Azurite

```powershell
# View queue messages
# Use Azure Storage Explorer connected to local emulator
# Navigate to: Queues > image-processing-queue

# View blob containers
# Navigate to: Blob Containers > weather-images

# View tables
# Navigate to: Tables > JobStatus
```

## 🎯 Complete Workflow Example

```powershell
# 1. Start Azurite (Terminal 1)
azurite --silent

# 2. Start Function App (Terminal 2)
cd "c:\Users\mf384\OneDrive\Desktop\ssp-assignment"
func start

# 3. Test the app (Terminal 3)
# Start a job
$job = Invoke-RestMethod -Uri "http://localhost:7071/api/job/start" `
  -Method POST `
  -Headers @{"X-API-Key"="test-api-key-12345"; "Content-Type"="application/json"}

Write-Host "Job started: $($job.jobId)" -ForegroundColor Green

# Wait for processing
Write-Host "Waiting 10 seconds for processing..." -ForegroundColor Yellow
Start-Sleep -Seconds 10

# Check status
$status = Invoke-RestMethod -Uri "http://localhost:7071/api/job/$($job.jobId)" `
  -Headers @{"X-API-Key"="test-api-key-12345"}

Write-Host "Processed: $($status.processedStations)/$($status.totalStations)" -ForegroundColor Cyan
Write-Host "Status: $($status.status)" -ForegroundColor Cyan
Write-Host "Images: $($status.images.Count)" -ForegroundColor Green

# Show first image URL
if ($status.images.Count -gt 0) {
    Write-Host "First image: $($status.images[0].imageUrl)" -ForegroundColor Green
}
```

## 📝 Quick Reference

| Action  | Command                                                                          |
| ------- | -------------------------------------------------------------------------------- |
| Build   | `dotnet build ssp.csproj`                                                        |
| Run     | `func start` or `dotnet run`                                                     |
| Test    | `bash test-local.sh`                                                             |
| Deploy  | `.\deploy.ps1 -ResourceGroupName "rg-name" -Location "westeurope" -ApiKey "key"` |
| Clean   | `dotnet clean`                                                                   |
| Restore | `dotnet restore ssp.csproj`                                                      |

---

**Keep this reference handy! 📌**
