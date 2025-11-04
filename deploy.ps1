# Azure Weather Image Function Deployment Script
# This script builds, packages, and deploys the Azure Function app

param(
    [Parameter(Mandatory=$true)]
    [string]$ResourceGroupName,
    
    [Parameter(Mandatory=$true)]
    [string]$Location = "westeurope",
    
    [Parameter(Mandatory=$true)]
    [string]$ApiKey,
    
    [Parameter(Mandatory=$false)]
    [string]$UnsplashAccessKey = "",
    
    [Parameter(Mandatory=$false)]
    [string]$FunctionAppName = ""
)

Write-Host "=== Azure Weather Image Function Deployment ===" -ForegroundColor Cyan

# Check if Azure CLI is installed
if (-not (Get-Command az -ErrorAction SilentlyContinue)) {
    Write-Error "Azure CLI is not installed. Please install it from https://docs.microsoft.com/cli/azure/install-azure-cli"
    exit 1
}

# Check if logged in to Azure
Write-Host "Checking Azure login status..." -ForegroundColor Yellow
$loginStatus = az account show 2>$null
if (-not $loginStatus) {
    Write-Host "Not logged in to Azure. Please log in..." -ForegroundColor Yellow
    az login
}

# Display current subscription
$subscription = az account show --query name -o tsv
Write-Host "Using Azure subscription: $subscription" -ForegroundColor Green

# Step 1: Create Resource Group
Write-Host "`nStep 1: Creating resource group '$ResourceGroupName' in '$Location'..." -ForegroundColor Yellow
az group create --name $ResourceGroupName --location $Location
if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to create resource group"
    exit 1
}
Write-Host "Resource group created successfully" -ForegroundColor Green

# Step 2: Deploy Bicep template
Write-Host "`nStep 2: Deploying Azure resources using Bicep template..." -ForegroundColor Yellow
$deploymentName = "weather-image-deployment-$(Get-Date -Format 'yyyyMMddHHmmss')"

$bicepParams = @{
    apiKey = $ApiKey
}

if ($UnsplashAccessKey) {
    $bicepParams.unsplashAccessKey = $UnsplashAccessKey
}

if ($FunctionAppName) {
    $bicepParams.functionAppName = $FunctionAppName
}

# Convert parameters to JSON
$paramsJson = $bicepParams | ConvertTo-Json -Compress

# Deploy
$deploymentOutput = az deployment group create `
    --name $deploymentName `
    --resource-group $ResourceGroupName `
    --template-file "deploy/main.bicep" `
    --parameters $paramsJson `
    --query "properties.outputs" `
    -o json

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to deploy Bicep template"
    exit 1
}

$outputs = $deploymentOutput | ConvertFrom-Json
$functionAppName = $outputs.functionAppName.value
$functionAppUrl = $outputs.functionAppUrl.value

Write-Host "Azure resources deployed successfully" -ForegroundColor Green
Write-Host "Function App Name: $functionAppName" -ForegroundColor Cyan
Write-Host "Function App URL: $functionAppUrl" -ForegroundColor Cyan

# Step 3: Build the project
Write-Host "`nStep 3: Building the .NET project..." -ForegroundColor Yellow
dotnet clean
dotnet build --configuration Release

if ($LASTEXITCODE -ne 0) {
    Write-Error "Build failed"
    exit 1
}
Write-Host "Build completed successfully" -ForegroundColor Green

# Step 4: Publish the project
Write-Host "`nStep 4: Publishing the project..." -ForegroundColor Yellow
$publishFolder = "publish"
if (Test-Path $publishFolder) {
    Remove-Item -Path $publishFolder -Recurse -Force
}

dotnet publish --configuration Release --output $publishFolder

if ($LASTEXITCODE -ne 0) {
    Write-Error "Publish failed"
    exit 1
}
Write-Host "Project published successfully" -ForegroundColor Green

# Step 5: Create deployment package
Write-Host "`nStep 5: Creating deployment package..." -ForegroundColor Yellow
$zipFile = "deploy.zip"
if (Test-Path $zipFile) {
    Remove-Item $zipFile -Force
}

# Zip the publish folder
Compress-Archive -Path "$publishFolder\*" -DestinationPath $zipFile -Force
Write-Host "Deployment package created: $zipFile" -ForegroundColor Green

# Step 6: Deploy to Azure Functions
Write-Host "`nStep 6: Deploying to Azure Functions..." -ForegroundColor Yellow
az functionapp deployment source config-zip `
    --resource-group $ResourceGroupName `
    --name $functionAppName `
    --src $zipFile

if ($LASTEXITCODE -ne 0) {
    Write-Error "Deployment to Azure Functions failed"
    exit 1
}

Write-Host "Deployment to Azure Functions completed successfully" -ForegroundColor Green

# Step 7: Display endpoint information
Write-Host "`n=== Deployment Complete ===" -ForegroundColor Cyan
Write-Host "Function App URL: $functionAppUrl" -ForegroundColor Green
Write-Host "`nAvailable endpoints:" -ForegroundColor Yellow
Write-Host "  POST $functionAppUrl/api/job/start" -ForegroundColor White
Write-Host "  GET  $functionAppUrl/api/job/{jobId}" -ForegroundColor White
Write-Host "  GET  $functionAppUrl/api/test/image" -ForegroundColor White
Write-Host "`nAPI Key: $ApiKey" -ForegroundColor Yellow
Write-Host "Remember to include the X-API-Key header in all requests!" -ForegroundColor Yellow

# Clean up
Write-Host "`nCleaning up temporary files..." -ForegroundColor Yellow
Remove-Item $zipFile -Force
Remove-Item $publishFolder -Recurse -Force

Write-Host "`nDeployment script completed successfully!" -ForegroundColor Green
