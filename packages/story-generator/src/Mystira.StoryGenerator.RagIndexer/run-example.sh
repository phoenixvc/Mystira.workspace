#!/bin/bash

# Example script to run the RAG indexer with sample data
# Note: You must first configure your Azure credentials in appsettings.json

echo "Mystira RAG Indexer Example"
echo "============================"
echo

# Check if sample data exists
if [ ! -f "./data/sample-instructions.json" ]; then
    echo "Error: Sample data file not found at ./data/sample-instructions.json"
    exit 1
fi

# Check if configuration exists
if [ ! -f "./appsettings.json" ]; then
    echo "Error: Configuration file not found at ./appsettings.json"
    echo "Please configure your Azure AI Search and OpenAI credentials first."
    exit 1
fi

echo "Running RAG indexer with sample data..."
echo "Command: dotnet run -- ./data/sample-instructions.json"
echo

dotnet run -- ./data/sample-instructions.json

echo
echo "Example completed."
echo "Make sure to update appsettings.json with your actual Azure credentials before production use."