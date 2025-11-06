# Weather Image Application - Azure Functions

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
    git clone https://github.com/YOUR_USERNAME/azure-weather-image-app.git
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

## 🌐 Deployment to Azure

### Option 1: GitHub Actions (Automated) ⭐ Recommended

The project includes GitHub Actions workflows for automated deployment!

**Setup Steps:**

1. **Create Azure Service Principal**

    ```bash
    az ad sp create-for-rbac \
      --name "github-actions-weather-app" \
      --role contributor \
      --scopes /subscriptions/{subscription-id} \
      --sdk-auth
    ```

2. **Add GitHub Secrets**

    - Go to your repo → Settings → Secrets and variables → Actions
    - Add `AZURE_CREDENTIALS` (JSON output from step 1)
    - Add `AZURE_SUBSCRIPTION_ID`

3. **Deploy Infrastructure**

    - Go to GitHub Actions tab
    - Run "Deploy Azure Infrastructure" workflow
    - Provide API key and other parameters

4. **Configure App Deployment**

    - Update `AZURE_FUNCTIONAPP_NAME` in `.github/workflows/azure-deploy.yml`
    - Add `AZURE_FUNCTIONAPP_PUBLISH_PROFILE` secret (download from Azure Portal)

5. **Push to Main** - Automatic deployment on every push! 🚀

📖 **Detailed Setup:** [GITHUB_ACTIONS_SETUP.md](GITHUB_ACTIONS_SETUP.md)  
📖 **Quick Reference:** [.github/workflows/README.md](.github/workflows/README.md)

### Option 2: PowerShell Script (Quick Deploy)

```powershell
.\deploy.ps1 `
  -ResourceGroupName "rg-weather-image-app" `
  -Location "westeurope" `
  -ApiKey "your-secure-api-key-here" `
  -UnsplashAccessKey "your-unsplash-key"
```

The script will:

1. Create the resource group
2. Deploy all Azure resources using Bicep
3. Build and publish the .NET project
4. Package the application
5. Deploy to Azure Functions

### Option 3: Manual Deployment

1. **Login to Azure**

    ```powershell
    az login
    ```

2. **Create resource group**

    ```powershell
    az group create --name rg-weather-image --location westeurope
    ```

3. **Deploy Bicep template**

    ```powershell
    az deployment group create `
      --name weather-deployment `
      --resource-group rg-weather-image `
      --template-file deploy/main.bicep `
      --parameters apiKey="your-api-key" unsplashAccessKey="your-unsplash-key"
    ```

4. **Build and publish**

    ```powershell
    dotnet publish --configuration Release --output publish
    ```

5. **Create deployment package**

    ```powershell
    Compress-Archive -Path "publish\*" -DestinationPath deploy.zip -Force
    ```

