## Azure Weather Image Application

Student: Muhammad Faizan

Student Number: 701765

Repository: "https://github.com/Faizan-2003/azure-weather-image-app"

Live App: "[weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/ServeWebsite](https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/ServeWebsite)"

---

## Overview

A serverless Azure Functions application that generates weather-themed images for 50 Dutch weather stations using real-time data from the Buienradar API.

It integrates multiple Azure services such as Blob Storage, Queue Storage, and Table Storage while utilizing the fan-out/fan-in processing pattern.

The app includes a web-based interface for testing and real-time progress tracking.

---

## Features

-   Real-time weather data from Buienradar API
-   Weather image generation using ImageSharp
-   Serverless & scalable (Azure Functions v4, .NET 8)
-   Background job orchestration with Azure Queues
-   Job tracking with Table Storage
-   API Key authentication
-   SAS tokens for secure blob access
-   Automated deployment via GitHub Actions
-   Infrastructure as Code (Bicep)
-   Interactive web UI for testing

---

## Live Demo

Web Interface:
https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/ServeWebsite

API Key: test-api-key-12345 (use as X-API-Key header)

Quick Test:
Click “Start Weather Job” - all configuration is preloaded.

**API Endpoints**

-   /api/HealthCheck - GET
-   /api/ServeWebsite - GET
-   /api/StartJob - POST
-   /api/GetJobStatus?jobId={id} - GET
-   /api/test/image - GET

---

## Quick Start

Using cURL

# Start job

curl -X POST -H "X-API-Key: test-api-key-12345" \
https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/StartJob

# Check job status

curl -H "X-API-Key: test-api-key-12345" \
"https://weather-image-func-eg2kg4p2kzwtc.azurewebsites.net/api/GetJobStatus?jobId={jobId}"

---

## Local Development

-   Prerequisites
-   .NET 8 SDK
-   Azure Functions Core Tools v4
-   Azurite
-   Azure CLI

Steps:

git clone https://github.com/Faizan-2003/azure-weather-image-app.git
cd azure-weather-image-app

azurite --silent --location ./azurite # Start local storage
dotnet restore
dotnet build
func start

Then open http://localhost:7071/api/ServeWebsite
.

---

## Deployment

**Option 1: GitHub Actions (CI/CD)**

Push to main → GitHub Actions automatically deploys via .github/workflows/azure-deploy.yml.

**Option 2: Manual (PowerShell)**

.\deploy.ps1 -ResourceGroupName "rg-weather" -Location "swedencentral" -ApiKey "your-api-key"

---

## Azure Resources

-   Type Name Purpose
-   Function App weather-image-func-eg2kg4p2kzwtc Hosts Azure Functions
-   Storage Account stweathereg2kg4p2kzwtc Stores blobs, queues, and tables
-   Blob Container weather-images Image storage
-   Queue job-start-queue / image-processing-queue Job orchestration
-   Table JobStatus Job tracking
-   Application Insights weather-image-func-insights Monitoring & logging

---

## Tech Stack

-   Language: C# (.NET 8 Isolated Worker)
-   Framework: Azure Functions v4
-   Image Processing: SixLabors.ImageSharp
-   Infrastructure: Azure Bicep
-   Automation: GitHub Actions
-   Storage: Azure Storage SDK v12

---

© 2025 Muhammad Faizan – Azure / GitHub Assignment
