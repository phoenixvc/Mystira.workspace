using System.CommandLine;
using Azure.AI.Projects;
using Azure.AI.Agents.Persistent;
using Azure.Identity;

var endpointOption = new Option<string>(
    name: "--endpoint",
    description: "Azure AI Foundry endpoint URL")
{
    IsRequired = true
};

var modelOption = new Option<string>(
    name: "--model",
    description: "Model deployment name (e.g., gpt-4, gpt-4-turbo)")
{
    IsRequired = true
};

var rootCommand = new RootCommand("Manages Azure AI Foundry agents for Mystira Story Generator");

// List command
var listCommand = new Command("list", "Lists all existing agents and their IDs");
listCommand.AddOption(endpointOption);
listCommand.SetHandler(async (endpoint) =>
{
    await ListAgentsAsync(endpoint);
}, endpointOption);

// Create command
var createCommand = new Command("create", "Creates new agents");
createCommand.AddOption(endpointOption);
createCommand.AddOption(modelOption);
createCommand.SetHandler(async (endpoint, model) =>
{
    await CreateAgentsAsync(endpoint, model);
}, endpointOption, modelOption);

rootCommand.AddCommand(listCommand);
rootCommand.AddCommand(createCommand);

return await rootCommand.InvokeAsync(args);

static Task ListAgentsAsync(string endpoint)
{
    Console.WriteLine("Connecting to Azure AI Foundry...");

    var projectClient = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential());

    Console.WriteLine("✓ Connected successfully");
    Console.WriteLine();
    Console.WriteLine("Retrieving agents...");
    Console.WriteLine();

    try
    {
        var agents = projectClient.Agents.GetAgents();

        var agentList = new List<(string Id, string Name)>();

        foreach (var agent in agents)
        {
            agentList.Add((agent.Id, agent.Name ?? "Unnamed"));
        }

        if (!agentList.Any())
        {
            Console.WriteLine("No agents found.");
            Console.WriteLine();
            Console.WriteLine("Run 'dotnet run create --endpoint <url> --model <model>' to create agents.");
            return Task.CompletedTask;
        }

        Console.WriteLine($"Found {agentList.Count} agent(s):");
        Console.WriteLine();
        Console.WriteLine($"{"ID",-35} {"Name",-35}");
        Console.WriteLine(new string('-', 75));

        foreach (var agent in agentList.OrderBy(a => a.Name))
        {
            var isValid = agent.Id.StartsWith("asst_");
            var status = isValid ? "" : " ⚠️  INVALID FORMAT";
            Console.WriteLine($"{agent.Id,-35} {agent.Name,-35}{status}");
        }

        Console.WriteLine();

        // Check for invalid agent IDs
        var invalidAgents = agentList.Where(a => !a.Id.StartsWith("asst_")).ToList();
        if (invalidAgents.Any())
        {
            Console.WriteLine();
            Console.WriteLine("========================================");
            Console.WriteLine("⚠️  WARNING: Invalid Agent IDs Detected");
            Console.WriteLine("========================================");
            Console.WriteLine();
            Console.WriteLine($"Found {invalidAgents.Count} agent(s) with INVALID ID format:");
            foreach (var agent in invalidAgents)
            {
                Console.WriteLine($"  - {agent.Id}");
            }
            Console.WriteLine();
            Console.WriteLine("Azure AI Agents requires IDs in OpenAI format: asst_[random]");
            Console.WriteLine("These agents will NOT work with the API.");
            Console.WriteLine();
            Console.WriteLine("SOLUTION:");
            Console.WriteLine("  1. Create new agents with auto-generated IDs:");
            Console.WriteLine($"     dotnet run create --endpoint {endpoint} --model <model>");
            Console.WriteLine("  2. Update appsettings.json with the new IDs");
            Console.WriteLine("  3. (Optional) Delete the invalid agents from Azure Portal");
            Console.WriteLine();
        }

        Console.WriteLine("========================================");
        Console.WriteLine("Configuration Mapping Suggestions");
        Console.WriteLine("========================================");
        Console.WriteLine();

        // Try to map agents to configuration keys based on naming (only valid ones)
        var validAgents = agentList.Where(a => a.Id.StartsWith("asst_")).ToList();
        var mappings = new Dictionary<string, string>
        {
            ["WriterAgentId"] = validAgents.FirstOrDefault(a => a.Name.Contains("writer", StringComparison.OrdinalIgnoreCase)).Id ?? "",
            ["JudgeAgentId"] = validAgents.FirstOrDefault(a => a.Name.Contains("judge", StringComparison.OrdinalIgnoreCase)).Id ?? "",
            ["RefinerAgentId"] = validAgents.FirstOrDefault(a => a.Name.Contains("refiner", StringComparison.OrdinalIgnoreCase)).Id ?? "",
            ["RubricSummaryAgentId"] = validAgents.FirstOrDefault(a => a.Name.Contains("rubric", StringComparison.OrdinalIgnoreCase)).Id ?? ""
        };

        Console.WriteLine("Update your appsettings.json with these agent IDs:");
        Console.WriteLine();
        Console.WriteLine("\"FoundryAgent\": {");
        foreach (var kvp in mappings.Where(m => !string.IsNullOrEmpty(m.Value)))
        {
            Console.WriteLine($"  \"{kvp.Key}\": \"{kvp.Value}\",");
        }
        Console.WriteLine("  ...");
        Console.WriteLine("}");
        Console.WriteLine();

        // Warn about missing mappings
        var missing = mappings.Where(m => string.IsNullOrEmpty(m.Value)).Select(m => m.Key).ToList();
        if (missing.Any())
        {
            Console.WriteLine("WARNING: Could not find agents for:");
            foreach (var key in missing)
            {
                Console.WriteLine($"  - {key}");
            }
            Console.WriteLine();
            Console.WriteLine("You may need to create these agents using:");
            Console.WriteLine($"  dotnet run create --endpoint {endpoint} --model <model>");
            Console.WriteLine();
        }

        return Task.CompletedTask;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"✗ Failed to list agents: {ex.Message}");
        Console.WriteLine();
        Console.WriteLine("Common issues:");
        Console.WriteLine("  1. Invalid Azure credentials (run 'az login')");
        Console.WriteLine("  2. Insufficient permissions on the Azure AI Project");
        Console.WriteLine("  3. Invalid endpoint URL");
        Console.WriteLine("  4. Network connectivity issues");
        Console.WriteLine();
        Environment.Exit(1);
        return Task.CompletedTask; // Unreachable, but required for compilation
    }
}

