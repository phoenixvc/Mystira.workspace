# Quick Fix: Assistant ID Error

## The Problem

You're seeing this error:

```
Error: Story generation failed: No assistant found with id 'asst-mystira-write-v01'. Status: 404 (Not Found)
```

This happens because `appsettings.json` contains placeholder agent names instead of actual Azure AI agent IDs.

## The Solution

You need to retrieve the actual agent IDs from Azure and update your configuration.

---

## Step 1: Retrieve Your Agent IDs

Choose ONE of the following methods:

### Method A: Using the AgentSetup Tool (Fastest)

```bash
cd src/Mystira.StoryGenerator.AgentSetup
dotnet run list --endpoint "https://mys-shared-ai-san.services.ai.azure.com/api/projects/mys-shared-ai-san-project"
```

This will display all your agents and their IDs.

### Method B: Using the Bash Script

```bash
# Make sure you're logged in to Azure first
az login

# Run the list script
./scripts/list-agents.sh
```

### Method C: Using the Azure Portal (Manual)

1. Go to https://portal.azure.com
2. Navigate to your Azure AI Foundry project "mys-shared-ai-san-project"
3. Click "Agents" in the left menu
4. For each agent (writer, judge, refiner, rubric), click on its name
5. Copy the "Agent ID" from the details page (it looks like `asst_abc123xyz456`)

---

## Step 2: Update appsettings.json

Edit `src/Mystira.StoryGenerator.Api/appsettings.json`:

### Current Configuration (WRONG):

```json
"FoundryAgent": {
  "WriterAgentId": "mystira-writer-v01",        ← Placeholder, not a real ID
  "JudgeAgentId": "mystira-judge-v01",          ← Placeholder, not a real ID
  "RefinerAgentId": "mystira-refiner-v01",      ← Placeholder, not a real ID
  "RubricSummaryAgentId": "mystira-rubric-v01", ← Placeholder, not a real ID
  ...
}
```

### Correct Configuration:

```json
"FoundryAgent": {
  "WriterAgentId": "asst_abc123xyz456",         ← Real ID from Azure
  "JudgeAgentId": "asst_def789uvw012",          ← Real ID from Azure
  "RefinerAgentId": "asst_ghi345rst678",        ← Real ID from Azure
  "RubricSummaryAgentId": "asst_jkl901mno234",  ← Real ID from Azure
  ...
}
```

**Important:**
- Agent IDs MUST start with `asst_` (with underscore)
- Do NOT use the agent names (like "mystira-writer-v01")
- Get the IDs from Step 1 above

---

## Step 3: Verify

Build and run the application to verify the fix:

```bash
cd src/Mystira.StoryGenerator.Api
dotnet build
dotnet run
```

You should see:
```
info: Mystira.StoryGenerator.Api.Startup[0]
      Azure AI Foundry configured with endpoint: https://mys-shared-ai-san...
```

No more 404 errors about assistants not being found!

---

## Understanding the Error

### What the error shows:
```
No assistant found with id 'asst-mystira-write-v01'
```

### Why it fails:
- `asst-mystira-write-v01` is the agent **name**, not the **ID**
- Azure needs the actual ID (like `asst_abc123xyz456`)
- Names are NOT valid agent identifiers for the API

### The fix:
Replace placeholder names with real IDs retrieved from Azure.

---

## Still Having Issues?

### If you see: "Invalid agent configuration detected"

Your IDs still have placeholder values. Make sure:
1. Each ID starts with `asst_` (with underscore)
2. You copied the IDs correctly from Azure (no typos)
3. You saved the appsettings.json file after editing

### If you see: "Failed to get Azure access token"

Run `az login` to authenticate with Azure.

### If agents don't exist in Azure yet

Create them using:

```bash
cd src/Mystira.StoryGenerator.AgentSetup
dotnet run create --endpoint "https://mys-shared-ai-san.services.ai.azure.com/api/projects/mys-shared-ai-san-project" --model "gpt-4"
```

(Replace "gpt-4" with your actual model deployment name)

---

## More Help

- See [docs/AGENT_SETUP_GUIDE.md](docs/AGENT_SETUP_GUIDE.md) for detailed instructions
- See [scripts/README.md](scripts/README.md) for script documentation
