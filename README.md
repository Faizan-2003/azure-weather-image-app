# Weather Image Application - Azure Functions

**Student Name:** Muhammad Faizan
**Student Number:** 701765
**GitHub Repository:** https://github.com/Faizan-2003/azure-weather-image-app

---

A serverless application built with Azure Functions that generates weather-themed images for Dutch weather stations. The application fetches real-time weather data from Buienradar API and overlays it on beautiful background images.

## 🌟 Features

-   **HTTP API** for starting jobs and checking status
-   **Queue-based processing** for scalable background image generation
-   **Blob Storage** for secure image storage with SAS token access
-   **Table Storage** for persistent job tracking
-   **API Key authentication** for all endpoints
-   **Automatic fan-out pattern** - processes 50 weather stations in parallel
-   **Real-time weather data** from Buienradar Netherlands
-   **Beautiful image composition** using ImageSharp library
-   **GitHub Actions CI/CD** for automated deployment 🚀

## 📋 Architecture

```
┌─────────────┐
│   Client    │
└──────┬──────┘
       │
       │ POST /api/job/start
       ▼
┌─────────────────┐         ┌──────────────┐
│ StartJobFunction├────────►│ Queue Storage│
└─────────────────┘         └──────┬───────┘
       │                           │
       │                           │ Messages (50)
       ▼                           ▼
┌─────────────────┐         ┌──────────────────┐
│ Table Storage   │◄────────┤ProcessImageFunction│
│   (Job State)   │         └──────────┬─────────┘
└─────────────────┘                    │
       ▲                               ▼
       │                        ┌──────────────┐
       │                        │ Blob Storage │
       │                        │   (Images)   │
       │                        └──────────────┘
       │
       │ GET /api/job/{id}
┌──────┴──────┐
│GetJobStatus │
│  Function   │
└─────────────┘
```

## 🚀 Quick Start

### Prerequisites

