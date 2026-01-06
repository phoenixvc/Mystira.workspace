# Agent Mode Setup & Operations Guide

This guide provides comprehensive instructions for setting up, running, testing, and troubleshooting the Mystira Story Generator's agent-based story generation pipeline with Azure AI Foundry integration.

## Table of Contents

1. [Prerequisites](#prerequisites)
2. [Configuration](#configuration)
3. [Starting the Application](#starting-the-application)
4. [Accessing the UI](#accessing-the-ui)
5. [Testing the System](#testing-the-system)
6. [Monitoring & Debugging](#monitoring--debugging)
7. [Troubleshooting](#troubleshooting)
8. [Architecture Overview](#architecture-overview)

---

## Prerequisites

### Required Azure Resources

Before running the application, ensure you have:

1. **Azure Subscription** with permissions to create resources
2. **Azure AI Foundry Project** (formerly Azure Machine Learning Studio)
3. **Four AI Agents** created in Foundry:
   - **Writer Agent**: Generates initial story content
   - **Judge Agent**: Evaluates story quality against rubrics
   - **Refiner Agent**: Refines stories based on evaluation feedback
   - **Rubric Summary Agent**: (Optional) Summarizes evaluation criteria

4. **Azure AI Search** (Optional - for AISearch knowledge mode)
   - Search service with "mystira-knowledge-index" index configured
   - Index should contain story guidelines and development principles

5. **Vector Store** (Optional - for FileSearch knowledge mode)
   - Configured in Azure AI Foundry
   - Contains story writing guidelines and examples

### Required API Keys

Collect the following credentials:

- Azure AI Foundry endpoint URL
- Azure AI Foundry API key
- Azure AI Search endpoint URL (if using AISearch mode)
- Azure AI Search API key (if using AISearch mode)
- Agent IDs for all four agents

### Development Environment

- .NET 8.0 SDK or later
- Visual Studio 2022 / VS Code / Rider
- SQL Server or PostgreSQL (for session persistence)
- Modern web browser (Chrome, Edge, Firefox, Safari)

---

## Configuration

### 1. Database Setup

Create the database and run migrations:

```bash
# Using SQL Server
dotnet ef database update --project src/Mystira.StoryGenerator.Infrastructure

# Or using PostgreSQL
# Update connection string in appsettings.json first
dotnet ef database update --project src/Mystira.StoryGenerator.Infrastructure
```

### 2. Application Settings

Update `appsettings.json` in `src/Mystira.StoryGenerator.Api/`:

```json
{
  "AzureFoundry": {
    "Endpoint": "https://your-foundry-project.azure.com",
    "ApiKey": "your-foundry-api-key",
    "AgentIds": {
      "Writer": "writer-agent-id-12345",
      "Judge": "judge-agent-id-67890",
      "Refiner": "refiner-agent-id-abcde",
      "RubricSummary": "rubric-agent-id-fghij"
    },
    "DefaultModel": "gpt-4",
    "RunTimeoutSeconds": 300,
    "MaxRetries": 3
  },
  "AzureAISearch": {
    "Endpoint": "https://your-search-service.search.windows.net",
    "ApiKey": "your-search-api-key",
    "IndexName": "mystira-knowledge-index"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=MystiraStoryGen;Trusted_Connection=True;"
  },
  "StoryGeneration": {
    "MaxIterations": 5,
    "DefaultAgeGroup": "6-9",
    "DefaultKnowledgeMode": "FileSearch"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Mystira.StoryGenerator": "Debug"
    }
  }
}
```

### 3. Environment Variables (Alternative to appsettings.json)

For production deployments, use environment variables:

```bash
export AZURE_FOUNDRY_ENDPOINT="https://your-foundry-project.azure.com"
export AZURE_FOUNDRY_API_KEY="your-api-key"
export AZURE_FOUNDRY_WRITER_AGENT_ID="writer-agent-id"
export AZURE_FOUNDRY_JUDGE_AGENT_ID="judge-agent-id"
export AZURE_FOUNDRY_REFINER_AGENT_ID="refiner-agent-id"
export AZURE_AI_SEARCH_ENDPOINT="https://your-search.search.windows.net"
export AZURE_AI_SEARCH_API_KEY="your-search-key"
```

### 4. CORS Configuration

For development with separate frontend:

```json
{
  "AllowedOrigins": [
    "http://localhost:5173",
    "https://localhost:7001"
  ]
}
```

---

## Starting the Application

### Option 1: Visual Studio

1. Open `Mystira.StoryGenerator.sln`
2. Set `Mystira.StoryGenerator.Api` as startup project
3. Press F5 to run with debugging
4. API will start at `https://localhost:7001`

### Option 2: Command Line

```bash
# Build the solution
dotnet build

# Run the API
cd src/Mystira.StoryGenerator.Api
dotnet run

# Or run in watch mode for development
dotnet watch run
```

### Option 3: Docker

```bash
# Build Docker image
docker build -t mystira-story-generator .

# Run container
docker run -p 7001:80 \
  -e AZURE_FOUNDRY_ENDPOINT="your-endpoint" \
  -e AZURE_FOUNDRY_API_KEY="your-key" \
  mystira-story-generator
```

### Expected Output

When the application starts successfully, you should see:

```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
info: Microsoft.Hosting.Lifetime[0]
      Application started. Press Ctrl+C to shut down.
info: Mystira.StoryGenerator.Api.Startup[0]
      Azure AI Foundry configured with endpoint: https://...
info: Mystira.StoryGenerator.Api.Startup[0]
      Knowledge mode: FileSearch
```

### Health Check

Verify the application is running:

```bash
# Check API health
curl https://localhost:7001/health

# Expected response
{
  "status": "Healthy",
  "checks": {
    "database": "Healthy",
    "foundry": "Healthy",
    "aiSearch": "Healthy"
  }
}
```

---

## Accessing the UI

### Blazor WebAssembly UI

1. **Navigate to the application**
   - URL: `https://localhost:7001`
   - Or deployed URL: `https://your-app.azurewebsites.net`

2. **From the home page**
   - Click **"AI Story Generator"** in the navigation menu
   - Or navigate directly to `/story/agent`

### First Story Creation Walkthrough

#### Step 1: Enter Story Details

1. **Story Prompt**: Enter your story idea
   ```
   Example: "A brave knight helps villagers defend their town from a dragon"
   ```

2. **Age Group**: Select target audience
   - 3-5 years
   - 6-9 years
   - 10-12 years

3. **Knowledge Mode**: Choose retrieval method
   - **FileSearch**: Uses vector store with pre-loaded guidelines
   - **AISearch**: Uses Azure AI Search for dynamic knowledge retrieval

4. **Narrative Axes** (Optional): Select themes to emphasize
   - Wonder
   - Discovery
   - Transformation
   - Courage
   - Friendship

5. Click **"Generate Story"**

#### Step 2: Watch Real-Time Progress

The progress panel shows:

- **Phase Timeline**: Visualizes current phase
  - Writing → Validating → Evaluating → Complete
- **Event Log**: Real-time status updates
  ```
  [10:23:45] Session started
  [10:23:46] Writer agent generating story...
  [10:23:52] Story generation complete
  [10:23:53] Validating schema...
  [10:23:54] Schema validation passed
  ```
- **Iteration Counter**: Tracks refinement cycles

#### Step 3: Review Evaluation Results

Once evaluation completes, you'll see:

- **Safety Gate**: Pass/Fail indicator
- **Quality Metrics**:
  - Axes Alignment Score
  - Development Principles Score
  - Narrative Logic Score
- **Recommendation**: "Story is ready" or "Needs refinement"
- **Detailed Findings**: Specific feedback per criterion

#### Step 4: Refine if Needed

If evaluation fails or you want improvements:

1. **Select Refinement Type**:
   - **Targeted Refinement**: Edit specific scenes only
   - **Full Rewrite**: Regenerate entire story

2. **Target Scenes** (for targeted refinement):
   - Select scene IDs to modify (e.g., scene_2, scene_3)
   - Other scenes will be preserved

3. **Refinement Aspects**:
   - Tone
   - Pacing
   - Dialogue
   - Character Development
   - Plot Coherence
   - All

4. **User Guidance** (Optional):
   ```
   Example: "Make the dialogue more age-appropriate and add more descriptive language"
   ```

5. Click **"Refine Story"**

#### Step 5: Publish or Download

Once satisfied:

- **View Story**: Read full story with scene breakdown
- **Download JSON**: Export story data
- **Publish**: Send to production story database

---

## Testing the System

### Manual API Testing with Postman/curl

#### 1. Start a Session

```bash
POST https://localhost:7001/api/story-agent/sessions/start
Content-Type: application/json

{
  "storyPrompt": "A wizard teaches a young apprentice about magic",
  "knowledgeMode": "FileSearch",
  "ageGroup": "6-9",
  "targetAxes": ["wonder", "discovery"]
}

# Response (202 Accepted)
{
  "sessionId": "session-abc123",
  "threadId": "thread-def456",
  "knowledgeMode": "FileSearch",
  "stage": "Uninitialized"
}
```

#### 2. Monitor Generation with SSE

```bash
curl -N -H "Accept: text/event-stream" \
  https://localhost:7001/api/story-agent/sessions/session-abc123/stream

# Output
event: SessionStarted
data: {"sessionId":"session-abc123","timestamp":"2024-01-06T10:23:45Z"}

event: GenerationStarted
data: {"sessionId":"session-abc123","timestamp":"2024-01-06T10:23:46Z"}

event: GenerationComplete
data: {"sessionId":"session-abc123","timestamp":"2024-01-06T10:23:52Z"}
```

#### 3. Check Session Status

```bash
GET https://localhost:7001/api/story-agent/sessions/session-abc123

# Response
{
  "sessionId": "session-abc123",
  "stage": "Validating",
  "iterationCount": 0,
  "currentStoryVersion": "{\"title\":\"...\",\"scenes\":[...]}",
  "createdAt": "2024-01-06T10:23:45Z",
  "updatedAt": "2024-01-06T10:23:52Z"
}
```

#### 4. Evaluate Story

```bash
POST https://localhost:7001/api/story-agent/sessions/session-abc123/evaluate
Content-Type: application/json

{}

# Response
{
  "success": true,
  "report": {
    "iterationNumber": 0,
    "overallStatus": "Pass",
    "safetyGatePassed": true,
    "axesAlignmentScore": 0.92,
    "devPrinciplesScore": 0.88,
    "narrativeLogicScore": 0.90,
    "recommendation": "Story is ready to publish",
    "findings": [...]
  }
}
```

#### 5. Refine Story (if needed)

```bash
POST https://localhost:7001/api/story-agent/sessions/session-abc123/refine
Content-Type: application/json

{
  "targetSceneIds": ["scene_2", "scene_3"],
  "aspects": ["dialogue", "tone"],
  "userGuidance": "Make dialogue more natural for children"
}

# Response (202 Accepted)
{
  "success": true,
  "message": "Refinement started"
}
```

### Automated Testing

Run the comprehensive test suite:

```bash
# Run all tests
dotnet test

# Run specific test categories
dotnet test --filter "Category=Integration"
dotnet test --filter "Category=E2E"
dotnet test --filter "Category=Performance"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run E2E demo scenarios
dotnet test --filter "FullyQualifiedName~AgentPipelineE2ETests"

# Run performance benchmarks
dotnet test --filter "FullyQualifiedName~AgentPipelinePerformanceTests"
```

### Test Coverage Report

```bash
# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# View report
reportgenerator -reports:coverage.cobertura.xml -targetdir:coverage-report
open coverage-report/index.html
```

---

## Monitoring & Debugging

### Viewing Correlation IDs

Every request gets a correlation ID for tracing:

```bash
# API returns correlation ID in response headers
X-Correlation-ID: corr-abc123def456

# Find in logs
grep "corr-abc123def456" logs/application.log
```

### Accessing Iteration History

```bash
GET /api/story-agent/sessions/{sessionId}/history

# Response
{
  "sessionId": "session-abc123",
  "iterations": [
    {
      "iterationNumber": 0,
      "timestamp": "2024-01-06T10:23:52Z",
      "action": "Generate",
      "result": "Pass"
    },
    {
      "iterationNumber": 1,
      "timestamp": "2024-01-06T10:25:10Z",
      "action": "Refine",
      "result": "Pass"
    }
  ]
}
```

### Reviewing Story Versions

```bash
GET /api/story-agent/sessions/{sessionId}/versions

# Response
{
  "sessionId": "session-abc123",
  "versions": [
    {
      "versionNumber": 1,
      "createdAt": "2024-01-06T10:23:52Z",
      "storyJson": "{...}"
    },
    {
      "versionNumber": 2,
      "createdAt": "2024-01-06T10:25:10Z",
      "storyJson": "{...}"
    }
  ]
}
```

### Checking Token Usage

```bash
GET /api/story-agent/sessions/{sessionId}/metrics

# Response
{
  "sessionId": "session-abc123",
  "totalTokensUsed": 15430,
  "breakdown": {
    "generation": 8200,
    "evaluation": 4500,
    "refinement": 2730
  },
  "estimatedCost": 0.046
}
```

### Foundry Agent Run Logs

1. **Navigate to Azure AI Foundry**:
   - Go to your project
   - Click "Agents" in left navigation
   - Select the agent (Writer, Judge, Refiner)

2. **View Run History**:
   - Each run shows:
     - Timestamp
     - Input prompt
     - Output response
     - Token usage
     - Duration

3. **Debug Failed Runs**:
   - Check error messages
   - Review input/output
   - Verify agent configuration

### Application Logs

```bash
# View logs in real-time
tail -f logs/mystira-story-generator.log

# Search for errors
grep "ERROR" logs/mystira-story-generator.log

# Filter by session ID
grep "session-abc123" logs/mystira-story-generator.log

# View structured logs (if using JSON logging)
cat logs/mystira-story-generator.json | jq '.[] | select(.Level == "Error")'
```

---

## Troubleshooting

### Issue: Streaming Not Working

**Symptoms**: No events received in SSE stream, connection times out

**Solutions**:

1. **Check CORS Configuration**:
   ```json
   {
     "AllowedOrigins": ["http://localhost:5173"]
   }
   ```

2. **Verify SSE Headers**:
   ```csharp
   // In API controller
   Response.Headers.Add("Content-Type", "text/event-stream");
   Response.Headers.Add("Cache-Control", "no-cache");
   Response.Headers.Add("Connection", "keep-alive");
   ```

3. **Check Proxy/Load Balancer**:
   - Ensure proxy doesn't buffer SSE responses
   - Configure timeouts appropriately

4. **Test with curl**:
   ```bash
   curl -N -H "Accept: text/event-stream" https://localhost:7001/api/story-agent/sessions/{id}/stream
   ```

### Issue: Agent Timeout

**Symptoms**: `AgentRunTimeoutException`, evaluation fails after 5 minutes

**Solutions**:

1. **Increase Timeout**:
   ```json
   {
     "AzureFoundry": {
       "RunTimeoutSeconds": 600
     }
   }
   ```

2. **Check Agent Configuration**:
   - Verify agent has appropriate max tokens
   - Ensure tools are properly configured

3. **Review Foundry Logs**:
   - Check for rate limiting
   - Verify model availability

4. **Simplify Prompt**:
   - Reduce input complexity
   - Break into smaller tasks

### Issue: Stories Failing Evaluation

**Symptoms**: `overallStatus: "Fail"`, repeated refinements don't help

**Solutions**:

1. **Review Findings**:
   ```json
   {
     "findings": [
       {
         "criterion": "Axes Alignment",
         "score": 0.4,
         "feedback": "Story doesn't emphasize 'wonder' enough"
       }
     ]
   }
   ```

2. **Check Safety Rules**:
   - Ensure content is age-appropriate
   - Verify no prohibited content

3. **Adjust Rubric Weights**:
   ```json
   {
     "EvaluationWeights": {
       "AxesAlignment": 0.4,
       "DevPrinciples": 0.3,
       "NarrativeLogic": 0.3
     }
   }
   ```

4. **Refine with Specific Guidance**:
   ```json
   {
     "userGuidance": "Add more magical elements to increase wonder. Describe the discovery process in detail."
   }
   ```

### Issue: Session Not Found

**Symptoms**: `404 Not Found` when calling evaluate/refine endpoints

**Solutions**:

1. **Verify Session ID**:
   ```bash
   GET /api/story-agent/sessions/{sessionId}
   ```

2. **Check Database**:
   ```sql
   SELECT * FROM StorySessions WHERE SessionId = 'session-abc123';
   ```

3. **Session Expired**:
   - Check session TTL configuration
   - Sessions may be cleaned up after inactivity

4. **Wrong Environment**:
   - Verify connecting to correct API instance
   - Check database connection string

### Issue: JSON Parsing Errors

**Symptoms**: `JsonException`, story validation fails

**Solutions**:

1. **Check Agent Response**:
   ```bash
   # View raw agent output in Foundry logs
   ```

2. **Update Agent Instructions**:
   - Ensure agent returns valid JSON
   - Add JSON schema to agent instructions

3. **Add Retry Logic**:
   ```json
   {
     "AzureFoundry": {
       "MaxRetries": 3
     }
   }
   ```

4. **Validate Response Before Parsing**:
   ```csharp
   // AgentResponseParser includes validation
   var isValid = StorySchemaValidator.Validate(agentResponse);
   ```

### Issue: Max Iterations Reached

**Symptoms**: Session state = `StuckNeedsReview`, can't refine further

**Solutions**:

1. **Human Review Required**:
   - Export story for manual editing
   - Review evaluation findings
   - Adjust prompt and start new session

2. **Increase Max Iterations** (not recommended):
   ```json
   {
     "StoryGeneration": {
       "MaxIterations": 10
     }
   }
   ```

3. **Start Fresh Session**:
   - Incorporate learnings from failed session
   - Improve initial prompt
   - Select different narrative axes

### Issue: Knowledge Retrieval Not Working

**Symptoms**: Stories lack expected guidelines, don't align with principles

**Solutions**:

1. **Verify Knowledge Mode**:
   - FileSearch: Check vector store attachment
   - AISearch: Verify index exists and is populated

2. **Test Knowledge Provider**:
   ```bash
   GET /api/knowledge/test?mode=FileSearch
   GET /api/knowledge/test?mode=AISearch
   ```

3. **Check Vector Store**:
   - Verify files are uploaded
   - Check indexing status

4. **Validate AI Search Index**:
   ```bash
   curl https://your-search.search.windows.net/indexes/mystira-knowledge-index?api-version=2023-11-01 \
     -H "api-key: your-key"
   ```

---

## Architecture Overview

### System Components

```
┌─────────────────────────────────────────────────────────┐
│                     Blazor WebAssembly UI               │
│  (Real-time SSE updates, progress tracking, refinement) │
└───────────────────┬─────────────────────────────────────┘
                    │ HTTP / SSE
                    ▼
┌─────────────────────────────────────────────────────────┐
│                    ASP.NET Core API                     │
│  - StoryAgentController (REST endpoints)                │
│  - SSE Stream endpoint                                  │
│  - Session management                                   │
└───────────────────┬─────────────────────────────────────┘
                    │
                    ▼
┌─────────────────────────────────────────────────────────┐
│                  Agent Orchestrator                     │
│  - State machine management                             │
│  - Pipeline coordination                                │
│  - Event publishing                                     │
└───────────────────┬─────────────────────────────────────┘
                    │
        ┌───────────┴───────────┬─────────────────┐
        ▼                       ▼                 ▼
┌───────────────┐    ┌─────────────────┐   ┌──────────────┐
│  Azure AI     │    │  Knowledge      │   │  Evaluation  │
│  Foundry      │    │  Provider       │   │  Engine      │
│  Agents       │    │  (FileSearch/   │   │  (Schema,    │
│  - Writer     │    │   AISearch)     │   │   Logic,     │
│  - Judge      │    │                 │   │   Judge)     │
│  - Refiner    │    │                 │   │              │
└───────────────┘    └─────────────────┘   └──────────────┘
        │                       │                 │
        └───────────┬───────────┴─────────────────┘
                    ▼
┌─────────────────────────────────────────────────────────┐
│                  Database (SQL Server)                  │
│  - StorySessions                                        │
│  - StoryVersions                                        │
│  - EvaluationReports                                    │
└─────────────────────────────────────────────────────────┘
```

### State Machine Flow

```
Uninitialized
    │
    ▼
Generating (Writer Agent)
    │
    ▼
Validating (Schema Check)
    │
    ▼
Evaluating (Logic + Judge Agent)
    │
    ├─► Pass ───► Evaluated ───► Complete ✓
    │
    └─► Fail ───► RequiresRefinement
                      │
                      ▼
                  Refining (Refiner Agent)
                      │
                      └─► Back to Validating
                          (Loop max 5 times)
                          │
                          └─► StuckNeedsReview (after 5 iterations)
```

### Key Interfaces

- **IAgentOrchestrator**: Main pipeline coordinator
- **IAgentStreamPublisher**: Real-time event streaming
- **IKnowledgeProvider**: Knowledge retrieval abstraction
- **IStorySessionRepository**: Session persistence
- **IEvaluationEngine**: Story quality assessment

---

## Performance Benchmarks

Target metrics for production:

- **Session Start**: < 500ms
- **First SSE Event**: < 500ms
- **Story Generation**: 15-60 seconds (model-dependent)
- **Evaluation**: < 3 seconds
- **Refinement Start**: < 500ms
- **Concurrent Sessions**: 100+ simultaneous users

---

## Support & Feedback

For issues, questions, or feature requests:

- **GitHub Issues**: https://github.com/your-org/mystira-story-generator/issues
- **Documentation**: https://docs.mystira.ai
- **Email**: support@mystira.ai

---

## Next Steps

1. ✅ Complete initial setup
2. ✅ Test with sample story
3. ✅ Review evaluation metrics
4. ✅ Test refinement flow
5. 🔄 Customize rubrics for your use case
6. 🔄 Add custom narrative axes
7. 🔄 Deploy to staging environment
8. 🔄 Load testing
9. 🔄 Production deployment

---

**Last Updated**: January 2024  
**Version**: 1.0.0
