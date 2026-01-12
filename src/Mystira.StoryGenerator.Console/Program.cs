using System;
using Azure.AI.Projects;
using Azure.Identity;

var endpoint = new Uri("https://mys-shared-ai-san.services.ai.azure.com/api/projects/mys-shared-ai-san-project");

AIProjectClient projectClient = new(endpoint, new DefaultAzureCredential());

// Newer SDK path (persistent agents)
var agents = projectClient.Agents.GetAgents();

// List agents
foreach (var a in agents)
{
    Console.WriteLine($"{a.Id} | {a.Name}");
}
