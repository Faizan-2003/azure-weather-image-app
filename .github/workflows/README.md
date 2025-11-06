# GitHub Actions Workflows

This directory contains GitHub Actions workflows for automating deployment to Azure.

## Workflows

### 1. `azure-infrastructure.yml` - Infrastructure Deployment

Deploys the Azure infrastructure using Bicep templates.

**Trigger:** Manual (workflow_dispatch)

**Required Secrets:**

-   `AZURE_CREDENTIALS` - Service Principal credentials for Azure
-   `AZURE_SUBSCRIPTION_ID` - Your Azure subscription ID

**How to Run:**

1. Go to GitHub Actions tab
2. Select "Deploy Azure Infrastructure"
3. Click "Run workflow"
4. Fill in the required parameters:
    - Resource Group Name (default: rg-weather-image-app)
    - Location (default: westeurope)
    - API Key (required)
    - Unsplash Access Key (optional)

### 2. `azure-deploy.yml` - Application Deployment

Automatically builds and deploys the Function App code to Azure.

**Trigger:**

-   Push to `main` branch
-   Manual (workflow_dispatch)

**Required Secrets:**

-   `AZURE_FUNCTIONAPP_PUBLISH_PROFILE` - Publish profile from Azure Function App

**Environment Variables to Update:**

-   `AZURE_FUNCTIONAPP_NAME` - Update with your actual Function App name after infrastructure deployment

---

## Setup Instructions

### Step 1: Create Azure Service Principal

Run this command in Azure Cloud Shell or local Azure CLI:

```bash
az ad sp create-for-rbac \
  --name "github-actions-weather-app" \
  --role contributor \
  --scopes /subscriptions/{subscription-id} \
  --sdk-auth
```

Copy the entire JSON output.

### Step 2: Add GitHub Secrets

Go to your GitHub repository → Settings → Secrets and variables → Actions

Add the following secrets:

1. **AZURE_CREDENTIALS**

    - Paste the entire JSON output from Step 1

2. **AZURE_SUBSCRIPTION_ID**

    - Your Azure subscription ID (found in Azure Portal)

3. **AZURE_FUNCTIONAPP_PUBLISH_PROFILE** (after infrastructure deployment)
    - Go to Azure Portal → Your Function App → Get publish profile
    - Copy the entire XML content

### Step 3: Deploy Infrastructure

1. Go to GitHub Actions tab
2. Run "Deploy Azure Infrastructure" workflow
3. Note the Function App name from the output

### Step 4: Update Function App Name

Edit `.github/workflows/azure-deploy.yml`:

```yaml
env:
    AZURE_FUNCTIONAPP_NAME: "your-actual-function-app-name"
```

### Step 5: Add Publish Profile

1. In Azure Portal, navigate to your Function App
2. Click "Get publish profile" (or "Download publish profile")
3. Copy the entire XML content
4. Add it as `AZURE_FUNCTIONAPP_PUBLISH_PROFILE` secret in GitHub

### Step 6: Push to Main Branch

Any push to `main` branch will now automatically deploy your application!

---

## Manual Deployment Alternative

If you prefer manual deployment, you can still use the PowerShell script:

```powershell
.\deploy.ps1 `
  -ResourceGroupName "rg-weather-image-app" `
  -Location "westeurope" `
  -ApiKey "your-api-key" `
  -UnsplashAccessKey "your-unsplash-key"
```

---

## Troubleshooting

### "Resource 'Microsoft.Web/sites' was not found"

-   Make sure the infrastructure workflow completed successfully first
-   Check that `AZURE_FUNCTIONAPP_NAME` matches your deployed function app

### "Publish profile is invalid"

-   Re-download the publish profile from Azure Portal
-   Ensure the entire XML is copied without modification
-   Update the secret in GitHub

### "Unable to login to Azure"

-   Verify `AZURE_CREDENTIALS` secret is properly formatted JSON
-   Ensure the service principal has contributor access
-   Check if the service principal hasn't expired

---

## Benefits of GitHub Actions

✅ **Automatic Deployment** - Push to main branch triggers deployment
✅ **Consistent Builds** - Same build process every time
✅ **No Local Dependencies** - Runs in GitHub's cloud infrastructure
✅ **Audit Trail** - Every deployment is logged and tracked
✅ **Easy Rollback** - Redeploy any previous commit
