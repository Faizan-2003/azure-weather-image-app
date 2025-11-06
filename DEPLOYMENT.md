# Deployment Guide

## Prerequisites

-   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
-   [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
-   Azure subscription

## Option 1: Automated (deploy.ps1)

```powershell
.\deploy.ps1 `
    -ResourceGroupName "rg-weather-app" `
    -Location "swedencentral" `
    -ApiKey "your-secure-key-123"
```

**What it does:**

1. Creates resource group
2. Deploys Bicep template (infrastructure)
3. Builds .NET project
4. Publishes to Azure Functions

**Parameters:**

-   `-ResourceGroupName` - Azure resource group name (required)
-   `-Location` - Azure region (required)
-   `-ApiKey` - API key for authentication (required)
-   `-UnsplashAccessKey` - Optional Unsplash API key
-   `-FunctionAppName` - Optional custom function app name

## Option 2: GitHub Actions

Push to main branch - automatic deployment via `.github/workflows/azure-deploy.yml`

**Required Secrets:**

-   `AZURE_FUNCTIONAPP_PUBLISH_PROFILE` - Download from Azure Portal

## Manual Deployment Steps

```bash
# 1. Login to Azure
az login

# 2. Create resource group
az group create --name rg-weather-app --location swedencentral

# 3. Deploy infrastructure
az deployment group create \
  --resource-group rg-weather-app \
  --template-file deploy/main.bicep \
  --parameters apiKey=your-key-here

# 4. Build and publish
dotnet publish --configuration Release --output ./publish

# 5. Create deployment package
Compress-Archive -Path ./publish/* -DestinationPath deploy.zip

# 6. Deploy to Azure Functions
az functionapp deployment source config-zip \
  --resource-group rg-weather-app \
  --name your-function-app-name \
  --src deploy.zip
```

## Bicep Template

The `deploy/main.bicep` creates:

-   Storage Account (blobs, queues, tables)
-   Function App (Consumption plan)
-   Application Insights

## Configuration

After deployment, set these app settings:

-   `ApiKey` - Your API key
-   `UnsplashAccessKey` - (Optional) Unsplash API key

```bash
az functionapp config appsettings set \
  --resource-group rg-weather-app \
  --name your-function-app \
  --settings ApiKey=your-key
```

## Verify Deployment

```bash
# Test health endpoint
curl https://your-function-app.azurewebsites.net/api/HealthCheck

# Open web UI
https://your-function-app.azurewebsites.net/api/ServeWebsite
```

## Troubleshooting

**Build fails:** Ensure .NET 8 SDK is installed  
**Deployment fails:** Check Azure CLI is logged in (`az account show`)  
**Functions not working:** Check Application Insights logs in Azure Portal
