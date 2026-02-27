// Program.cs
//
// Mystira.StoryGenerator.AgentSetup — Persistent Agents (Azure.AI.Agents.Persistent)
//
// This tool creates/updates/list/deletes *Persistent* agents (the ones visible via:
//   projectClient.GetPersistentAgentsClient().Administration.GetAgents()
//
// Packages you should have in the .csproj:
//   - Azure.AI.Projects
//   - Azure.AI.Agents.Persistent
//   - Azure.Identity
//
// Docs (method signatures):
//   PersistentAgentsAdministrationClient.CreateAgent / UpdateAgent / DeleteAgent
//   https://learn.microsoft.com/en-us/dotnet/api/azure.ai.agents.persistent.persistentagentsadministrationclient.createagent
//   https://learn.microsoft.com/en-us/dotnet/api/azure.ai.agents.persistent.persistentagentsadministrationclient.updateagent
//   https://learn.microsoft.com/en-us/dotnet/api/azure.ai.agents.persistent.persistentagentsadministrationclient.deleteagent
//

using Azure;
using Azure.AI.Agents.Persistent;
using Azure.AI.Projects;
using Azure.Identity;
using Mystira.StoryGenerator.AgentSetup;

// ----------------------
// Entrypoint and argument parsing
// ----------------------

if (args.Length == 0 || args[0] is "-h" or "--help")
{
    PrintUsage();
    return 0;
}

var command = args[0].ToLowerInvariant();
var options = ParseOptions(args.Skip(1).ToArray());

switch (command)
{
    case "list":
        return HandleList(options);
    case "create":
        return HandleCreate(options);
    case "update":
        return HandleUpdate(options);
    case "delete":
        return HandleDelete(options);
    default:
        Console.WriteLine($"Unknown command: {command}");
        Console.WriteLine();
        PrintUsage();
        return 1;
}

// ----------------------
// Helpers
// ----------------------

static PersistentAgentsClient CreatePersistentClient(string projectEndpoint)
{
    // You can swap to AzureCliCredential if you prefer:
    // var cred = new AzureCliCredential();
    var cred = new DefaultAzureCredential();

    // AIProjectClient gives you the authenticated PersistentAgentsClient for that project endpoint.
    var projectClient = new AIProjectClient(new Uri(projectEndpoint), cred);
    return projectClient.GetPersistentAgentsClient();
}

static PersistentAgent? ResolveAgent(PersistentAgentsClient client, string? id, string? name)
{
    if (!string.IsNullOrWhiteSpace(id))
    {
        try
        {
            return client.Administration.GetAgent(id).Value;
        }
        catch
        {
            return null;
        }
    }

    if (!string.IsNullOrWhiteSpace(name))
    {
        // Exact match first, then case-insensitive match
        PersistentAgent? exact = null;
        PersistentAgent? ci = null;

        foreach (var a in client.Administration.GetAgents())
        {
            if (a.Name == name) exact ??= a;
            if (string.Equals(a.Name, name, StringComparison.OrdinalIgnoreCase)) ci ??= a;
        }

        return exact ?? ci;
    }

    return null;
}

static Dictionary<string, string?> ParseOptions(string[] args)
{
    var result = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);

    string? currentKey = null;
    for (var i = 0; i < args.Length; i++)
    {
        var token = args[i];
        if (token.StartsWith("--", StringComparison.Ordinal))
        {
            currentKey = token;

            // Default bool flags to "true" if no explicit value follows.
            if (i + 1 >= args.Length || args[i + 1].StartsWith("--", StringComparison.Ordinal))
            {
                result[currentKey] = "true";
                currentKey = null;
            }
        }
        else if (currentKey != null)
        {
            result[currentKey] = token;
            currentKey = null;
        }
    }

    return result;
}

static string? GetOption(Dictionary<string, string?> options, string name)
{
    options.TryGetValue(name, out var value);
    return value;
}

static bool GetBoolOption(Dictionary<string, string?> options, string name)
{
    if (!options.TryGetValue(name, out var value) || value is null)
    {
        return false;
    }

    if (bool.TryParse(value, out var parsed))
    {
        return parsed;
    }

    return true;
}