-   [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
-   [Azure Functions Core Tools](https://docs.microsoft.com/azure/azure-functions/functions-run-local)
-   [Azurite](https://docs.microsoft.com/azure/storage/common/storage-use-azurite) (for local development)
-   [Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli) (for deployment)
-   (Optional) [Unsplash API Key](https://unsplash.com/developers) for real background images

### Installation (Windows PowerShell)

```powershell
# Install .NET 8 SDK
winget install Microsoft.DotNet.SDK.8

# Install Azure Functions Core Tools
npm install -g azure-functions-core-tools@4

# Install Azurite
npm install -g azurite
```

### Local Development Setup

1. **Clone the repository**

    ```powershell
    cd azure-weather-image-app
    ```

2. **Start Azurite** (in a separate terminal)

    ```powershell
    azurite --silent --location ./azurite
    ```

    Keep this terminal running. Azurite provides local Azure Storage emulation.

3. **Start the application**

    Simply run the startup script:

    ```powershell
    .\start.ps1
    ```

    Or use the batch file:

    ```cmd
    start.cmd
    ```

    The script will:

    - Check if Azurite is running (and start it if needed)
    - Build the project with the correct configuration
    - Start the Azure Functions host

    You should see output like:

    ```
    Functions:
        GetJobStatus: [GET] http://localhost:7071/api/job/{jobId}
        HealthCheck: [GET] http://localhost:7071/api/health
        ProcessImage: [QueueTrigger]
        StartJob: [POST] http://localhost:7071/api/job/start
        TestImageProcessing: [GET] http://localhost:7071/api/test/image
    ```

4. **Test the API**

    In a new terminal, run the test script:

    ```powershell
    .\test-local.ps1
    ```

    Or see the complete testing guide in **[TESTING.md](TESTING.md)** for all commands and test scenarios.

### Testing Endpoints

The application exposes three main endpoints:

#### 1. Start a New Job

```http
POST http://localhost:7071/api/job/start
X-API-Key: test-api-key-12345
```

Response:

```json
{
    "jobId": "550e8400-e29b-41d4-a716-446655440000",
    "status": "InProgress",
    "message": "Job started with 50 stations"
}
```

#### 2. Get Job Status

```http
GET http://localhost:7071/api/job/{jobId}
X-API-Key: test-api-key-12345
```

Response:

```json
{
    "jobId": "550e8400-e29b-41d4-a716-446655440000",
    "status": "InProgress",
    "totalStations": 50,
    "processedStations": 25,
    "images": [
        {
            "stationName": "Amsterdam",
            "imageUrl": "https://...blob.core.windows.net/weather-images/...",
            "createdAt": "2025-11-04T12:00:00Z"
        }
    ]
}
```

#### 3. Test Image Generation

```http
GET http://localhost:7071/api/test/image
X-API-Key: test-api-key-12345
```

Returns a JPEG image directly in the response.

## 📊 Azure Resources Created

The Bicep template creates the following resources:

| Resource Type        | Name                          | Purpose                           |
| -------------------- | ----------------------------- | --------------------------------- |
| Storage Account      | `stweather{unique}`           | Stores blobs, queues, and tables  |
| Blob Container       | `weather-images`              | Stores generated images (private) |
| Queue                | `image-processing-queue`      | Message queue for processing      |
| Table                | `JobStatus`                   | Stores job state and progress     |
| App Service Plan     | `{function-app}-plan`         | Consumption plan (Y1)             |
| Function App         | `weather-image-func-{unique}` | Hosts the application             |
| Application Insights | `{function-app}-insights`     | Monitoring and logging            |

## 📝 API Endpoints Reference

### POST /api/job/start

Starts a new image generation job.

**Response:** 202 Accepted

```json
{
    "jobId": "uuid",
    "status": "InProgress",
    "message": "Job started with N stations"
}
```

### GET /api/job/{jobId}

Retrieves job status and results.

**Response:** 200 OK

```json
{
  "jobId": "uuid",
  "status": "InProgress|Completed",
  "totalStations": 50,
  "processedStations": 25,
  "images": [...]
}
```

### GET /api/test/image

Generates a test image for debugging.

**Response:** 200 OK (image/jpeg)

## 🐛 Troubleshooting

### Azurite Connection Issues

-   Ensure Azurite is running: `azurite --silent`
-   Check that `local.settings.json` has `UseDevelopmentStorage=true`

### Function Not Responding

-   Check the function host is running: `func start`
-   Verify the correct port (default: 7071)
-   Check firewall settings

### Queue Messages Not Processing

-   Verify Azurite is running
-   Check function host logs for errors
-   Ensure `image-processing-queue` exists in Azurite

### Images Not Uploading

-   Check blob storage connection
-   Verify `weather-images` container exists
-   Check function logs for detailed errors

## 📚 Additional Resources

-   [Azure Functions Documentation](https://docs.microsoft.com/azure/azure-functions/)
-   [Buienradar API](https://data.buienradar.nl/2.0/feed/json)
-   [Unsplash API](https://unsplash.com/developers)
-   [ImageSharp Documentation](https://docs.sixlabors.com/articles/imagesharp/)
-   [Azure Storage Documentation](https://docs.microsoft.com/azure/storage/)

## 🌐 Live Deployment

**🎨 INTERACTIVE WEB UI (For Your Teacher):**  
👉 **https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api** 👈

**Open this link in Chrome - No API key needed! Test everything with buttons and see real-time results!**

_(Alternative URL: https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/servewebsite)_

---

**Function App Base URL:** https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net  
**Region:** Sweden Central  
**Resource Group:** StudentGroup

### 📋 How to Test This Application

#### Option 1: 🎨 Web Interface (RECOMMENDED for Teachers)

Simply open in your browser:

```
https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/
```

Features:

-   ✅ No API key configuration needed
-   ✅ Visual buttons to test all endpoints
-   ✅ Real-time progress bar
-   ✅ Image gallery showing generated weather images
-   ✅ Beautiful UI showing all functionality

#### Option 2: 🔧 Direct API Calls (For Developers)

All API endpoints require `X-API-Key: test-api-key-12345` header.

**Available Endpoints:**

-   `GET /api/health` - Health check endpoint
-   `POST /api/job/start` - Start weather image generation job
-   `GET /api/job/{jobId}` - Check job status and get results

**Example using curl:**

```bash
# Health Check
curl -H "X-API-Key: test-api-key-12345" https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/health

# Start Job
curl -X POST -H "X-API-Key: test-api-key-12345" -H "Content-Type: application/json" https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/job/start

# Check Status
curl -H "X-API-Key: test-api-key-12345" https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/job/{jobId}
```

### 📦 Azure Resources

-   **Storage Account:** stweathereg2kg4p2kzwtc
-   **Queues:** job-start-queue, image-processing-queue
-   **Blob Container:** weather-images (private with SAS tokens)
-   **Table:** JobStatus
-   **Application Insights:** Enabled for monitoring

## 📄 License

This project is created for educational purposes as part of a cloud computing assignment.

## 👥 Author

**Student:** Muhammad Faizan
**Student Number:** 701765
**GitHub:** [Faizan-2003](https://github.com/Faizan-2003)  
**Repository:** https://github.com/Faizan-2003/azure-weather-image-app

---

<!-- Deployment trigger -->

