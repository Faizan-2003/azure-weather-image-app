# Quick Setup Guide

## 🚀 Getting Started in 5 Minutes

### Step 1: Install Prerequisites (if not already installed)

```powershell
# Install .NET 8 SDK
winget install Microsoft.DotNet.SDK.8

# Install Azure Functions Core Tools
npm install -g azure-functions-core-tools@4

# Install Azurite for local storage emulation
npm install -g azurite
```

### Step 2: Start Azurite

Open a PowerShell terminal and run:

```powershell
azurite --silent
```

Keep this terminal running. Azurite provides local Azure Storage emulation.

### Step 3: Run the Application

Open another PowerShell terminal in the project folder:

```powershell
cd "c:\Users\mf384\OneDrive\Desktop\ssp-assignment"
func start
```

Or simply:

```powershell
dotnet run
```

You should see output like:

```
Functions:
    GetJobStatus: [GET] http://localhost:7071/api/job/{jobId}
    ProcessImage: [QueueTrigger]
    StartJob: [POST] http://localhost:7071/api/job/start
    TestImageProcessing: [GET] http://localhost:7071/api/test/image
```

### Step 4: Test the API

#### Using curl (in Git Bash or WSL):

```bash
# Test the test endpoint
curl -X GET "http://localhost:7071/api/test/image" \
  -H "X-API-Key: test-api-key-12345" \
  --output test-image.jpg

# Start a new job
curl -X POST "http://localhost:7071/api/job/start" \
  -H "X-API-Key: test-api-key-12345" \
  -H "Content-Type: application/json"

# Get job status (replace {jobId} with actual job ID from previous response)
curl -X GET "http://localhost:7071/api/job/{jobId}" \
  -H "X-API-Key: test-api-key-12345"
```

#### Using PowerShell:

```powershell
# Test the test endpoint
Invoke-WebRequest -Uri "http://localhost:7071/api/test/image" `
  -Headers @{"X-API-Key"="test-api-key-12345"} `
  -OutFile "test-image.jpg"

# Start a new job
$response = Invoke-RestMethod -Uri "http://localhost:7071/api/job/start" `
  -Method POST `
  -Headers @{"X-API-Key"="test-api-key-12345"; "Content-Type"="application/json"}

$response

# Get job status
$jobId = $response.jobId
Invoke-RestMethod -Uri "http://localhost:7071/api/job/$jobId" `
  -Headers @{"X-API-Key"="test-api-key-12345"}
```

#### Using the HTTP file in VS Code:

1. Install the "REST Client" extension in VS Code
2. Open `api-requests.http`
3. Click "Send Request" above any HTTP request

### Step 5: Run Automated Tests

```bash
# Make sure Git Bash is installed
bash test-local.sh
bash test-features.sh
```

## 📦 Deploy to Azure

### Prerequisites:

-   Azure subscription
-   Azure CLI installed and logged in

### Deploy using the script:

```powershell
.\deploy.ps1 `
  -ResourceGroupName "rg-weather-image-app" `
  -Location "westeurope" `
  -ApiKey "your-secure-production-api-key" `
  -UnsplashAccessKey "your-unsplash-access-key-if-you-have-one"
```

The script will:

1. ✅ Create resource group
2. ✅ Deploy Azure resources (Storage, Function App, etc.)
3. ✅ Build the project
4. ✅ Deploy to Azure Functions
5. ✅ Display your endpoints

### After Deployment:

Your endpoints will be available at:

```
https://your-function-app-name.azurewebsites.net/api/job/start
https://your-function-app-name.azurewebsites.net/api/job/{jobId}
https://your-function-app-name.azurewebsites.net/api/test/image
```

Remember to use your production API key in the `X-API-Key` header!

## 🔍 Troubleshooting

### Azurite not starting?

-   Kill any existing process: `taskkill /F /IM azurite.exe`
-   Clear data: Delete the `__azurite*` folders
-   Start again: `azurite --silent`

### Function app not starting?

-   Make sure Azurite is running
-   Check `local.settings.json` exists
-   Try: `dotnet clean` then `dotnet build`

### Queue messages not processing?

-   Check Azurite is running
-   Look at the function logs for errors
-   The queue name must be: `image-processing-queue`

### Need help?

Check the full `README.md` for detailed documentation!

## 🎯 Next Steps

1. ✅ Test locally with Azurite
2. ✅ Run the automated tests
3. ✅ Deploy to Azure
4. ✅ Test the production endpoints
5. ✅ Add `triplegh2025` or `triplegithub2025@outlook.com` to your GitHub repo
6. ✅ Submit your assignment!

---

**Happy Coding! 🚀**
