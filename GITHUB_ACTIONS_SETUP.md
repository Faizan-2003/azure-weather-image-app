# GitHub Actions Setup Guide

This guide will help you set up automated deployment using GitHub Actions.

## 🎯 Overview

The project includes two GitHub Actions workflows:

1. **Infrastructure Deployment** (`azure-infrastructure.yml`) - Deploys Azure resources using Bicep
2. **Application Deployment** (`azure-deploy.yml`) - Builds and deploys the Function App code

## 📋 Prerequisites

-   Azure subscription
-   GitHub repository with this code
-   Azure CLI installed (for setup)

---

## 🚀 Setup Instructions

### Step 1: Create Azure Service Principal

This allows GitHub Actions to deploy resources to your Azure subscription.

**Using Azure CLI:**

```bash
az ad sp create-for-rbac \
  --name "github-actions-weather-app" \
  --role contributor \
  --scopes /subscriptions/{YOUR-SUBSCRIPTION-ID} \
  --sdk-auth
```

**Using Azure Cloud Shell:**

1. Go to https://portal.azure.com
2. Click the Cloud Shell icon (>\_) in the top right
3. Run the command above (replace `{YOUR-SUBSCRIPTION-ID}` with your actual subscription ID)

You'll get output like this:

```json
{
  "clientId": "xxxx",
  "clientSecret": "xxxx",
  "subscriptionId": "xxxx",
  "tenantId": "xxxx",
  ...
}
```

**⚠️ Important:** Copy the ENTIRE JSON output - you'll need it in the next step!

---

### Step 2: Add GitHub Repository Secrets

1. Go to your GitHub repository
2. Click **Settings** → **Secrets and variables** → **Actions**
3. Click **New repository secret** and add:

#### Secret 1: `AZURE_CREDENTIALS`

-   **Name:** `AZURE_CREDENTIALS`
-   **Value:** Paste the entire JSON output from Step 1

#### Secret 2: `AZURE_SUBSCRIPTION_ID`

-   **Name:** `AZURE_SUBSCRIPTION_ID`
-   **Value:** Your Azure subscription ID (just the GUID, e.g., `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`)

**Example:**

```
Name: AZURE_SUBSCRIPTION_ID
Value: 12345678-1234-1234-1234-123456789abc
```

---

### Step 3: Deploy Infrastructure

Now that secrets are configured, deploy the Azure infrastructure:

1. Go to your GitHub repository
2. Click the **Actions** tab
3. Select **"Deploy Azure Infrastructure"** from the left sidebar
4. Click **"Run workflow"** button (right side)
5. Fill in the parameters:
    - **Resource Group Name:** `rg-weather-image-app` (or your choice)
    - **Azure Region:** `westeurope` (or your choice)
    - **API Key:** A secure key for your API (e.g., `MySecureKey123!`)
    - **Unsplash Access Key:** (Optional) Your Unsplash API key
6. Click **"Run workflow"**

**Wait for completion (~3-5 minutes)**

7. After completion, click on the workflow run to see the outputs
8. **Copy the Function App Name** from the logs - you'll need it next!

---

### Step 4: Get Function App Publish Profile

1. Go to **Azure Portal** (https://portal.azure.com)
2. Navigate to your Function App (name from Step 3)
3. In the left menu, click **"Get publish profile"** or **"Download publish profile"**
4. Open the downloaded `.PublishSettings` file in a text editor
5. **Copy the ENTIRE XML content**

---

### Step 5: Add Publish Profile to GitHub

Back in your GitHub repository:

1. Go to **Settings** → **Secrets and variables** → **Actions**
2. Click **"New repository secret"**
3. Add:
    - **Name:** `AZURE_FUNCTIONAPP_PUBLISH_PROFILE`
    - **Value:** Paste the entire XML content from the publish profile

---

### Step 6: Update Function App Name in Workflow

1. Open `.github/workflows/azure-deploy.yml` in your repository
2. Find this line:
    ```yaml
    AZURE_FUNCTIONAPP_NAME: "weather-image-func"
    ```
3. Replace `'weather-image-func'` with your actual Function App name from Step 3
4. **Commit and push** the change

---

### Step 7: Test Automatic Deployment 🎉

Now everything is set up! Test it:

1. Make a small change to any file (e.g., add a comment in `README.md`)
2. Commit and push to the `main` branch
3. Go to **Actions** tab - you should see the deployment running automatically!
4. Wait for it to complete (~2-3 minutes)

Your application is now automatically deployed! 🚀

---

## 🔍 Verify Deployment

After deployment, test your endpoints:

```bash
# Get your Function App URL from Azure Portal or workflow output
$baseUrl = "https://your-function-app.azurewebsites.net"
$apiKey = "YourApiKeyFromStep3"

# Test health endpoint
curl "$baseUrl/api/health" -H "X-API-Key: $apiKey"

# Start a job
curl -X POST "$baseUrl/api/job/start" -H "X-API-Key: $apiKey"

# Check job status (replace with actual job ID)
curl "$baseUrl/api/job/{jobId}" -H "X-API-Key: $apiKey"
```

---

## 📊 Summary of Secrets Needed

| Secret Name                         | Where to Get It                     | Purpose                               |
| ----------------------------------- | ----------------------------------- | ------------------------------------- |
| `AZURE_CREDENTIALS`                 | Service Principal creation (Step 1) | Allows GitHub to deploy to Azure      |
| `AZURE_SUBSCRIPTION_ID`             | Azure Portal or CLI output          | Identifies your Azure subscription    |
| `AZURE_FUNCTIONAPP_PUBLISH_PROFILE` | Azure Portal → Function App         | Allows deploying code to Function App |

---

## 🔧 Troubleshooting

### Error: "Resource 'Microsoft.Web/sites' was not found"

-   Make sure the infrastructure workflow completed successfully first
-   Verify `AZURE_FUNCTIONAPP_NAME` in `azure-deploy.yml` matches your actual Function App name

### Error: "Publish profile is invalid"

-   Re-download the publish profile from Azure Portal
-   Ensure you copied the entire XML without any modifications
-   Make sure there are no extra spaces or line breaks

### Error: "Unable to login to Azure"

-   Verify `AZURE_CREDENTIALS` is properly formatted JSON (not truncated)
-   Ensure the service principal has Contributor role on the subscription
-   Check if the service principal hasn't expired

### Workflow doesn't trigger automatically

-   Ensure you're pushing to the `main` branch
-   Check that `.github/workflows/azure-deploy.yml` exists in the `main` branch
-   Verify Actions are enabled in repository settings

---

## 🎓 What Happens on Each Push?

When you push to `main`:

1. ✅ GitHub Actions automatically starts
2. ✅ Checks out your code
3. ✅ Sets up .NET 8
4. ✅ Builds your project in Release mode
5. ✅ Deploys to Azure Functions
6. ✅ Your application is live with the latest changes!

---

## 🌟 Benefits

-   **No manual deployment** - Push and forget!
-   **Consistent builds** - Same process every time
-   **Rollback capability** - Redeploy any previous commit
-   **Audit trail** - All deployments logged in GitHub
-   **Works from anywhere** - No need for local Azure CLI or tools

---

## 📞 Need Help?

-   Check the [GitHub Actions documentation](https://docs.github.com/en/actions)
-   Review the [Azure Functions deployment docs](https://docs.microsoft.com/azure/azure-functions/functions-how-to-github-actions)
-   See detailed workflow logs in the Actions tab

Happy deploying! 🚀
