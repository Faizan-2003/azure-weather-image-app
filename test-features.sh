#!/bin/bash
# Feature test script - comprehensive testing
# Run this after test-local.sh to ensure all features work

BASE_URL="${BASE_URL:-http://localhost:7071}"
API_KEY="${API_KEY:-test-api-key-12345}"

echo "=== Weather Image API Feature Tests ==="
echo "Base URL: $BASE_URL"
echo ""

# Colors for output
GREEN='\033[0;32m'
RED='\033[0;31m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

test_count=0
passed_count=0
failed_count=0

# Function to run a test
run_test() {
    local test_name=$1
    local expected_code=$2
    local actual_code=$3
    
    test_count=$((test_count + 1))
    
    if [ "$actual_code" = "$expected_code" ]; then
        echo -e "${GREEN}✓${NC} $test_name"
        passed_count=$((passed_count + 1))
        return 0
    else
        echo -e "${RED}✗${NC} $test_name (expected $expected_code, got $actual_code)"
        failed_count=$((failed_count + 1))
        return 1
    fi
}

# Test 1: Authentication
echo -e "${YELLOW}=== Authentication Tests ===${NC}"
response=$(curl -s -w "%{http_code}" -o /dev/null -X POST "$BASE_URL/api/job/start")
run_test "Reject request without API key" "401" "$response"

response=$(curl -s -w "%{http_code}" -o /dev/null -X POST "$BASE_URL/api/job/start" -H "X-API-Key: wrong-key")
run_test "Reject request with wrong API key" "401" "$response"

response=$(curl -s -w "%{http_code}" -o /dev/null -X POST "$BASE_URL/api/job/start" -H "X-API-Key: $API_KEY")
run_test "Accept request with correct API key" "202" "$response"

echo ""

# Test 2: Job Management
echo -e "${YELLOW}=== Job Management Tests ===${NC}"

# Start a job
response=$(curl -s -X POST "$BASE_URL/api/job/start" -H "X-API-Key: $API_KEY" -H "Content-Type: application/json")
http_code=$(curl -s -w "%{http_code}" -o /dev/null -X POST "$BASE_URL/api/job/start" -H "X-API-Key: $API_KEY")
run_test "Start new job" "202" "$http_code"

# Extract job ID
job_id=$(echo "$response" | grep -o '"jobId":"[^"]*' | cut -d'"' -f4)

if [ -n "$job_id" ]; then
    echo "Created job ID: $job_id"
    
    # Check job status
    http_code=$(curl -s -w "%{http_code}" -o /dev/null -X GET "$BASE_URL/api/job/$job_id" -H "X-API-Key: $API_KEY")
    run_test "Get job status for existing job" "200" "$http_code"
    
    # Try to get non-existent job
    http_code=$(curl -s -w "%{http_code}" -o /dev/null -X GET "$BASE_URL/api/job/non-existent-id" -H "X-API-Key: $API_KEY")
    run_test "Get 404 for non-existent job" "404" "$http_code"
else
    echo -e "${RED}Failed to create job - skipping dependent tests${NC}"
fi

echo ""

# Test 3: Image Processing
echo -e "${YELLOW}=== Image Processing Tests ===${NC}"

http_code=$(curl -s -w "%{http_code}" -o /dev/null -X GET "$BASE_URL/api/test/image" -H "X-API-Key: $API_KEY")
run_test "Generate test image" "200" "$http_code"

echo ""

# Summary
echo -e "${YELLOW}=== Test Summary ===${NC}"
echo "Total tests: $test_count"
echo -e "${GREEN}Passed: $passed_count${NC}"
echo -e "${RED}Failed: $failed_count${NC}"

if [ $failed_count -eq 0 ]; then
    echo -e "\n${GREEN}All tests passed!${NC}"
    exit 0
else
    echo -e "\n${RED}Some tests failed!${NC}"
    exit 1
fi
