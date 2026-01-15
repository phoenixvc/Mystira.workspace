namespace Mystira.StoryGenerator.Contracts.Configuration;

/// <summary>
/// Validates FoundryAgentConfig to ensure proper setup.
/// </summary>
public static class FoundryAgentConfigValidator
{
    /// <summary>
    /// Validates that agent IDs are properly configured (not placeholder values).
    /// </summary>
    /// <param name="config">The configuration to validate</param>
    /// <exception cref="InvalidOperationException">Thrown if configuration is invalid</exception>
    public static void ValidateAgentIds(FoundryAgentConfig config)
    {
        var errors = new List<string>();

        // Check Writer Agent ID
        if (!IsValidAgentId(config.WriterAgentId))
        {
            errors.Add($"WriterAgentId '{config.WriterAgentId}' is invalid. Expected an ID that begins with 'asst'.");
        }

        // Check Judge Agent ID
        if (!IsValidAgentId(config.JudgeAgentId))
        {
            errors.Add($"JudgeAgentId '{config.JudgeAgentId}' is invalid. Expected an ID that begins with 'asst'.");
        }

        // Check Refiner Agent ID (if configured)
        if (!string.IsNullOrEmpty(config.RefinerAgentId) && !IsValidAgentId(config.RefinerAgentId))
        {
            errors.Add($"RefinerAgentId '{config.RefinerAgentId}' is invalid. Expected an ID that begins with 'asst'.");
        }

        // Check Rubric Summary Agent ID (if configured)
        if (!string.IsNullOrEmpty(config.RubricSummaryAgentId) && !IsValidAgentId(config.RubricSummaryAgentId))
        {
            errors.Add($"RubricSummaryAgentId '{config.RubricSummaryAgentId}' is invalid. Expected an ID that begins with 'asst'.");
        }

        if (errors.Any())
        {
            var errorMessage = string.Join("\n", errors);
            throw new InvalidOperationException(
                $@"Invalid agent configuration detected:

{errorMessage}

Azure AI Agents requires agent IDs that begin with 'asst' (e.g., 'asst_abc123xyz').

QUICK FIX - If agents already exist in Azure:

   List existing agents:
   cd src/Mystira.StoryGenerator.AgentSetup
   dotnet run list --endpoint ""https://your-project.azure.com/api/projects/your-project""

   OR use the bash script:
   ./scripts/list-agents.sh

   Then copy the IDs shown and update appsettings.json.

FULL SETUP - If agents don't exist yet:

1. Create agents in Azure AI Foundry using the setup script:

   PowerShell:
   .\scripts\setup-agents.ps1 -Endpoint ""https://your-project.azure.com/api/projects/your-project"" -ModelDeployment ""gpt-4""

   OR .NET tool:
   cd src/Mystira.StoryGenerator.AgentSetup
   dotnet run create --endpoint ""https://your-project.azure.com/api/projects/your-project"" --model ""gpt-4""

2. Update appsettings.json with the actual agent IDs:

   ""FoundryAgent"": {{
     ""WriterAgentId"": ""asst_your_writer_agent_id"",
     ""JudgeAgentId"": ""asst_your_judge_agent_id"",
     ""RefinerAgentId"": ""asst_your_refiner_agent_id"",
     ""RubricSummaryAgentId"": ""asst_your_rubric_agent_id"",
     ...
   }}

For more information:
- Quick Start: QUICKSTART_AGENT_IDS.md
- Full Guide: docs/AGENT_SETUP_GUIDE.md");
        }
    }

    /// <summary>
    /// Checks if an agent ID is valid (starts with 'asst').
    /// </summary>
    private static bool IsValidAgentId(string? agentId)
    {
        if (string.IsNullOrWhiteSpace(agentId))
        {
            return false;
        }

        // Azure AI Agents uses OpenAI-style assistant IDs that start with 'asst'
        return agentId.StartsWith("asst", StringComparison.OrdinalIgnoreCase);
    }
}
