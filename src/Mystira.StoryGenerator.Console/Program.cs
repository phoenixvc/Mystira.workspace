using System;
using Azure.AI.Projects;
using Azure.Identity;

var endpoint = new Uri("https://mys-shared-ai-san.services.ai.azure.com/api/projects/mys-shared-ai-san-project");

AIProjectClient projectClient = new(endpoint, new DefaultAzureCredential());

// Newer SDK path (persistent agents)
var agents = projectClient.Agents.GetAgents();

Console.WriteLine("Agent List:");
Console.WriteLine("============================================");
Console.WriteLine();

// List agents
foreach (var a in agents)
{
    Console.WriteLine($"ID:      {a.Id}");
    Console.WriteLine($"Name:    {a.Name}");
    Console.WriteLine($"Valid:   {(a.Id.StartsWith("asst_") ? "✓ YES" : "✗ NO - Invalid ID format!")}");
    Console.WriteLine($"Created: {a.CreatedAt:yyyy-MM-dd HH:mm:ss}");
    Console.WriteLine();
}

Console.WriteLine("============================================");
Console.WriteLine("DIAGNOSIS:");
Console.WriteLine("============================================");
Console.WriteLine();

var invalidAgents = agents.Where(a => !a.Id.StartsWith("asst_")).ToList();
if (invalidAgents.Any())
{
    Console.WriteLine("⚠️  PROBLEM DETECTED:");
    Console.WriteLine();
    Console.WriteLine($"Found {invalidAgents.Count} agent(s) with INVALID ID format.");
    Console.WriteLine();
    Console.WriteLine("Azure AI Agents requires IDs in OpenAI format: asst_[random]");
    Console.WriteLine("Your agents have custom IDs, which the API rejects.");
    Console.WriteLine();
    Console.WriteLine("SOLUTION:");
    Console.WriteLine();
    Console.WriteLine("1. Delete the invalid agents (or leave them if you want to keep for reference)");
    Console.WriteLine("2. Create new agents using the AgentSetup tool:");
    Console.WriteLine();
    Console.WriteLine("   cd src/Mystira.StoryGenerator.AgentSetup");
    Console.WriteLine("   dotnet run create --endpoint https://mys-shared-ai-san.services.ai.azure.com/api/projects/mys-shared-ai-san-project --model gpt-4.1");
    Console.WriteLine();
    Console.WriteLine("3. Update appsettings.json with the new auto-generated IDs");
    Console.WriteLine();
}
else
{
    Console.WriteLine("✓ All agents have valid OpenAI-style IDs");
}