static int HandleList(Dictionary<string, string?> options)
{
    var endpoint = GetOption(options, "--endpoint");
    if (string.IsNullOrWhiteSpace(endpoint))
    {
        Console.WriteLine("ERROR: --endpoint is required.");
        return 1;
    }

    var client = CreatePersistentClient(endpoint);

    Console.WriteLine("Persistent Agents (Administration.GetAgents):");
    Console.WriteLine("====================================================");

    int count = 0;
    foreach (var a in client.Administration.GetAgents())
    {
        count++;
        Console.WriteLine($"Id:          {a.Id}");
        Console.WriteLine($"Name:        {a.Name}");
        Console.WriteLine($"Model:       {a.Model}");
        Console.WriteLine($"Description: {a.Description}");
        Console.WriteLine("----------------------------------------------------");
    }

    if (count == 0)
    {
        Console.WriteLine("(none found)");
        Console.WriteLine();
        Console.WriteLine("If you see agents in the portal but none here, those were created in the *classic* agent plane.");
        Console.WriteLine("This tool creates *persistent* agents, which are separate.");
    }

    return 0;
}

static int HandleCreate(Dictionary<string, string?> options)
{
    var endpoint = GetOption(options, "--endpoint");
    var model = GetOption(options, "--model");

    if (string.IsNullOrWhiteSpace(endpoint))
    {
        Console.WriteLine("ERROR: --endpoint is required for create.");
        return 1;
    }

    if (string.IsNullOrWhiteSpace(model))
    {
        Console.WriteLine("ERROR: --model is required for create.");
        return 1;
    }

    var client = CreatePersistentClient(endpoint);

    Console.WriteLine("Connecting to Azure AI Foundry (Persistent Agents)...");
    Console.WriteLine("✓ Connected successfully");
    Console.WriteLine();
    Console.WriteLine("Creating 4 persistent agents...");
    Console.WriteLine();

    var defs = new[]
    {
        new AgentDef(
            Name: "mystira-writer-v01",
            Description: "Writer Agent - Generates initial story content based on prompts and age-appropriate guidelines",
            Instructions:
@"You are the Mystira Story Writer Agent.

Generate the initial Mystira story JSON according to the provided schema and rules retrieved via RAG.
- Always keep content age-appropriate for the target audience.
- Always return valid JSON ONLY (no preamble, no markdown)."
        ),
        new AgentDef(
            Name: "mystira-judge-v01",
            Description: "Judge Agent - Evaluates story quality against safety, development, and logic requirements",
            Instructions:
@"You are the Mystira Story Judge Agent.

Evaluate a story JSON against the Mystira requirements retrieved via RAG.
Return a structured JSON report ONLY (no preamble, no markdown)."
        ),
        new AgentDef(
            Name: "mystira-refiner-v01",
            Description: "Refiner Agent - Improves stories based on judge feedback while preserving core narrative",
            Instructions:
@"You are the Mystira Story Refiner Agent.

Given (1) story JSON and (2) feedback, produce a refined story JSON that fixes issues while preserving intent.
Return valid JSON ONLY (no preamble, no markdown)."
        ),
        new AgentDef(
            Name: "mystira-rubric-v01",
            Description: "Rubric Agent - Produces detailed scoring rubric/summary for a story",
            Instructions:
@"You are the Mystira Rubric Generator Agent.

Generate scoring rubric outputs as required by Mystira (from RAG guidance).
Return the required structured output ONLY (JSON if requested)."
        ),
    };

    var createdIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    foreach (var d in defs)
    {
        Console.Write($"Creating {d.Name}... ");
        try
        {
            // Persistent agent creation — NOTE: ID may be human-readable; do NOT assume asst_*.
            Response<PersistentAgent> resp = client.Administration.CreateAgent(
                model: model!,
                name: d.Name,
                description: d.Description,
                instructions: d.Instructions
            );

            var agent = resp.Value;
            Console.WriteLine($"✓ Created: {agent.Id}");
            createdIds[d.Name] = agent.Id;
        }
        catch (Exception ex)
        {
            Console.WriteLine("✗ Failed");
            Console.WriteLine(ex);
            return 1;
        }
    }

    Console.WriteLine();
    Console.WriteLine("========================================");
    Console.WriteLine("Agent Creation Complete!");
    Console.WriteLine("Update your appsettings.json with these agent IDs:");
    Console.WriteLine();

    // Map to your config keys
    // (Change keys as needed to match your appsettings schema)
    Console.WriteLine("\"FoundryAgent\": {");
    Console.WriteLine($"  \"WriterAgentId\": \"{createdIds["mystira-writer-v01"]}\",");
    Console.WriteLine($"  \"JudgeAgentId\": \"{createdIds["mystira-judge-v01"]}\",");
    Console.WriteLine($"  \"RefinerAgentId\": \"{createdIds["mystira-refiner-v01"]}\",");
    Console.WriteLine($"  \"RubricSummaryAgentId\": \"{createdIds["mystira-rubric-v01"]}\",");
    Console.WriteLine("  ...");
    Console.WriteLine("}");

    return 0;
}

