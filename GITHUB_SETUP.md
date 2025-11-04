# GitHub Setup Guide

## 📦 Preparing Your Repository

### Step 1: Initialize Git Repository (if not already done)

```powershell
cd "c:\Users\mf384\OneDrive\Desktop\ssp-assignment"
git init
```

### Step 2: Add All Files

```powershell
git add .
```

### Step 3: Commit Your Work

```powershell
git commit -m "Initial commit: Complete weather image Azure Functions app

- Implemented HTTP triggers for job management
- Implemented Queue trigger for background processing
- Integrated Buienradar API for weather data
- Image generation with ImageSharp
- Blob/Queue/Table Storage integration
- API key authentication
- Bicep infrastructure template
- PowerShell deployment script
- Complete documentation"
```

### Step 4: Create GitHub Repository

1. Go to [GitHub](https://github.com)
2. Click "New Repository" (+ icon in top right)
3. Name: `ssp-assignment` or `weather-image-app`
4. Description: "Azure Functions weather image generator for SSP assignment"
5. Choose **Private** (you'll add collaborators)
6. **Don't** initialize with README (we already have one)
7. Click "Create repository"

### Step 5: Link and Push to GitHub

GitHub will show you commands. Use these:

```powershell
# Add the remote
git remote add origin https://github.com/YOUR_USERNAME/ssp-assignment.git

# Push to GitHub
git branch -M main
git push -u origin main
```

Replace `YOUR_USERNAME` with your GitHub username.

### Step 6: Add Collaborators

#### Method 1: Add GitHub User

1. Go to your repository on GitHub
2. Click "Settings" tab
3. Click "Collaborators" in the left sidebar
4. Click "Add people"
5. Search for: `triplegh2025`
6. Click "Add triplegh2025 to this repository"
7. Select role: **Write** or **Admin**

#### Method 2: Add by Email

1. Same steps as above
2. Use email: `triplegithub2025@outlook.com`
3. They'll receive an invitation email

### Step 7: Verify Your Repository

Your repository should contain:

```
✅ Source code (Functions/, Services/, Models/, Middleware/)
✅ Configuration (*.csproj, host.json, local.settings.json)
✅ Infrastructure (deploy/, main.bicep)
✅ Deployment script (deploy.ps1)
✅ Documentation (README.md, QUICKSTART.md, etc.)
✅ API docs (api-requests.http)
✅ Test scripts (test-*.sh)
✅ .gitignore (excludes bin/, obj/, etc.)
```

### Important: Do NOT Commit

The `.gitignore` file already excludes these:

-   ❌ `bin/` and `obj/` folders
-   ❌ `local.settings.json` (contains local secrets)
-   ❌ `*.zip` deployment packages
-   ❌ Azurite data files

But `local.settings.json` is tracked for the template. **Before pushing, remove any real API keys if you added them!**

## 📝 Update local.settings.json Before Pushing

Make sure it only has placeholder values:

```json
{
    "IsEncrypted": false,
    "Values": {
        "AzureWebJobsStorage": "UseDevelopmentStorage=true",
        "FUNCTIONS_WORKER_RUNTIME": "dotnet-isolated",
        "ApiKey": "test-api-key-12345",
        "UnsplashAccessKey": "YOUR_UNSPLASH_ACCESS_KEY_HERE"
    }
}
```

## 🎯 Final Checklist

Before submitting:

-   [ ] All code committed to GitHub
-   [ ] Repository is accessible
-   [ ] `triplegh2025` or `triplegithub2025@outlook.com` added as collaborator
-   [ ] README.md is complete and accurate
-   [ ] No secrets in committed files
-   [ ] `.gitignore` is working properly
-   [ ] Code builds successfully (`dotnet build`)
-   [ ] Application has been tested locally
-   [ ] (Optional) Deployed to Azure and working

## 📧 Notification

After adding the collaborator, send an email with:

```
Subject: SSP Assignment Submission - Weather Image App

Hi,

I've completed the SSP assignment and added you as a collaborator to my GitHub repository.

Repository URL: https://github.com/YOUR_USERNAME/ssp-assignment

The application includes:
- HTTP API for weather image generation
- Queue-based background processing
- Integration with Buienradar API
- Blob Storage for images with SAS tokens
- Complete Bicep infrastructure template
- Automated deployment script
- Full documentation

The code is ready to run locally and deploy to Azure.

Best regards,
[Your Name]
```

## 🚀 Optional: Deploy to Azure

To show it working in production:

```powershell
.\deploy.ps1 `
  -ResourceGroupName "rg-ssp-assignment" `
  -Location "westeurope" `
  -ApiKey "production-key-here"
```

Then include the deployed URL in your submission email.

## 🎉 You're Done!

Your assignment is complete and ready for review!

**Good luck! 🚀**