6. **Deploy to Azure Functions**
    ```powershell
    az functionapp deployment source config-zip `
      --resource-group rg-weather-image `
      --name your-function-app-name `
      --src deploy.zip
    ```

## 📦 Project Structure

```
ssp-assignment/
├── Functions/
│   ├── StartJobFunction.cs          # HTTP trigger to start jobs
│   ├── GetJobStatusFunction.cs      # HTTP trigger to get status
│   ├── ProcessImageFunction.cs      # Queue trigger for processing
│   └── TestImageProcessingFunction.cs # HTTP trigger for testing
├── Services/
│   ├── IWeatherService.cs           # Weather data interface
│   ├── WeatherService.cs            # Buienradar integration
│   ├── IImageService.cs             # Image generation interface
│   ├── ImageService.cs              # ImageSharp implementation
│   ├── IBlobStorageService.cs       # Blob storage interface
│   ├── BlobStorageService.cs        # Azure Blob integration
│   ├── IQueueService.cs             # Queue interface
│   ├── QueueService.cs              # Azure Queue integration
│   ├── ITableStorageService.cs      # Table storage interface
│   └── TableStorageService.cs       # Azure Table integration
├── Models/
│   ├── WeatherStation.cs            # Weather data model
│   ├── ImageInfo.cs                 # Image metadata model
│   ├── ImageProcessingMessage.cs    # Queue message model
│   ├── JobStatusEntity.cs           # Table entity model
│   ├── StartJobResponse.cs          # API response model
│   └── JobStatusResponse.cs         # API response model
├── Middleware/
│   └── ApiKeyAuthMiddleware.cs      # API key authentication
├── deploy/
│   └── main.bicep                   # Infrastructure as Code
├── Program.cs                       # Application entry point
├── host.json                        # Function host configuration
├── local.settings.json              # Local development settings
├── ssp.csproj                       # Project file
├── deploy.ps1                       # Deployment script
├── api-requests.http                # API documentation
├── test-local.sh                    # Local test script
├── test-features.sh                 # Feature test script
└── README.md                        # This file
```

## 🔑 API Authentication

All API endpoints require an `X-API-Key` header:

```http
X-API-Key: your-api-key-here
```

### Local Development

Use: `test-api-key-12345`

### Production

Set a secure API key during deployment.

## 🧪 Testing

### Automated Tests

Run all feature tests:

```bash
bash test-features.sh
```

Run basic local tests:

```bash
bash test-local.sh
```

### Manual Testing with VS Code REST Client

1. Install the "REST Client" extension in VS Code
2. Open `api-requests.http`
3. Click "Send Request" above any HTTP request

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

## 🔧 Configuration

### Environment Variables

| Variable                   | Description                 | Default (Local)              |
| -------------------------- | --------------------------- | ---------------------------- |
| `AzureWebJobsStorage`      | Storage connection string   | `UseDevelopmentStorage=true` |
| `ApiKey`                   | API authentication key      | `test-api-key-12345`         |
| `UnsplashAccessKey`        | Unsplash API key (optional) | Empty (uses gradient)        |
| `FUNCTIONS_WORKER_RUNTIME` | Runtime identifier          | `dotnet-isolated`            |

## 📝 API Endpoints Reference

### POST /api/job/start

Starts a new image generation job.

**Headers:**

-   `X-API-Key`: Your API key

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

**Headers:**

-   `X-API-Key`: Your API key

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

**Headers:**

-   `X-API-Key`: Your API key

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

## ✅ Assignment Requirements

### Must-Have Requirements ✓

| Requirement                               | Status | Implementation                                   |
| ----------------------------------------- | ------ | ------------------------------------------------ |
| HTTP endpoint to start image creation     | ✅     | `StartJobFunction.cs` - POST /api/job/start      |
| Return unique job ID                      | ✅     | Returns GUID for tracking                        |
| Fetch status of running process           | ✅     | `GetJobStatusFunction.cs` - GET /api/job/{jobId} |
| Fetch results of completed process        | ✅     | Returns list of image URLs with SAS tokens       |
| Serve images from blob storage            | ✅     | Images stored in `weather-images` container      |
| Queue-based processing (QueueTrigger)     | ✅     | `ProcessImageFunction.cs` with queue trigger     |
| Fast initial call (background processing) | ✅     | Job starts immediately, processing in background |
| Buienradar API integration                | ✅     | `WeatherService.cs` fetches 50 stations          |
| Public API for image retrieval            | ✅     | Uses Unsplash (fallback: gradient)               |
| Write weather data on images              | ✅     | `ImageService.cs` using ImageSharp               |
| HTTP files as API documentation           | ✅     | `api-requests.http`                              |
| Bicep template                            | ✅     | `deploy/main.bicep` with all resources           |
| Include queues in Bicep                   | ✅     | `image-processing-queue` defined                 |
| Deploy script (deploy.ps1)                | ✅     | Complete PowerShell deployment script            |
| Working deployed endpoint                 | ✅     | Ready to deploy with script                      |

### Could-Have Requirements ✓

| Requirement                      | Status | Implementation                                |
| -------------------------------- | ------ | --------------------------------------------- |
| SAS token instead of public blob | ✅     | `BlobStorageService.cs` generates SAS URLs    |
| Authentication on API            | ✅     | `ApiKeyAuthMiddleware.cs` - X-API-Key header  |
| Status endpoint                  | ✅     | GET /api/job/{jobId} shows progress           |
| Save status in Table             | ✅     | `TableStorageService.cs` with JobStatus table |

## 📂 GitHub Setup

### Initialize and Push to GitHub

```powershell
# Initialize git (if not already done)
git init

# Add all files
git add .

# Commit
git commit -m "Initial commit: Weather image Azure Functions app"

# Create repository on GitHub, then:
git remote add origin https://github.com/YOUR_USERNAME/azure-weather-image-app.git
git branch -M main
git push -u origin main
```

### Add Collaborators

1. Go to your GitHub repository
2. Click "Settings" → "Collaborators"
3. Click "Add people"
4. Enter your collaborator's GitHub username
5. They will receive an invitation via email

## 📄 License

This project is created for educational purposes as part of an assignment.

## 👥 Contributors

-   [Faizan-2003](https://github.com/Faizan-2003)

---

**Happy Coding! 🚀**
