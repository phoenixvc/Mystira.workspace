# Mystira Story Generator - Setup Scripts

This directory contains utility scripts for setting up and configuring the Mystira Story Generator.

## Available Scripts

### setup-agents.ps1

Creates all four required Azure AI Foundry agents for the story generation pipeline.

**Usage:**

```powershell
.\setup-agents.ps1 `
    -Endpoint "https://your-project.azure.com/api/projects/your-project" `
    -ModelDeployment "gpt-4.1"
```

**Parameters:**

- `Endpoint` (required): Your Azure AI Foundry project endpoint URL
- `ModelDeployment` (required): The model deployment name to use for the agents

**Output:**

The script will:
1. Build the AgentSetup tool
2. Create four agents in Azure AI Foundry:
   - Writer Agent (`mystira-writer-v01`)
   - Judge Agent (`mystira-judge-v01`)
   - Refiner Agent (`mystira-refiner-v01`)
   - Rubric Summary Agent (`mystira-rubric-v01`)
3. Display the agent IDs for you to copy into `appsettings.json`

**Example Output:**

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

**Prerequisites:**

- Azure CLI installed and authenticated (`az login`)
- .NET 8.0 SDK installed
- Access to Azure AI Foundry project
- Model deployment available in your project

**Troubleshooting:**

If the script fails:

1. **Authentication Error**: Run `az login` to authenticate with Azure
2. **Permission Error**: Ensure you have Contributor or Owner role on the Azure AI project
3. **Model Not Found**: Verify the model deployment name is correct (case-sensitive)
4. **Network Error**: Check your internet connection and Azure service status

For detailed setup instructions, see [docs/AGENT_SETUP_GUIDE.md](../docs/AGENT_SETUP_GUIDE.md).

---

### list-agents.sh

Retrieves all agent IDs from your Azure AI Foundry project using the Azure REST API.

**Usage:**

```bash
./scripts/list-agents.sh
```

The script will:
1. Authenticate using Azure CLI credentials
2. Query the Azure AI Foundry API for all agents
3. Display a formatted table of agents with their IDs, names, and creation dates
4. Provide configuration mapping suggestions for `appsettings.json`

**Output Example:**

```
Found 4 agent(s):

ID                              Name                            Created
----------------------------------------------------------------------------------------------------
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

**Prerequisites:**

- Azure CLI installed and authenticated (`az login`)
- `curl` and `jq` installed
- Access to the Azure AI Foundry project

**Environment Variables:**

You can override the default endpoint by setting:

```bash
export AZURE_AI_ENDPOINT="https://your-project.azure.com/api/projects/your-project"
./scripts/list-agents.sh
```

---

### list-agents.py

Python alternative to the bash script for listing agents.

**Prerequisites:**

```bash
pip install azure-identity azure-ai-projects
az login
```

**Usage:**

```bash
python3 scripts/list-agents.py "https://your-project.azure.com/api/projects/your-project"
```

---

## Troubleshooting

### Error: "az: command not found"

Install Azure CLI:
- **macOS**: `brew install azure-cli`
- **Ubuntu/Debian**: `curl -sL https://aka.ms/InstallAzureCLIDeb | sudo bash`
- **Windows**: Download from https://aka.ms/installazurecliwindows

Then run `az login` to authenticate.

### Error: "jq: command not found"

Install jq:
- **macOS**: `brew install jq`
- **Ubuntu/Debian**: `sudo apt-get install jq`
- **Windows**: Download from https://stedolan.github.io/jq/

### Error: "Failed to get Azure access token"

Run `az login` to authenticate with Azure.

### Error: "Failed to retrieve agents: 403 Forbidden"

You don't have permissions to access the Azure AI project. Ask your Azure administrator to grant you "Contributor" or "Azure AI Developer" role on the project.

---

## Future Scripts

Additional scripts may be added for:
- Vector store setup and document upload
- Configuration validation
- Deployment automation
- Testing and verification

---

**Last Updated**: January 2026