static async Task CreateAgentsAsync(string endpoint, string modelDeployment)
{
    Console.WriteLine("Connecting to Azure AI Foundry...");

    var projectClient = new AIProjectClient(new Uri(endpoint), new DefaultAzureCredential());
    var agentsClient = projectClient.GetPersistentAgentsClient();

    Console.WriteLine("✓ Connected successfully");
    Console.WriteLine();

    // Define the agents to create
    var agentDefinitions = new[]
    {
        new
        {
            Name = "mystira-writer-v01",
            Description = "Writer Agent - Generates initial story content based on prompts and age-appropriate guidelines",
            Instructions = @"You are an expert children's story writer for the Mystira Story Generator.

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

CRITICAL: Always return valid JSON matching the required schema. Do not include any text before or after the JSON.",
            ConfigKey = "WriterAgentId"
        },
        new
        {
            Name = "mystira-judge-v01",
            Description = "Judge Agent - Evaluates story quality against developmental rubrics and narrative principles",
            Instructions = @"You are an expert story evaluator for the Mystira Story Generator.

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
- overallStatus (""Pass"" or ""Fail"")
- findings (detailed feedback by category)
- recommendation (next steps)

CRITICAL: Always return valid JSON matching the evaluation report schema.",
            ConfigKey = "JudgeAgentId"
        },
        new
        {
            Name = "mystira-refiner-v01",
            Description = "Refiner Agent - Improves stories based on evaluation feedback while preserving core narrative",
            Instructions = @"You are an expert story refiner for the Mystira Story Generator.

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

CRITICAL: Always return valid JSON matching the story schema. Maintain the same schema structure as the input.",
            ConfigKey = "RefinerAgentId"
        },
        new
        {
            Name = "mystira-rubric-v01",
            Description = "Rubric Summary Agent - Generates evaluation summaries and rubric reports",
            Instructions = @"You are a rubric summary specialist for the Mystira Story Generator.

Your role is to create clear, actionable summaries of evaluation criteria and results.

When generating rubric summaries:
1. Use the file_search tool to retrieve evaluation rubrics
2. Organize criteria by category (safety, axes, development, narrative)
3. Explain scoring methodology and thresholds
4. Provide examples of what constitutes high vs. low scores
5. Summarize overall evaluation results in accessible language
6. Highlight strengths and areas for improvement

Return summaries as structured text or JSON as appropriate for the use case.

CRITICAL: Ensure summaries are clear, specific, and actionable for both developers and content reviewers.",
            ConfigKey = "RubricSummaryAgentId"
        }
    };

    Console.WriteLine($"Creating {agentDefinitions.Length} agents...");
    Console.WriteLine();

    var agentIds = new Dictionary<string, string>();

    foreach (var agentDef in agentDefinitions)
    {
        Console.Write($"Creating {agentDef.Name}...");

        try
        {
            // Create the agent
            var response = await agentsClient.CreateAgentAsync(
                model: modelDeployment,
                name: agentDef.Name,
                instructions: agentDef.Instructions,
                description: agentDef.Description,
                tools: new List<ToolDefinition>
                {
                    new FileSearchToolDefinition()
                }
            );

            var agent = response.Value;
            agentIds[agentDef.ConfigKey] = agent.Id;

            Console.WriteLine($" ✓ Created: {agent.Id}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($" ✗ Failed: {ex.Message}");
            Console.WriteLine();
            Console.WriteLine("ERROR: Failed to create agent. Common issues:");
            Console.WriteLine("  1. Invalid Azure credentials (run 'az login')");
            Console.WriteLine("  2. Insufficient permissions on the Azure AI Project");
            Console.WriteLine("  3. Model deployment not found");
            Console.WriteLine("  4. Network connectivity issues");
            Console.WriteLine();
            Environment.Exit(1);
        }
    }

    Console.WriteLine();
    Console.WriteLine("========================================");
    Console.WriteLine("Agent Creation Complete!");
    Console.WriteLine("========================================");
    Console.WriteLine();
    Console.WriteLine("Update your appsettings.json with these agent IDs:");
    Console.WriteLine();
    Console.WriteLine("\"FoundryAgent\": {");
    foreach (var kvp in agentIds)
    {
        Console.WriteLine($"  \"{kvp.Key}\": \"{kvp.Value}\",");
    }
    Console.WriteLine("  ...");
    Console.WriteLine("}");
    Console.WriteLine();
    Console.WriteLine("Agent IDs (for reference):");
    foreach (var kvp in agentIds)
    {
        Console.WriteLine($"  {kvp.Key,-25} = {kvp.Value}");
    }
    Console.WriteLine();
}
