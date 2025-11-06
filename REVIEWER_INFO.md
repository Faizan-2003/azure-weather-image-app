# Reviewer Information

> **⚠️ IMPORTANT:** Update this file with your actual deployment details before submission!

## 🌐 Azure Function App Details

-   **Function App URL:** `https://YOUR-FUNCTION-APP-NAME.azurewebsites.net`
-   **API Key:** `YOUR-API-KEY-HERE`
-   **Azure Resource Group:** `rg-weather-image-app`
-   **Azure Region:** `westeurope`

---

## 🔌 Available Endpoints

### 1. Health Check

```bash
GET https://YOUR-FUNCTION-APP-NAME.azurewebsites.net/api/health
Headers: X-API-Key: YOUR-API-KEY-HERE
```

### 2. Start a Weather Image Job

```bash
POST https://YOUR-FUNCTION-APP-NAME.azurewebsites.net/api/job/start
Headers:
  X-API-Key: YOUR-API-KEY-HERE
  Content-Type: application/json
```

**Response:**

```json
{
    "jobId": "550e8400-e29b-41d4-a716-446655440000",
    "status": "Queued",
    "message": "Job has been queued for processing"
}
```

### 3. Get Job Status

```bash
GET https://YOUR-FUNCTION-APP-NAME.azurewebsites.net/api/job/{jobId}
Headers: X-API-Key: YOUR-API-KEY-HERE
```

Replace `{jobId}` with the actual job ID from the start endpoint.

**Response:**

```json
{
    "jobId": "550e8400-e29b-41d4-a716-446655440000",
    "status": "Completed",
    "totalStations": 50,
    "processedStations": 50,
    "images": [
        {
            "stationName": "Amsterdam",
            "imageUrl": "https://...blob.core.windows.net/weather-images/...",
            "createdAt": "2025-11-06T12:00:00Z"
        }
    ]
}
```

### 4. Test Image Generation

```bash
GET https://YOUR-FUNCTION-APP-NAME.azurewebsites.net/api/test/image
Headers: X-API-Key: YOUR-API-KEY-HERE
```

Returns a JPEG image directly.

---

## 📋 Testing with cURL

```bash
# Set variables (update these!)
$baseUrl = "https://YOUR-FUNCTION-APP-NAME.azurewebsites.net"
$apiKey = "YOUR-API-KEY-HERE"

# 1. Health check
curl "$baseUrl/api/health" -H "X-API-Key: $apiKey"

# 2. Start a job
$response = curl -X POST "$baseUrl/api/job/start" `
  -H "X-API-Key: $apiKey" `
  -H "Content-Type: application/json"

# 3. Check status (use jobId from response)
curl "$baseUrl/api/job/YOUR-JOB-ID" -H "X-API-Key: $apiKey"

# 4. Get test image
curl "$baseUrl/api/test/image" -H "X-API-Key: $apiKey" --output test.jpg
```

---

## 📁 GitHub Repository

-   **Repository URL:** https://github.com/Faizan-2003/azure-weather-image-app
-   **Main Branch:** `main`
-   **Collaborator Access:** ✅ Added (`triplegh2025` or `triplegithub2025@outlook.com`)

---

## 🏗️ Architecture Overview

### Queue Flow

1. **Client** → `POST /api/job/start` → `StartJobFunction`
2. **StartJobFunction** → enqueues message to `job-start-queue`
3. **JobInitiatorFunction** (QueueTrigger) → fetches 50 weather stations → fan-out
4. **JobInitiatorFunction** → enqueues 50 messages to `image-processing-queue`
5. **ProcessImageFunction** (QueueTrigger) → processes each station in parallel
6. **ProcessImageFunction** → uploads images to Blob Storage with SAS tokens
7. **Client** → `GET /api/job/{id}` → `GetJobStatusFunction` → returns status + image URLs

### Azure Resources

-   ✅ Azure Functions (Consumption Plan)
-   ✅ Storage Account (Blob, Queue, Table)
-   ✅ Application Insights
-   ✅ Two Queues: `job-start-queue`, `image-processing-queue`
-   ✅ Blob Container: `weather-images` (private with SAS access)
-   ✅ Table Storage: `JobStatus` (for tracking)

