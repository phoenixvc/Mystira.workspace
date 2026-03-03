#!/bin/bash
# Lists all Azure AI Foundry agents and their IDs using the REST API.
# This script helps you find the correct assistant IDs to use in appsettings.json.
#
# Usage: ./list-agents.sh

set -e

ENDPOINT="${AZURE_AI_ENDPOINT:-https://mys-shared-ai-san.services.ai.azure.com/api/projects/mys-shared-ai-san-project}"

echo "Connecting to Azure AI Foundry..."
echo "Endpoint: $ENDPOINT"
echo ""

# Get Azure access token
echo "Authenticating..."
TOKEN=$(az account get-access-token --resource https://cognitiveservices.azure.com --query accessToken -o tsv 2>/dev/null)

if [ -z "$TOKEN" ]; then
    echo "✗ Failed to get Azure access token"
    echo ""
    echo "Please run: az login"
    echo ""
    exit 1
fi

echo "✓ Authenticated successfully"
echo ""
echo "Retrieving agents..."
echo ""

# Call the Azure AI API to list agents
RESPONSE=$(curl -s -X GET \
    "${ENDPOINT}/agents" \
    -H "Authorization: Bearer $TOKEN" \
    -H "Content-Type: application/json" \
    -H "api-version: 2024-12-01-preview")

# Check if the response contains an error
if echo "$RESPONSE" | grep -q '"error"'; then
    echo "✗ Failed to retrieve agents:"
    echo "$RESPONSE" | jq -r '.error.message' 2>/dev/null || echo "$RESPONSE"
    echo ""
    echo "Common issues:"
    echo "  1. Invalid Azure credentials (run 'az login')"
    echo "  2. Insufficient permissions on the Azure AI Project"
    echo "  3. Invalid endpoint URL"
    echo "  4. Wrong API version"
    exit 1
fi

# Parse and display agents
AGENT_COUNT=$(echo "$RESPONSE" | jq -r '.data | length' 2>/dev/null || echo "0")

if [ "$AGENT_COUNT" -eq "0" ]; then
    echo "No agents found."
    echo ""
    echo "Run the AgentSetup tool to create agents:"
    echo "  cd src/Mystira.StoryGenerator.AgentSetup"
    echo "  dotnet run create --endpoint $ENDPOINT --model <model-name>"
    exit 0
fi

echo "Found $AGENT_COUNT agent(s):"
echo ""
printf "%-35s %-35s %-25s\n" "ID" "Name" "Created"
echo "----------------------------------------------------------------------------------------------------"

echo "$RESPONSE" | jq -r '.data[] | "\(.id) \(.name // "Unnamed") \(.created_at | tostring)"' | \
    while read -r id name created; do
        printf "%-35s %-35s %-25s\n" "$id" "$name" "$created"
    done

echo ""
echo "========================================================================================================"
echo "Configuration Mapping Suggestions"
echo "========================================================================================================"
echo ""

# Extract agent IDs based on names
WRITER_ID=$(echo "$RESPONSE" | jq -r '.data[] | select(.name | test("writer"; "i")) | .id' | head -n1)
JUDGE_ID=$(echo "$RESPONSE" | jq -r '.data[] | select(.name | test("judge"; "i")) | .id' | head -n1)
REFINER_ID=$(echo "$RESPONSE" | jq -r '.data[] | select(.name | test("refiner"; "i")) | .id' | head -n1)
RUBRIC_ID=$(echo "$RESPONSE" | jq -r '.data[] | select(.name | test("rubric"; "i")) | .id' | head -n1)

echo "Update your appsettings.json with these agent IDs:"
echo ""
echo '"FoundryAgent": {'

if [ -n "$WRITER_ID" ]; then
    echo "  \"WriterAgentId\": \"$WRITER_ID\","
fi
if [ -n "$JUDGE_ID" ]; then
    echo "  \"JudgeAgentId\": \"$JUDGE_ID\","
fi
if [ -n "$REFINER_ID" ]; then
    echo "  \"RefinerAgentId\": \"$REFINER_ID\","
fi
if [ -n "$RUBRIC_ID" ]; then
    echo "  \"RubricSummaryAgentId\": \"$RUBRIC_ID\","
fi

echo '  ...'
echo '}'
echo ""

# Warn about missing mappings
MISSING=""
[ -z "$WRITER_ID" ] && MISSING="$MISSING WriterAgentId"
[ -z "$JUDGE_ID" ] && MISSING="$MISSING JudgeAgentId"
[ -z "$REFINER_ID" ] && MISSING="$MISSING RefinerAgentId"
[ -z "$RUBRIC_ID" ] && MISSING="$MISSING RubricSummaryAgentId"

if [ -n "$MISSING" ]; then
    echo "WARNING: Could not find agents for:"
    for key in $MISSING; do
        echo "  - $key"
    done
    echo ""
    echo "You may need to create these agents."
    echo ""
fi
