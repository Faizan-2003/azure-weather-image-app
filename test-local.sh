#!/bin/bash
# Test script for local development
# Make sure the Azure Functions app is running locally before executing this script

BASE_URL="http://localhost:7071"
API_KEY="test-api-key-12345"

echo "=== Weather Image API Local Tests ==="
echo ""

# Test 1: Test unauthorized access
echo "Test 1: Testing unauthorized access (should fail)..."
response=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/job/start")
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | head -n-1)

if [ "$http_code" = "401" ]; then
    echo "✓ Unauthorized access correctly rejected"
else
    echo "✗ Expected 401, got $http_code"
fi
echo ""

# Test 2: Test image endpoint
echo "Test 2: Testing image generation..."
response=$(curl -s -w "\n%{http_code}" -X GET "$BASE_URL/api/test/image" -H "X-API-Key: $API_KEY" -o test-image.jpg)
http_code=$(echo "$response" | tail -n1)

if [ "$http_code" = "200" ]; then
    echo "✓ Image generated successfully (saved as test-image.jpg)"
else
    echo "✗ Failed to generate image, got status $http_code"
fi
echo ""

# Test 3: Start a job
echo "Test 3: Starting a new job..."
response=$(curl -s -w "\n%{http_code}" -X POST "$BASE_URL/api/job/start" -H "X-API-Key: $API_KEY" -H "Content-Type: application/json")
http_code=$(echo "$response" | tail -n1)
body=$(echo "$response" | head -n-1)

if [ "$http_code" = "202" ] || [ "$http_code" = "200" ]; then
    echo "✓ Job started successfully"
    echo "Response: $body"
    
    # Extract jobId from response
    job_id=$(echo "$body" | grep -o '"jobId":"[^"]*' | cut -d'"' -f4)
    
    if [ -n "$job_id" ]; then
        echo "Job ID: $job_id"
        
        # Wait a bit for processing
        echo ""
        echo "Waiting 5 seconds for processing..."
        sleep 5
        
        # Test 4: Check job status
        echo ""
        echo "Test 4: Checking job status..."
        response=$(curl -s -w "\n%{http_code}" -X GET "$BASE_URL/api/job/$job_id" -H "X-API-Key: $API_KEY")
        http_code=$(echo "$response" | tail -n1)
        body=$(echo "$response" | head -n-1)
        
        if [ "$http_code" = "200" ]; then
            echo "✓ Job status retrieved successfully"
            echo "Response: $body"
        else
            echo "✗ Failed to get job status, got status $http_code"
        fi
    else
        echo "✗ Could not extract job ID from response"
    fi
else
    echo "✗ Failed to start job, got status $http_code"
    echo "Response: $body"
fi

echo ""
echo "=== Tests Complete ==="