static int HandleUpdate(Dictionary<string, string?> options)
{
    var endpoint = GetOption(options, "--endpoint");
    var id = GetOption(options, "--id");
    var name = GetOption(options, "--name");
    var model = GetOption(options, "--model");
    var instructions = GetOption(options, "--instructions");
    var description = GetOption(options, "--description");

    if (string.IsNullOrWhiteSpace(endpoint))
    {
        Console.WriteLine("ERROR: --endpoint is required for update.");
        return 1;
    }

    var client = CreatePersistentClient(endpoint);

    var agent = ResolveAgent(client, id, name);
    if (agent is null)
    {
        Console.WriteLine("ERROR: Agent not found. Provide --id or a --name that exists in Administration.GetAgents().");
        return 1;
    }

    Console.WriteLine($"Updating agent: {agent.Id} ({agent.Name})");
    Console.WriteLine("----------------------------------------------------");

    // Only update fields you passed. Anything null means "leave unchanged".
    var newModel = string.IsNullOrWhiteSpace(model) ? default : model;
    var newName = default(string);
    var newDescription = string.IsNullOrWhiteSpace(description) ? default : description;
    var newInstructions = string.IsNullOrWhiteSpace(instructions) ? default : instructions;

    try
    {
        Response<PersistentAgent> updated = client.Administration.UpdateAgent(
            assistantId: agent.Id,
            model: newModel,
            name: newName,
            description: newDescription,
            instructions: newInstructions
        );

        Console.WriteLine("✓ Updated");
        Console.WriteLine($"Id:          {updated.Value.Id}");
        Console.WriteLine($"Name:        {updated.Value.Name}");
        Console.WriteLine($"Model:       {updated.Value.Model}");
        Console.WriteLine($"Description: {updated.Value.Description}");
    }
    catch (Exception ex)
    {
        Console.WriteLine("✗ Update failed");
        Console.WriteLine(ex);
        return 1;
    }

    return 0;
}

static int HandleDelete(Dictionary<string, string?> options)
{
    var endpoint = GetOption(options, "--endpoint");
    var id = GetOption(options, "--id");
    var name = GetOption(options, "--name");
    var force = GetBoolOption(options, "--force");

    if (string.IsNullOrWhiteSpace(endpoint))
    {
        Console.WriteLine("ERROR: --endpoint is required for delete.");
        return 1;
    }

    var client = CreatePersistentClient(endpoint);

    var agent = ResolveAgent(client, id, name);
    if (agent is null)
    {
        Console.WriteLine("ERROR: Agent not found. Provide --id or a --name that exists in Administration.GetAgents().");
        return 1;
    }

    if (!force)
    {
        Console.Write($"Delete agent {agent.Id} ({agent.Name})? Type 'yes' to confirm: ");
        var confirm = Console.ReadLine();
        if (!string.Equals(confirm, "yes", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Cancelled.");
            return 0;
        }
    }

    try
    {
        Response<bool> resp = client.Administration.DeleteAgent(agent.Id);
        Console.WriteLine(resp.Value ? "✓ Deleted" : "✗ Delete returned false");
    }
    catch (Exception ex)
    {
        Console.WriteLine("✗ Delete failed");
        Console.WriteLine(ex);
        return 1;
    }

    return 0;
}

static void PrintUsage()
{
    Console.WriteLine("Mystira Foundry AgentSetup (Persistent Agents)");
    Console.WriteLine();
    Console.WriteLine("Usage:");
    Console.WriteLine("  agent-setup list   --endpoint <url>");
    Console.WriteLine("  agent-setup create --endpoint <url> --model <deployment-name>");
    Console.WriteLine("  agent-setup update --endpoint <url> [--id <id> | --name <name>] [--model <model>] [--instructions <text>] [--description <text>]");
    Console.WriteLine("  agent-setup delete --endpoint <url> [--id <id> | --name <name>] [--force]");
}

namespace Mystira.StoryGenerator.AgentSetup
{
    internal readonly record struct AgentDef(string Name, string Description, string Instructions);
}
