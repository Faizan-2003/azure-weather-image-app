@echo off
REM Start the Azure Functions application

echo === Starting Azure Functions Application ===
echo.

REM Build the project
echo Building project...
dotnet build ssp.csproj --verbosity quiet
if errorlevel 1 (
    echo Build failed
    exit /b 1
)
echo Build successful
echo.

REM Start Functions host
echo Starting Azure Functions host...
echo Press Ctrl+C to stop
echo.

cd bin\Debug\net8.0
func start
