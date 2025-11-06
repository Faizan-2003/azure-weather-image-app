# Azure Weather Image Application# Azure Weather Image Application# Azure Weather Image Application# Weather Image Application - Azure Functions

**Student:** Muhammad Faizan | **Number:** 701765 **Student:** Muhammad Faizan **Student:** Muhammad Faizan **Student Name:** Muhammad Faizan

**Repository:** https://github.com/Faizan-2003/azure-weather-image-app

**Student Number:** 701765

---

**GitHub:** [https://github.com/Faizan-2003/azure-weather-image-app](https://github.com/Faizan-2003/azure-weather-image-app)**Student Number:** 701765 **Student Number:** 701765

## 📋 What It Does

---**GitHub Repository:** [https://github.com/Faizan-2003/azure-weather-image-app](https://github.com/Faizan-2003/azure-weather-image-app)**GitHub Repository:** https://github.com/Faizan-2003/azure-weather-image-app

Serverless Azure Functions app that generates weather images for Dutch weather stations:

-   Fetches real-time weather from Buienradar API## 📋 Overview---

-   Generates images with weather overlays

-   Uses Queue Storage for background processingA serverless Azure Functions application that generates weather-themed images for Dutch weather stations using real-time data from Buienradar API.## 📋 Project Description - A serverless application built with Azure Functions that generates weather-themed images for Dutch weather stations. The application fetches real-time weather data from Buienradar API and overlays it on beautiful background images.

-   Stores images in Blob Storage

-   Tracks jobs in Table Storage## 🚀 Live DemoA serverless Azure Functions application that generates weather-themed images for Dutch weather stations. The application:## 🌟 Features

## 🚀 Live Demo**🌐 Web Interface:** [https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/ServeWebsite](https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/ServeWebsite)- Fetches real-time weather data from Buienradar API

**Web UI:** https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/ServeWebsite- ✅ No setup required - just open and click "Start Weather Job"- Generates images with weather information overlays- **HTTP API** for starting jobs and checking status

Just open the link and click "Start Weather Job" - everything is pre-configured!- ✅ Real-time progress tracking

## 📡 API Endpoints- ✅ View generated weather images- Uses Azure Queue Storage for scalable background processing- **Queue-based processing** for scalable background image generation

| Endpoint | Method | Auth | Description |- ✅ API key pre-configured

|----------|--------|------|-------------|

| `/api/HealthCheck` | GET | No | Health check |- Stores images in Azure Blob Storage with SAS token access- **Blob Storage** for secure image storage with SAS token access

| `/api/StartJob` | POST | Yes | Start job |

| `/api/GetJobStatus?jobId={id}` | GET | Yes | Check status |## 🎯 Features

**API Key:** `test-api-key-12345` (use as `X-API-Key` header)- Tracks job status using Azure Table Storage- **Table Storage** for persistent job tracking

## 🛠️ Quick Test## 🏗️ Architecture

````bash ││   Client    │

# Start a job

curl -X POST -H "X-API-Key: test-api-key-12345" \```

  https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/StartJob

Client → StartJob → job-start-queue → JobInitiator         │ POST /api/StartJob└──────┬──────┘

# Check status (replace {jobId})

curl -H "X-API-Key: test-api-key-12345" \                          ↓

  "https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/GetJobStatus?jobId={jobId}"

```                  Creates 10 messages         ▼       │



## 🏗️ Architecture                          ↓



```              image-processing-queue┌──────────────────────┐         ┌────────────────────┐       │ POST /api/job/start

Client → StartJob → job-start-queue → JobInitiator

                         ↓                          ↓

                   10 messages

                         ↓                 ProcessImageFunction│ StartJobFunction     ├────────►│ job-start-queue    │       ▼

              image-processing-queue

                         ↓                    ↓           ↓

                  ProcessImage (parallel)

                         ↓              Blob Storage   Table Storage│ (Creates job entry)  │         └─────────┬──────────┘┌─────────────────┐         ┌──────────────┐

                   Blob Storage

                         ↓                              ↓

                  Table Storage ← GetJobStatus

```                         GetJobStatus└──────────────────────┘                   ││ StartJobFunction├────────►│ Queue Storage│



## 📦 Azure Resources```



- **Function App:** weather-image-func-eg2kg4p2kzwtc         │                                  │└─────────────────┘         └──────┬───────┘

- **Storage:** stweathereg2kg4p2kzwtc

- **Region:** Sweden Central## 📡 API Endpoints

- **Resource Group:** StudentGroup

         │                                  ▼       │                           │

## ✅ Assignment Requirements

| Endpoint | Method | Auth | Description |

- [x] Multiple Azure Functions (7 total)

- [x] Queue Storage (2 queues)|----------|--------|------|-------------| │ ┌────────────────────┐ │ │ Messages (50)

- [x] Blob Storage (image storage)

- [x] Table Storage (job tracking)| `/api/HealthCheck` | GET | No | Health status |

- [x] API Key authentication

- [x] SAS tokens for blobs| `/api/ServeWebsite` | GET | No | Web UI | │ │ JobInitiatorFunction│ ▼ ▼

- [x] Fan-out/fan-in pattern

- [x] Bicep IaC template| `/api/StartJob` | POST | Yes | Start job |

- [x] deploy.ps1 script

- [x] GitHub Actions CI/CD| `/api/GetJobStatus?jobId={id}` | GET | Yes | Job status | │ │ (Fan-out pattern) │┌─────────────────┐ ┌──────────────────┐

- [x] Web UI for testing

| `/api/test/image` | GET | No | Test image |

## 📁 Project Structure

         │                         └─────────┬───────────┘│ Table Storage   │◄────────┤ProcessImageFunction│

````

├── Functions/ # 7 Azure Functions**API Key:** `test-api-key-12345` (Header: `X-API-Key`)

├── Services/ # Business logic

├── Models/ # Data models │ ││ (Job State) │ └──────────┬─────────┘

├── Middleware/ # API key auth

├── wwwroot/ # Web UI## 🧪 Quick Test

├── deploy/ # Bicep template

├── deploy.ps1 # Deployment script │ Creates 10 messages└─────────────────┘ │

└── .github/workflows/ # CI/CD

`````### Using cURL:



## 🚢 Deployment````bash │                                   │       ▲                               ▼



See [DEPLOYMENT.md](DEPLOYMENT.md) for deployment instructions.# Start job



Quick deploy:curl -X POST -H "X-API-Key: test-api-key-12345" \         │                                   ▼       │                        ┌──────────────┐

```powershell

.\deploy.ps1 -ResourceGroupName "rg-name" -Location "swedencentral" -ApiKey "your-key"  https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/StartJob

`````

         │                         ┌─────────────────────┐       │                        │ Blob Storage │

## 🔧 Local Development

# Check status (replace {jobId})

````bash

# Install dependenciescurl -H "X-API-Key: test-api-key-12345" \         │                         │image-processing-queue│       │                        │   (Images)   │

dotnet restore

  "https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/GetJobStatus?jobId={jobId}"

# Start Azurite (separate terminal)

azurite --silent --location ./azurite```         │                         └─────────┬────────────┘       │                        └──────────────┘



# Run locally

func start

```### Using PowerShell:         │                                   │       │



Access: http://localhost:7071/api/ServeWebsite```powershell



---$headers = @{ "X-API-Key" = "test-api-key-12345" }         │                                   │ Process in parallel       │ GET /api/job/{id}



**© 2025 Muhammad Faizan - Cloud Computing Assignment**$job = Invoke-RestMethod -Method Post -Uri "https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/StartJob" -Headers $headers


Invoke-RestMethod -Uri "https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/GetJobStatus?jobId=$($job.JobId)" -Headers $headers         │                                   ▼┌──────┴──────┐

````

         │                         ┌─────────────────────┐│GetJobStatus │

## 🛠️ Local Development

         │                         │ProcessImageFunction ││  Function   │

### Prerequisites

-   .NET 8 SDK │ │- Generate image │└─────────────┘

-   Azure Functions Core Tools v4

-   Azurite (local storage emulator) │ │- Upload to blob │```

### Quick Start │ │- Update progress │

````bash

# Clone repository         │                         └─────────┬───────────┘## 🚀 Quick Start

git clone https://github.com/Faizan-2003/azure-weather-image-app.git

cd azure-weather-image-app         │                                   │



# Start Azurite (separate terminal)         │                                   ▼### Prerequisites

azurite --silent --location ./azurite

         │                         ┌─────────────────────┐

# Build and run

dotnet restore         │                         │  Blob Storage       │-   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

dotnet build

func start         │                         │  (weather-images)   │-   [Azure Functions Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local)



# Open browser         │                         └─────────────────────┘-   [Azurite](https://docs.microsoft.com/azure/storage/common/storage-use-azurite) (for local development)

http://localhost:7071/api/ServeWebsite

```         │                                   │-   [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) (for deployment)



## 🚢 Deployment         │                                   │ Update status-   (Optional) [Unsplash API Key](https://unsplash.com/developers) for real background images



See [DEPLOYMENT.md](DEPLOYMENT.md) for detailed deployment instructions.         │                                   ▼



### Quick Deploy:         │                         ┌─────────────────────┐### Installation (Windows PowerShell)

```powershell

.\deploy.ps1 -ResourceGroupName "rg-weather" -Location "swedencentral" -ApiKey "your-key"         └────────────────────────►│  Table Storage      │

````

           GET /api/GetJobStatus   │  (JobStatus)        │```powershell

## 📦 Azure Resources# Install Azurite

-   **Function App:** weather-image-func-eg2kg4p2kzwtc (Windows, Consumption)#### MUST Requirements (12/12)npm install -g azurite

-   **Storage:** stweathereg2kg4p2kzwtc

-   **Region:** Sweden Central1. ✅ Fetch weather data from Buienradar API```

-   **Resource Group:** StudentGroup

2. ✅ Multiple function endpoints (HTTP-triggered and Queue-triggered)

## 🔧 Technologies

3. ✅ Queue Storage for background processing### Local Development Setup

-   .NET 8 Isolated Worker

-   Azure Functions v44. ✅ Blob Storage for image storage

-   SixLabors.ImageSharp

-   Azure Storage SDK v125. ✅ Table Storage for job status tracking1. **Clone the repository**

-   Bicep IaC

-   GitHub Actions6. ✅ API Key authentication middleware

## 📚 Documentation7. ✅ SAS tokens for secure blob access ```powershell

-   [DEPLOYMENT.md](DEPLOYMENT.md) - Detailed deployment guide8. ✅ Fan-out/fan-in pattern implementation cd azure-weather-image-app

-   [Architecture Diagram](#-architecture) - See above

-   [API Reference](#-api-endpoints) - See above9. ✅ Bicep IaC template for infrastructure ```

## 🐛 Troubleshooting10. ✅ GitHub Actions CI/CD pipeline

**"Unauthorized" error?** 11. ✅ Deploy script (deploy.ps1)2. **Start Azurite** (in a separate terminal)

→ Include header: `X-API-Key: test-api-key-12345`

12. ✅ Comprehensive README documentation

**Images not loading?**

→ SAS tokens expire after 1 hour. Refresh job status. ```powershell

**Local development issues?** #### COULD Requirements (4/4) azurite --silent --location ./azurite

→ Ensure Azurite is running: `azurite --silent`

13. ✅ Additional HTTP endpoints (HealthCheck, TestImageProcessing) ```

## 📞 Contact

14. ✅ Interactive web UI for testing

**Student:** Muhammad Faizan (#701765)

**GitHub Issues:** [Report Issue](https://github.com/Faizan-2003/azure-weather-image-app/issues)15. ✅ Application Insights monitoring Keep this terminal running. Azurite provides local Azure Storage emulation.

---16. ✅ Error handling and retry logic

**© 2025 Muhammad Faizan - Cloud Computing Assignment**3. **Start the application**

## 🚀 Live Deployment

    Simply run the startup script:

### 🌐 Web Interface (Recommended for Testing)

    ```powershell

**URL:** [https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/ServeWebsite](https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/ServeWebsite) .\start.ps1

    ```

**Features:**

-   ✅ No API key configuration needed (pre-filled) Or use the batch file:

-   ✅ One-click job start

-   ✅ Real-time progress tracking ```cmd

-   ✅ Image gallery with generated weather images start.cmd

-   ✅ Health check testing ```

-   ✅ Beautiful, responsive UI

    The script will:

### 📡 API Endpoints

    - Check if Azurite is running (and start it if needed)

**Base URL:** `https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net` - Build the project with the correct configuration

**API Key:** `test-api-key-12345` (Include as `X-API-Key` header) - Start the Azure Functions host

| Endpoint | Method | Auth Required | Description | You should see output like:

|----------|--------|---------------|-------------|

| `/api/HealthCheck` | GET | ❌ No | Health check endpoint | ```

| `/api/ServeWebsite` | GET | ❌ No | Web interface | Functions:

| `/api/StartJob` | POST | ✅ Yes | Start weather job | GetJobStatus: [GET] http://localhost:7071/api/job/{jobId}

| `/api/GetJobStatus?jobId={id}` | GET | ✅ Yes | Check job status | HealthCheck: [GET] http://localhost:7071/api/health

| `/api/test/image` | GET | ❌ No | Test single image generation | ProcessImage: [QueueTrigger]

        StartJob: [POST] http://localhost:7071/api/job/start

### 🧪 Testing Examples TestImageProcessing: [GET] http://localhost:7071/api/test/image

    ```

#### Using cURL:

4. **Test the API**

````bash

# 1. Health Check (No API key needed)    In a new terminal, run the test script:

curl https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/HealthCheck

    ```powershell

# 2. Start Job (Returns job ID)    .\test-local.ps1

curl -X POST \    ```

  -H "X-API-Key: test-api-key-12345" \

  -H "Content-Type: application/json" \    Or see the complete testing guide in **[TESTING.md](TESTING.md)** for all commands and test scenarios.

  https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/StartJob

### Testing Endpoints

# 3. Check Job Status (Replace {jobId} with actual ID)

curl -H "X-API-Key: test-api-key-12345" \The application exposes three main endpoints:

  "https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/GetJobStatus?jobId={jobId}"

#### 1. Start a New Job

# 4. Test Image (Returns JPEG image)

curl https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/test/image \```http

  --output test-image.jpgPOST http://localhost:7071/api/job/start

```X-API-Key: test-api-key-12345

````

#### Using PowerShell:

Response:

````powershell

# 1. Health Check```json

Invoke-RestMethod -Uri "https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/HealthCheck"{

    "jobId": "550e8400-e29b-41d4-a716-446655440000",

# 2. Start Job    "status": "InProgress",

$headers = @{ "X-API-Key" = "test-api-key-12345" }    "message": "Job started with 50 stations"

$response = Invoke-RestMethod -Method Post -Uri "https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/StartJob" -Headers $headers}

$jobId = $response.JobId```

Write-Host "Job ID: $jobId"

#### 2. Get Job Status

# 3. Check Job Status

Invoke-RestMethod -Uri "https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/GetJobStatus?jobId=$jobId" -Headers $headers```http

GET http://localhost:7071/api/job/{jobId}

# 4. Test ImageX-API-Key: test-api-key-12345

Invoke-WebRequest -Uri "https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/test/image" -OutFile "test-image.jpg"```

````

Response:

## 📦 Azure Resources Deployed

```json

| Resource Type | Name | Purpose |{

|---------------|------|---------|    "jobId": "550e8400-e29b-41d4-a716-446655440000",

| Function App | `weather-image-func-eg2kg4p2kzwtc` | Hosts the application (Windows, .NET 8) |    "status": "InProgress",

| App Service Plan | Consumption (Y1) | Serverless hosting |    "totalStations": 50,

| Storage Account | `stweathereg2kg4p2kzwtc` | Stores blobs, queues, and tables |    "processedStations": 25,

| Blob Container | `weather-images` | Stores generated images (private) |    "images": [

| Queue | `job-start-queue` | Job initiation queue |        {

| Queue | `image-processing-queue` | Image processing queue (10 messages) |            "stationName": "Amsterdam",

| Table | `JobStatus` | Job tracking and status |            "imageUrl": "https://...blob.core.windows.net/weather-images/...",

| Application Insights | `weather-image-func-eg2kg4p2kzwtc-insights` | Monitoring and logs |            "createdAt": "2025-11-04T12:00:00Z"

        }

**Region:** Sweden Central      ]

**Resource Group:** StudentGroup}

```

## 🛠️ Local Development

#### 3. Test Image Generation

### Prerequisites

````http

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)GET http://localhost:7071/api/test/image

- [Azure Functions Core Tools v4](https://docs.microsoft.com/azure/azure-functions/functions-run-local)X-API-Key: test-api-key-12345

- [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)```

- [Azurite](https://docs.microsoft.com/azure/storage/common/storage-use-azurite) (for local storage emulation)

Returns a JPEG image directly in the response.

### Setup Steps

## 📊 Azure Resources Created

1. **Clone the repository**

   ```bashThe Bicep template creates the following resources:

   git clone https://github.com/Faizan-2003/azure-weather-image-app.git

   cd azure-weather-image-app| Resource Type        | Name                          | Purpose                           |

   ```| -------------------- | ----------------------------- | --------------------------------- |

| Storage Account      | `stweather{unique}`           | Stores blobs, queues, and tables  |

2. **Install dependencies**| Blob Container       | `weather-images`              | Stores generated images (private) |

   ```bash| Queue                | `image-processing-queue`      | Message queue for processing      |

   dotnet restore| Table                | `JobStatus`                   | Stores job state and progress     |

   ```| App Service Plan     | `{function-app}-plan`         | Consumption plan (Y1)             |

| Function App         | `weather-image-func-{unique}` | Hosts the application             |

3. **Start Azurite** (in a separate terminal)| Application Insights | `{function-app}-insights`     | Monitoring and logging            |

   ```bash

   azurite --silent --location ./azurite## 📝 API Endpoints Reference

````

### POST /api/job/start

4. **Build and run**

    ```bashStarts a new image generation job.

    dotnet build

    func start**Response:** 202 Accepted

    ```

```json

5. **Test locally**{

   - Open browser: `http://localhost:7071/api/ServeWebsite`    "jobId": "uuid",

   - Or use the API endpoints with `http://localhost:7071`    "status": "InProgress",

    "message": "Job started with N stations"

### Configuration}

```

The `local.settings.json` file contains local development settings:

### GET /api/job/{jobId}

````json

{Retrieves job status and results.

  "IsEncrypted": false,

  "Values": {**Response:** 200 OK

    "AzureWebJobsStorage": "UseDevelopmentStorage=true",

    "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",```json

    "ApiKey": "test-api-key-12345",{

    "UnsplashAccessKey": ""  "jobId": "uuid",

  }  "status": "InProgress|Completed",

}  "totalStations": 50,

```  "processedStations": 25,

  "images": [...]

## 🚢 Deployment}

````

### Option 1: Automated Deployment (GitHub Actions)

### GET /api/test/image

The repository includes a GitHub Actions workflow (`.github/workflows/azure-deploy.yml`) that automatically deploys on push to main branch.

Generates a test image for debugging.

**Setup:**

1. Ensure you have Azure credentials configured as GitHub Secrets**Response:** 200 OK (image/jpeg)

2. Push to main branch

3. GitHub Actions will automatically build and deploy## 🐛 Troubleshooting

### Option 2: Manual Deployment (deploy.ps1)### Azurite Connection Issues

Use the PowerShell deployment script:- Ensure Azurite is running: `azurite --silent`

-   Check that `local.settings.json` has `UseDevelopmentStorage=true`

````powershell

.\deploy.ps1 `### Function Not Responding

    -ResourceGroupName "YourResourceGroup" `

    -Location "swedencentral" `-   Check the function host is running: `func start`

    -ApiKey "your-api-key-here"-   Verify the correct port (default: 7071)

```-   Check firewall settings


##  Troubleshooting

### Common Issues

**Issue:** "Unauthorized: Invalid or missing API key"
**Solution:** Ensure `X-API-Key: test-api-key-12345` header is included

**Issue:** Job status shows "InProgress" forever
**Solution:** Check Application Insights logs for queue processing errors

**Issue:** Images not displaying
**Solution:** SAS tokens expire after 1 hour. Refresh job status to get new URLs

**Issue:** Local development storage errors
**Solution:** Ensure Azurite is running: `azurite --silent --location ./azurite`

## 📚 Assignment Requirements Checklist

### Core Functionality

-   [x] Fetch data from external API (Buienradar)
-   [x] Multiple Azure Functions (7 functions total)
-   [x] HTTP-triggered functions (5 endpoints)
-   [x] Queue-triggered functions (2 queue processors)
-   [x] Queue Storage integration (2 queues)
-   [x] Blob Storage integration (image storage)
-   [x] Table Storage integration (job tracking)
-   [x] API Key authentication
-   [x] SAS token generation
-   [x] Fan-out/fan-in pattern

### Infrastructure & Deployment

-   [x] Bicep template for IaC
-   [x] deploy.ps1 script with:
    -   [x] dotnet CLI build/publish
    -   [x] Bicep deployment
    -   [x] Azure CLI function deployment
-   [x] GitHub Actions CI/CD
-   [x] Comprehensive documentation

### Extra Features

-   [x] Web interface for testing
-   [x] Health check endpoint
-   [x] Test image endpoint
-   [x] Application Insights
-   [x] Error handling
-   [x] Retry mechanisms
-   [x] Progress tracking

## 📝 API Response Examples

### StartJob Response

```json
{
    "jobId": "a3b8c9d0-1234-5678-90ab-cdef12345678",
    "message": "Job started successfully"
}
```

### GetJobStatus Response (In Progress)

```json
{
    "jobId": "a3b8c9d0-1234-5678-90ab-cdef12345678",
    "status": "InProgress",
    "totalStations": 10,
    "processedStations": 6,
    "images": [
        {
            "stationName": "De Bilt",
            "imageUrl": "https://stweathereg2kg4p2kzwtc.blob.core.windows.net/weather-images/...",
            "createdAt": "2025-11-06T10:30:00Z"
        }
    ]
}
```

### GetJobStatus Response (Completed)

```json
{
  "jobId": "a3b8c9d0-1234-5678-90ab-cdef12345678",
  "status": "Completed",
  "totalStations": 10,
  "processedStations": 10,
  "images": [
    {
      "stationName": "De Bilt",
      "imageUrl": "https://stweathereg2kg4p2kzwtc.blob.core.windows.net/weather-images/a3b8c9d0_de-bilt_20251106.jpg?sv=...",
      "createdAt": "2025-11-06T10:30:00Z"
    },
    ...
  ]
}
```

## 📞 Support

For questions or issues:

-   **Student:** Muhammad Faizan
-   **Student Number:** 701765
-   **GitHub Issues:** [https://github.com/Faizan-2003/azure-weather-image-app/issues](https://github.com/Faizan-2003/azure-weather-image-app/issues)

---

**© 2025 Muhammad Faizan - Azure / GitHub Assignment**
````
