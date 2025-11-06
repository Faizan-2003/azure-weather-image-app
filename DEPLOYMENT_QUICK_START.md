# Quick Deployment Guide

## 🚀 Three Ways to Deploy

### 1️⃣ GitHub Actions (Automated - Best for Production)

**One-time Setup:**

```bash
# 1. Create service principal
az ad sp create-for-rbac --name "github-actions-weather" \
  --role contributor --scopes /subscriptions/{sub-id} --sdk-auth

# 2. Add secrets to GitHub (Settings → Secrets):
#    - AZURE_CREDENTIALS (JSON from step 1)
#    - AZURE_SUBSCRIPTION_ID
#    - AZURE_FUNCTIONAPP_PUBLISH_PROFILE (from Azure Portal)
```

**Deploy:**

-   Push to `main` branch → Automatic deployment! ✨

📖 **Full Guide:** [GITHUB_ACTIONS_SETUP.md](GITHUB_ACTIONS_SETUP.md)

---

### 2️⃣ PowerShell Script (Fastest)

```powershell
.\deploy.ps1 `
  -ResourceGroupName "rg-weather-image-app" `
  -Location "westeurope" `
  -ApiKey "YourSecureKey123!" `
  -UnsplashAccessKey "optional-unsplash-key"
```

Takes ~5-10 minutes. Everything automated.

---

### 3️⃣ Manual (Step-by-Step)

```powershell
# 1. Login
az login

# 2. Deploy infrastructure
az deployment group create \
  --name weather-deploy \
  --resource-group rg-weather-image-app \
  --template-file deploy/main.bicep \
  --parameters apiKey="YourKey123!"

# 3. Build & deploy
dotnet publish -c Release -o publish
Compress-Archive -Path "publish\*" -DestinationPath deploy.zip
az functionapp deployment source config-zip \
  --resource-group rg-weather-image-app \
  --name <function-app-name> \
  --src deploy.zip
```

---

## 📋 Required Information

| Item               | Example                | Notes                      |
| ------------------ | ---------------------- | -------------------------- |
| **Resource Group** | `rg-weather-image-app` | Choose any name            |
| **Location**       | `westeurope`           | Azure region               |
| **API Key**        | `MySecure123!`         | For endpoint auth          |
| **Unsplash Key**   | `optional`             | For real images (optional) |

---

## 🧪 Test After Deployment

```bash
# Replace with your actual values
$url = "https://your-app.azurewebsites.net"
$key = "YourApiKey"

# Start a job
curl -X POST "$url/api/job/start" -H "X-API-Key: $key"

# Check status (use jobId from response)
curl "$url/api/job/{jobId}" -H "X-API-Key: $key"
```

---

## 📁 Key Files

-   **[GITHUB_ACTIONS_SETUP.md](GITHUB_ACTIONS_SETUP.md)** - Complete GitHub Actions guide
-   **[README.md](README.md)** - Full documentation
-   **[TESTING.md](TESTING.md)** - Testing guide
-   **[api-requests.http](api-requests.http)** - API examples
-   **[deploy.ps1](deploy.ps1)** - Deployment script
-   **[deploy/main.bicep](deploy/main.bicep)** - Infrastructure as Code

---

## ✅ Assignment Requirements Met

### MUST ✅

-   ✅ Public HTTP API for starting jobs
-   ✅ QueueTrigger for background processing
-   ✅ Blob Storage for images
-   ✅ Buienradar API integration
-   ✅ Public image API (Unsplash)
-   ✅ Status endpoint
-   ✅ HTTP files for documentation
-   ✅ Bicep template with queues
-   ✅ deploy.ps1 script
-   ✅ Two queues (job-start → image-processing)

### COULD ✅

-   ✅ SAS tokens for blob access
-   ✅ GitHub Actions for CI/CD
-   ✅ API Key authentication
-   ✅ Status tracking in Table Storage

---

## 🎯 Next Steps

1. **Choose deployment method** (GitHub Actions recommended)
2. **Deploy to Azure**
3. **Test endpoints**
4. **Share results** with your team! 🎉

Need help? Check the detailed guides linked above!
