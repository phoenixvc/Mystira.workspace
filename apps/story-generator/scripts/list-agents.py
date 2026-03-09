#!/usr/bin/env python3
"""
Lists all Azure AI Foundry agents and their IDs.
This script helps you find the correct assistant IDs to use in appsettings.json.
"""

import sys
import json
from azure.identity import DefaultAzureCredential
from azure.ai.projects import AIProjectClient
from azure.ai.projects.models import AgentsApiResponseFormat

def list_agents(endpoint: str):
    """List all agents in the Azure AI Project."""

    print("Connecting to Azure AI Foundry...")

    try:
        credential = DefaultAzureCredential()
        project_client = AIProjectClient(endpoint=endpoint, credential=credential)
        agents_client = project_client.agents

        print("✓ Connected successfully\n")
        print("Retrieving agents...\n")

        # List all agents
        agents_list = agents_client.list_agents()

        if not agents_list.data:
            print("No agents found.")
            print("\nRun the AgentSetup tool to create agents:")
            print(f"  cd src/Mystira.StoryGenerator.AgentSetup")
            print(f"  dotnet run create --endpoint {endpoint} --model <model-name>")
            return

        print(f"Found {len(agents_list.data)} agent(s):\n")
        print(f"{'ID':<35} {'Name':<35} {'Created':<25}")
        print("-" * 100)

        agents_data = sorted(agents_list.data, key=lambda a: a.name or "")

        for agent in agents_data:
            created_at = agent.created_at.strftime("%Y-%m-%d %H:%M:%S") if agent.created_at else "N/A"
            print(f"{agent.id:<35} {agent.name or 'Unnamed':<35} {created_at:<25}")

        print("\n" + "=" * 100)
        print("Configuration Mapping Suggestions")
        print("=" * 100 + "\n")

        # Try to map agents to configuration keys based on naming
        mappings = {
            "WriterAgentId": next((a.id for a in agents_data if "writer" in (a.name or "").lower()), ""),
            "JudgeAgentId": next((a.id for a in agents_data if "judge" in (a.name or "").lower()), ""),
            "RefinerAgentId": next((a.id for a in agents_data if "refiner" in (a.name or "").lower()), ""),
            "RubricSummaryAgentId": next((a.id for a in agents_data if "rubric" in (a.name or "").lower()), "")
        }

        print("Update your appsettings.json with these agent IDs:\n")
        print('"FoundryAgent": {')
        for key, value in mappings.items():
            if value:
                print(f'  "{key}": "{value}",')
        print('  ...')
        print('}\n')

        # Warn about missing mappings
        missing = [k for k, v in mappings.items() if not v]
        if missing:
            print("WARNING: Could not find agents for:")
            for key in missing:
                print(f"  - {key}")
            print("\nYou may need to create these agents.")
            print()

    except Exception as e:
        print(f"✗ Failed to list agents: {e}")
        print("\nCommon issues:")
        print("  1. Invalid Azure credentials (run 'az login')")
        print("  2. Insufficient permissions on the Azure AI Project")
        print("  3. Invalid endpoint URL")
        print("  4. Network connectivity issues")
        sys.exit(1)

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python list-agents.py <endpoint-url>")
        print("\nExample:")
        print("  python list-agents.py https://mys-shared-ai-san.services.ai.azure.com/api/projects/mys-shared-ai-san-project")
        sys.exit(1)

    endpoint = sys.argv[1]
    list_agents(endpoint)