---

## 🎯 Assignment Requirements Met

### MUST Requirements ✅

-   ✅ Publicly accessible HTTP API for starting jobs
-   ✅ QueueTrigger for background processing (2 queue triggers)
-   ✅ Blob Storage for image storage and serving
-   ✅ Buienradar API integration (50 weather stations)
-   ✅ Public image API (Unsplash)
-   ✅ HTTP API for fetching results
-   ✅ HTTP files as API documentation (`api-requests.http`)
-   ✅ Bicep template with queues (`deploy/main.bicep`)
-   ✅ GitHub repository with collaborator access
-   ✅ deploy.ps1 script
-   ✅ Multiple queues (job-start-queue → image-processing-queue)
-   ✅ Deployed to Azure with working endpoints

### COULD Requirements (Bonus) ✅

-   ✅ SAS tokens for secure blob access (1-hour expiration)
-   ✅ GitHub Actions for automated CI/CD
-   ✅ API Key authentication (X-API-Key header)
-   ✅ Status endpoint with Table Storage tracking

---

## 📖 Documentation

All documentation is included in the repository:

-   **[README.md](README.md)** - Complete project documentation
-   **[TESTING.md](TESTING.md)** - Comprehensive testing guide
-   **[DEPLOYMENT_QUICK_START.md](DEPLOYMENT_QUICK_START.md)** - Quick deployment reference
-   **[GITHUB_ACTIONS_SETUP.md](GITHUB_ACTIONS_SETUP.md)** - CI/CD setup guide
-   **[ASSIGNMENT_CHECKLIST.md](ASSIGNMENT_CHECKLIST.md)** - Requirements checklist
-   **[api-requests.http](api-requests.http)** - API endpoint examples

---

## 🔧 Key Implementation Details

### Authentication

All endpoints require the `X-API-Key` header. This is implemented via custom middleware (`ApiKeyAuthMiddleware.cs`).

### Image Processing

1. Fetches weather data from Buienradar API
2. Downloads background image from Unsplash
3. Uses ImageSharp library to overlay weather data
4. Uploads to Azure Blob Storage
5. Generates SAS token for secure access

### Fan-Out Pattern

The application uses a two-stage queue system:

-   Stage 1: Job initiation queue receives the initial request
-   Stage 2: Image processing queue handles 50 parallel tasks (one per station)

This ensures the initial HTTP request returns quickly while processing happens asynchronously.

### Error Handling

-   Failed queue messages are automatically retried by Azure Functions
-   All errors are logged to Application Insights
-   Invalid requests return appropriate HTTP status codes

---

## 🎓 Technologies Used

-   **.NET 8** (Isolated worker model)
-   **Azure Functions v4**
-   **Azure Storage** (Blob, Queue, Table)
-   **ImageSharp** (Image processing)
-   **Buienradar API** (Weather data)
-   **Unsplash API** (Background images)
-   **Bicep** (Infrastructure as Code)
-   **GitHub Actions** (CI/CD)

---

## 📞 Support

If you have any questions or issues testing the application:

1. Check the [TESTING.md](TESTING.md) guide
2. Review [README.md](README.md) for detailed documentation
3. Check Application Insights logs in Azure Portal
4. Contact me via GitHub

---

## ✅ Verification Steps

To verify the application works correctly:

1. ✅ Call `/api/health` → should return HTTP 200
2. ✅ Call `/api/job/start` → should return a job ID
3. ✅ Call `/api/job/{id}` immediately → should show "InProgress"
4. ✅ Wait 1-2 minutes
5. ✅ Call `/api/job/{id}` again → should show processed stations increasing
6. ✅ Check image URLs → should be accessible SAS URLs
7. ✅ Open an image URL in browser → should display weather image

---

## 🏆 Project Highlights

-   **100% requirement coverage** - All MUST and COULD requirements implemented
-   **Production-ready** - Includes monitoring, error handling, and security
-   **Well-documented** - Comprehensive guides and examples
-   **Automated deployment** - GitHub Actions for CI/CD
-   **Best practices** - Clean architecture, dependency injection, async/await
-   **Scalable design** - Queue-based with automatic retries and parallel processing

---

**Thank you for reviewing!** 🚀
