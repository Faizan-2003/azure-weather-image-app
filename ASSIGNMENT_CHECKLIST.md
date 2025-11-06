# Assignment Requirements Checklist ✅

## MUST Requirements

| #   | Requirement                                            | Status | Implementation Details                                           |
| --- | ------------------------------------------------------ | ------ | ---------------------------------------------------------------- |
| 1   | **Publicly accessible HTTP API for requesting images** | ✅     | `StartJobFunction.cs` - POST `/api/job/start`                    |
| 2   | **QueueTrigger for background processing**             | ✅     | `JobInitiatorFunction.cs` + `ProcessImageFunction.cs`            |
| 3   | **Blob Storage for storing images**                    | ✅     | `BlobStorageService.cs` - uploads to `weather-images` container  |
| 4   | **Use Buienradar API**                                 | ✅     | `WeatherService.cs` - `https://data.buienradar.nl/2.0/feed/json` |
| 5   | **Use public API for images**                          | ✅     | `ImageService.cs` - Unsplash API integration                     |
| 6   | **HTTP API for fetching generated images**             | ✅     | `GetJobStatusFunction.cs` - GET `/api/job/{id}`                  |
| 7   | **HTTP files as API documentation**                    | ✅     | `api-requests.http`                                              |
| 8   | **Bicep template (with queues)**                       | ✅     | `deploy/main.bicep` - includes storage, 2 queues, blobs, tables  |
| 9   | **GitHub repo with collaborator access**               | ⚠️     | **TODO:** Add `triplegh2025` or `triplegithub2025@outlook.com`   |
| 10  | **deploy.ps1 script**                                  | ✅     | `deploy.ps1` - complete deployment automation                    |
| 11  | **Multiple queues**                                    | ✅     | 2 queues: `job-start-queue` → `image-processing-queue`           |
| 12  | **Deploy to Azure**                                    | ⚠️     | **TODO:** Run deployment script or GitHub Actions                |

---

## COULD Requirements (Bonus)

| #   | Requirement                            | Status | Implementation Details                                    |
| --- | -------------------------------------- | ------ | --------------------------------------------------------- |
| 1   | **SAS tokens for blob access**         | ✅     | `BlobStorageService.GetBlobSasUrlAsync()` - 1-hour tokens |
| 2   | **GitHub Actions for CI/CD**           | ✅     | `.github/workflows/` - automated deployment               |
| 3   | **API authentication**                 | ✅     | `ApiKeyAuthMiddleware.cs` - X-API-Key header validation   |
| 4   | **Status endpoint with Table Storage** | ✅     | `TableStorageService.cs` + `GetJobStatusFunction.cs`      |

---

## 📋 Pre-Deployment Checklist

### Before You Deploy:

-   [ ] **Add GitHub Collaborator** (REQUIRED)

    -   Go to: https://github.com/Faizan-2003/azure-weather-image-app/settings/access
    -   Add: `triplegh2025` OR `triplegithub2025@outlook.com`
    -   Role: Write or Admin

-   [ ] **Choose Deployment Method**

    -   [ ] GitHub Actions (recommended for automated deployment)
    -   [ ] PowerShell Script (fastest for one-time deployment)
    -   [ ] Manual Azure CLI

-   [ ] **Prepare Required Information**
    -   [ ] Azure Subscription ID
    -   [ ] API Key (create a secure key)
    -   [ ] Unsplash API Key (optional but recommended)
    -   [ ] Resource Group Name (e.g., `rg-weather-image-app`)
    -   [ ] Azure Region (e.g., `westeurope`)

---

## 🚀 Deployment Steps

### Option A: GitHub Actions

1. [ ] Create Azure Service Principal

    ```bash
    az ad sp create-for-rbac --name "github-actions-weather" \
      --role contributor --scopes /subscriptions/{sub-id} --sdk-auth
    ```

2. [ ] Add GitHub Secrets

    - [ ] `AZURE_CREDENTIALS`
    - [ ] `AZURE_SUBSCRIPTION_ID`

3. [ ] Run "Deploy Azure Infrastructure" workflow

    - [ ] Note the Function App name from output

4. [ ] Add publish profile secret

    - [ ] Download from Azure Portal
    - [ ] Add as `AZURE_FUNCTIONAPP_PUBLISH_PROFILE`

5. [ ] Update Function App name in `azure-deploy.yml`

6. [ ] Push to main branch → automatic deployment!

📖 **Full Guide:** [GITHUB_ACTIONS_SETUP.md](GITHUB_ACTIONS_SETUP.md)

### Option B: PowerShell Script

1. [ ] Login to Azure

    ```powershell
    az login
    ```

2. [ ] Run deployment script

    ```powershell
    .\deploy.ps1 `
      -ResourceGroupName "rg-weather-image-app" `
      -Location "westeurope" `
      -ApiKey "YourSecureKey123!" `
      -UnsplashAccessKey "your-unsplash-key"
    ```

