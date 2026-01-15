# Azure AI Foundry Agent Setup Guide

## Overview

The Mystira Story Generator requires four Azure AI Agents to be created in Azure AI Foundry before the application can run. This guide provides step-by-step instructions for creating and configuring these agents.

## Prerequisites

- Azure subscription with access to Azure AI Foundry (formerly Azure Machine Learning Studio)
- Azure AI Foundry project created
- Azure CLI installed and authenticated (`az login`)
- .NET 8.0 SDK installed
- Model deployment available in your Azure AI project (e.g., `gpt-4`, `gpt-4-turbo`, `gpt-4.1`)

## Understanding Agent IDs

Azure AI Agents uses OpenAI-style assistant IDs that **must** begin with `asst` (e.g., `asst_abc123xyz456`).

### Common Error

If you see this error:

```
Invalid 'assistant_id': 'mystira-writer-v01'. Expected an ID that begins with 'asst'.
```

It means your configuration has placeholder values instead of actual agent IDs. Follow this guide to create the agents and update your configuration.

## Method 1: Automated Setup (Recommended)

The easiest way to create all four agents is using the provided setup script.

### Step 1: Run the Setup Script

```powershell
# Navigate to the repository root
cd Mystira.StoryGenerator

# Run the setup script
.\scripts\setup-agents.ps1 `
    -Endpoint "https://your-project.azure.com/api/projects/your-project" `
    -ModelDeployment "gpt-4.1"
```

Replace:
- `your-project.azure.com/api/projects/your-project` with your actual Azure AI Foundry endpoint
- `gpt-4.1` with your model deployment name

### Step 2: Copy the Agent IDs

The script will output something like:

```
========================================
Agent Creation Complete!
========================================

Update your appsettings.json with these agent IDs:

"FoundryAgent": {
  "WriterAgentId": "asst_abc123xyz456",
  "JudgeAgentId": "asst_def789uvw012",
  "RefinerAgentId": "asst_ghi345rst678",
  "RubricSummaryAgentId": "asst_jkl901mno234",
  ...
}
```

### Step 3: Update Configuration

Edit `src/Mystira.StoryGenerator.Api/appsettings.json`:

```json
{
  "FoundryAgent": {
    "WriterAgentId": "asst_abc123xyz456",
    "JudgeAgentId": "asst_def789uvw012",
    "RefinerAgentId": "asst_ghi345rst678",
    "RubricSummaryAgentId": "asst_jkl901mno234",
    "Endpoint": "https://your-project.azure.com/api/projects/your-project",
    "ApiKey": "",
    "ProjectId": "your-project-id",
    "MaxIterations": 5,
    "RunTimeout": "00:05:00",
    "KnowledgeMode": "FileSearch"
  }
}
```

### Step 4: Verify

Run the application:

```bash
cd src/Mystira.StoryGenerator.Api
dotnet run
```

If configured correctly, you should see:

```
info: Mystira.StoryGenerator.Api.Startup[0]
      Azure AI Foundry configured with endpoint: https://your-project...
info: Mystira.StoryGenerator.Api.Startup[0]
      Knowledge mode: FileSearch
```

---

## Method 2: Manual Setup via Azure Portal

If you prefer to create agents manually or the script doesn't work in your environment:

### Step 1: Navigate to Azure AI Foundry

