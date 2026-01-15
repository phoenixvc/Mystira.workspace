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

## Future Scripts

Additional scripts may be added for:
- Vector store setup and document upload
- Configuration validation
- Deployment automation
- Testing and verification

---

**Last Updated**: January 2026