3. [ ] Wait for completion (~5-10 minutes)

4. [ ] Note the Function App URL from output

---

## 🧪 Post-Deployment Testing

After deployment, test all endpoints:

### 1. Health Check

```bash
curl https://your-app.azurewebsites.net/api/health \
  -H "X-API-Key: YourApiKey"
```

### 2. Start a Job

```bash
curl -X POST https://your-app.azurewebsites.net/api/job/start \
  -H "X-API-Key: YourApiKey" \
  -H "Content-Type: application/json"
```

Expected response:

```json
{
    "jobId": "550e8400-e29b-41d4-a716-446655440000",
    "status": "Queued",
    "message": "Job has been queued for processing"
}
```

### 3. Check Job Status

```bash
curl https://your-app.azurewebsites.net/api/job/{jobId} \
  -H "X-API-Key: YourApiKey"
```

Expected response:

```json
{
  "jobId": "550e8400-e29b-41d4-a716-446655440000",
  "status": "InProgress",
  "totalStations": 50,
  "processedStations": 25,
  "images": [...]
}
```

### 4. Test Image Processing

```bash
curl https://your-app.azurewebsites.net/api/test/image \
  -H "X-API-Key: YourApiKey" \
  --output test-image.jpg
```

---

## 📝 What to Submit

### GitHub Repository Should Include:

-   [x] All source code files
-   [x] `deploy.ps1` script
-   [x] Bicep templates (`deploy/main.bicep`)
-   [x] API documentation (`api-requests.http`)
-   [x] README with instructions
-   [x] GitHub Actions workflows (`.github/workflows/`)
-   [ ] **Collaborator access** for reviewer

### Information to Provide to Reviewer:

Create a file called `REVIEWER_INFO.md` with:

```markdown
# Reviewer Information

## Azure Function App Details

-   **Function App URL:** https://your-app-name.azurewebsites.net
-   **API Key:** YourSecureApiKey123!

## Available Endpoints

1. **Start Job**

    - POST `https://your-app-name.azurewebsites.net/api/job/start`
    - Headers: `X-API-Key: YourSecureApiKey123!`

2. **Get Job Status**

    - GET `https://your-app-name.azurewebsites.net/api/job/{jobId}`
    - Headers: `X-API-Key: YourSecureApiKey123!`

3. **Test Image**
    - GET `https://your-app-name.azurewebsites.net/api/test/image`
    - Headers: `X-API-Key: YourSecureApiKey123!`

## GitHub Repository

-   **Repository:** https://github.com/Faizan-2003/azure-weather-image-app
-   **Collaborator Access:** Added ✅

## Architecture Highlights

-   2 Queue system (job-start-queue → image-processing-queue)
-   Fan-out pattern for 50 weather stations
-   SAS token-based blob access
-   Table Storage for job tracking
-   API Key authentication on all endpoints
-   GitHub Actions for CI/CD
```

---

## ✅ Final Verification

Before submission, verify:

-   [ ] All code is committed and pushed to GitHub
-   [ ] Reviewer has collaborator access to the repository
-   [ ] Application is deployed and running in Azure
-   [ ] All three endpoints are working (tested)
-   [ ] API Key is configured and working
-   [ ] Images are being generated successfully
-   [ ] SAS tokens are working for image URLs
-   [ ] Job status is being tracked in Table Storage
-   [ ] GitHub Actions workflows are configured (optional but done!)
-   [ ] Documentation is complete (README, TESTING, API docs)

---

## 🎉 Success Criteria

Your project successfully meets ALL requirements when:

✅ Reviewer can access GitHub repository  
✅ Reviewer can call `/api/job/start` and get a job ID  
✅ Reviewer can call `/api/job/{id}` and see progress  
✅ Images are generated for 50 weather stations  
✅ Images are accessible via SAS URLs  
✅ All endpoints require API Key  
✅ Bicep template deploys all resources  
✅ deploy.ps1 script works  
✅ Code uses 2 queues with fan-out pattern

---

## 📞 Troubleshooting

### Deployment Issues

-   Check Azure CLI is installed and logged in
-   Verify subscription has sufficient quota
-   Check resource names are unique

### Runtime Issues

-   Check Application Insights logs in Azure Portal
-   Verify storage account connection strings
-   Test API Key is correct
-   Ensure queues are created

### GitHub Actions Issues

-   Verify all secrets are added correctly
-   Check service principal permissions
-   Ensure Function App name matches in workflow

---

## 🌟 You're Ready!

Your project:

-   ✅ Meets ALL MUST requirements
-   ✅ Implements ALL COULD requirements (bonus!)
-   ✅ Has comprehensive documentation
-   ✅ Includes automated deployment
-   ✅ Follows best practices

**Next Steps:**

1. Add GitHub collaborator
2. Deploy to Azure
3. Test all endpoints
4. Create REVIEWER_INFO.md
5. Submit! 🚀