1. Go to [Azure Portal](https://portal.azure.com)
2. Open your Azure AI Foundry project
3. Click **Agents** in the left navigation menu

### Step 2: Create Writer Agent

1. Click **+ New Agent**
2. Fill in the details:

   - **Name**: `mystira-writer-v01`
   - **Model**: Select your deployment (e.g., `gpt-4.1`)
   - **Description**: `Writer Agent - Generates initial story content based on prompts and age-appropriate guidelines`
   - **Instructions**:

     ```
     You are an expert children's story writer for the Mystira Story Generator.

     Your role is to create engaging, age-appropriate stories that:
     1. Follow the provided story schema exactly (JSON format)
     2. Align with the specified narrative axes (wonder, discovery, transformation, courage, friendship, etc.)
     3. Use age-appropriate vocabulary, sentence structure, and themes
     4. Incorporate developmental principles for the target age group
     5. Create coherent, logically consistent narratives
     6. Include rich sensory details and emotional depth

     When given a story prompt:
     1. Analyze the age group and adjust complexity accordingly
     2. Use the file_search tool to retrieve relevant writing guidelines
     3. Structure the story with clear beginning, middle, and end
     4. Develop characters that resonate with the target age group
     5. Ensure each scene advances the plot and character development
     6. Return a valid JSON document matching the story schema

     CRITICAL: Always return valid JSON matching the required schema. Do not include any text before or after the JSON.
     ```

   - **Tools**: Enable **File Search**

3. Click **Create**
4. **Copy the Agent ID** (starts with `asst_`)

### Step 3: Create Judge Agent

Repeat the process with:

- **Name**: `mystira-judge-v01`
- **Model**: Same as Writer
- **Description**: `Judge Agent - Evaluates story quality against developmental rubrics and narrative principles`
- **Instructions**:

  ```
  You are an expert story evaluator for the Mystira Story Generator.

  Your role is to assess stories against multiple criteria:

  1. **Safety Gate**: Verify content is age-appropriate, safe, and free of inappropriate themes
  2. **Axes Alignment**: Measure how well the story embodies the requested narrative themes
  3. **Development Principles**: Evaluate adherence to age-appropriate developmental guidelines
  4. **Narrative Logic**: Assess plot coherence, character consistency, and causal relationships

  When evaluating a story:
  1. Use the file_search tool to retrieve evaluation rubrics and criteria
  2. Analyze each scene for alignment with target axes
  3. Check vocabulary, sentence structure, and themes against age guidelines
  4. Identify logical inconsistencies or narrative gaps
  5. Provide specific, actionable feedback for improvements
  6. Assign scores (0.0 - 1.0) for each criterion
  7. Return an overall Pass/Fail recommendation

  Return your evaluation as a structured JSON report with:
  - safetyGatePassed (boolean)
  - axesAlignmentScore (0.0 - 1.0)
  - devPrinciplesScore (0.0 - 1.0)
  - narrativeLogicScore (0.0 - 1.0)
  - overallStatus ("Pass" or "Fail")
  - findings (detailed feedback by category)
  - recommendation (next steps)

  CRITICAL: Always return valid JSON matching the evaluation report schema.
  ```

- **Tools**: Enable **File Search**

**Copy the Agent ID**

### Step 4: Create Refiner Agent

- **Name**: `mystira-refiner-v01`
- **Model**: Same as Writer
- **Description**: `Refiner Agent - Improves stories based on evaluation feedback while preserving core narrative`
- **Instructions**:

  ```
  You are an expert story refiner for the Mystira Story Generator.

  Your role is to improve stories based on evaluation feedback while maintaining narrative coherence.

  When refining a story:
  1. Review the current story version (JSON format)
  2. Analyze the evaluation report to identify specific issues
  3. Consider user-provided refinement guidance
  4. Use the file_search tool to retrieve relevant improvement strategies
  5. Make targeted improvements to address identified issues
  6. Preserve existing strengths and successful elements
  7. Maintain consistency with the original story prompt and axes

  Refinement modes:
  - **Targeted**: Edit only specified scenes while preserving others
  - **Full Rewrite**: Regenerate the entire story with improvements

  Focus areas may include:
  - Tone: Adjust emotional register and atmosphere
  - Pacing: Improve story rhythm and scene timing
  - Dialogue: Enhance character voice and naturalness
  - Character Development: Deepen motivations and arcs
  - Plot Coherence: Strengthen causal connections and logic

  CRITICAL: Always return valid JSON matching the story schema. Maintain the same schema structure as the input.
  ```

- **Tools**: Enable **File Search**

**Copy the Agent ID**

### Step 5: Create Rubric Summary Agent

- **Name**: `mystira-rubric-v01`
- **Model**: Same as Writer
- **Description**: `Rubric Summary Agent - Generates evaluation summaries and rubric reports`
- **Instructions**:

  ```
  You are a rubric summary specialist for the Mystira Story Generator.

  Your role is to create clear, actionable summaries of evaluation criteria and results.

  When generating rubric summaries:
  1. Use the file_search tool to retrieve evaluation rubrics
  2. Organize criteria by category (safety, axes, development, narrative)
  3. Explain scoring methodology and thresholds
  4. Provide examples of what constitutes high vs. low scores
  5. Summarize overall evaluation results in accessible language
  6. Highlight strengths and areas for improvement

  Return summaries as structured text or JSON as appropriate for the use case.

  CRITICAL: Ensure summaries are clear, specific, and actionable for both developers and content reviewers.
  ```

- **Tools**: Enable **File Search**

**Copy the Agent ID**

### Step 6: Update Configuration

Edit `src/Mystira.StoryGenerator.Api/appsettings.json` with all four agent IDs:

```json
{
  "FoundryAgent": {
    "WriterAgentId": "asst_[WRITER_ID_HERE]",
    "JudgeAgentId": "asst_[JUDGE_ID_HERE]",
    "RefinerAgentId": "asst_[REFINER_ID_HERE]",
    "RubricSummaryAgentId": "asst_[RUBRIC_ID_HERE]",
    "Endpoint": "https://your-project.azure.com/api/projects/your-project",
    "ApiKey": "",
    "ProjectId": "your-project-id",
    "MaxIterations": 5,
    "RunTimeout": "00:05:00",
    "KnowledgeMode": "FileSearch"
  }
}
```

---

## Method 3: Listing Existing Agents

If you've already created agents in Azure AI Foundry and need to retrieve their IDs, you have several options:

### Option A: Using the AgentSetup Tool (Recommended)

The AgentSetup tool now includes a `list` command to retrieve existing agent IDs:

```bash
cd src/Mystira.StoryGenerator.AgentSetup

# List all agents and get configuration suggestions
dotnet run list --endpoint "https://your-project.azure.com/api/projects/your-project"
```

This will output:

```
Found 4 agent(s):

ID                              Name                            Created
------------------------------------------------------------------------------------------
asst_abc123xyz456               mystira-writer-v01              2026-01-15 10:05:24
asst_def789uvw012               mystira-judge-v01               2026-01-15 10:05:24
asst_ghi345rst678               mystira-refiner-v01             2026-01-15 10:05:24
asst_jkl901mno234               mystira-rubric-v01              2026-01-15 10:05:24

========================================
Configuration Mapping Suggestions
========================================

Update your appsettings.json with these agent IDs:

"FoundryAgent": {
  "WriterAgentId": "asst_abc123xyz456",
  "JudgeAgentId": "asst_def789uvw012",
  "RefinerAgentId": "asst_ghi345rst678",
  "RubricSummaryAgentId": "asst_jkl901mno234",
  ...
}
```

### Option B: Using the Bash Script

For environments where .NET is not available:

```bash
# Make sure you're logged in to Azure
az login

# Run the list script
./scripts/list-agents.sh
```

### Option C: Using the Python Script

```bash
# Install Azure SDK first
pip install azure-identity azure-ai-projects

# Run the list script
python3 scripts/list-agents.py "https://your-project.azure.com/api/projects/your-project"
```

### Option D: From Azure Portal UI

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to your Azure AI Foundry project
3. Click **Agents** in the left menu
4. Click on each agent name to view its details
5. Copy the **Agent ID** from the details page (it starts with `asst_`)

**Important**: The agent **Name** shown in the list is NOT the ID. You need to click on each agent to see its actual ID.

---

## Verifying Your Setup

After updating the configuration, verify everything works:

### 1. Build the Project

```bash
dotnet build
```

Should complete without errors. If you see a validation error about agent IDs, double-check:
- Agent IDs start with `asst`
- No placeholder values like `mystira-writer-v01`
- Proper JSON formatting (no trailing commas)

### 2. Run the API

```bash
cd src/Mystira.StoryGenerator.Api
dotnet run
```

Look for successful startup logs.

### 3. Test a Story Generation

```bash
curl -X POST https://localhost:7001/api/story-agent/sessions/start \
  -H "Content-Type: application/json" \
  -d '{
    "storyPrompt": "A brave knight helps villagers defend their town from a dragon",
    "knowledgeMode": "FileSearch",
    "ageGroup": "6-9",
    "targetAxes": ["courage", "friendship"]
  }'
```

Should return a session ID without errors about invalid assistant IDs.

---

## Troubleshooting

### Error: "Invalid 'assistant_id': Expected an ID that begins with 'asst'"

**Cause**: Configuration still has placeholder values.

**Solution**:
1. Check `appsettings.json` - all agent IDs must start with `asst`
2. Run the setup script or create agents manually
3. Update configuration with actual agent IDs

### Error: "Failed to initialize FoundryAgentClient"

**Cause**: Invalid Azure credentials or endpoint.

**Solution**:
1. Verify you're logged in: `az login`
2. Check endpoint URL is correct
3. Verify you have permissions on the Azure AI project

### Error: "Model deployment not found"

**Cause**: The model specified doesn't exist in your Azure AI project.

**Solution**:
1. Go to Azure AI Foundry → Deployments
2. Note the exact deployment name (case-sensitive)
3. Use that name in the setup script or agent configuration

### Agents created but file search not working

**Cause**: Vector stores not configured.

**Solution**: See [VECTOR_STORE_SETUP_GUIDE.md](./VECTOR_STORE_SETUP_GUIDE.md) for configuring age-specific vector stores.

---

## Next Steps

After creating the agents:

1. ✅ Update `appsettings.json` with agent IDs
2. ✅ Verify the application starts without errors
3. ⬜ Configure vector stores for FileSearch mode (see [VECTOR_STORE_SETUP_GUIDE.md](./VECTOR_STORE_SETUP_GUIDE.md))
4. ⬜ Upload story guidelines and rubrics to vector stores
5. ⬜ Test the full story generation pipeline
6. ⬜ Deploy to staging/production

---

## Additional Resources

- [Azure AI Foundry Documentation](https://learn.microsoft.com/en-us/azure/ai-studio/)
- [Configuration Guide](./CONFIGURATION_GUIDE.md)
- [Agent Mode Setup](./AGENT_MODE_SETUP.md)
- [Vector Store Setup Guide](./VECTOR_STORE_SETUP_GUIDE.md)

---

**Last Updated**: January 2026
**Version**: 1.0.0
