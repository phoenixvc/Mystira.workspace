# Example script to run the RAG indexer with sample data
# Note: You must first configure your Azure credentials in appsettings.json

Write-Host "Mystira RAG Indexer Example" -ForegroundColor Green
Write-Host "============================" -ForegroundColor Green
Write-Host ""

# Check if sample data exists
if (-not (Test-Path "./data/sample-instructions.json")) {
    Write-Host "Error: Sample data file not found at ./data/sample-instructions.json" -ForegroundColor Red
    exit 1
}

# Check if configuration exists
if (-not (Test-Path "./appsettings.json")) {
    Write-Host "Error: Configuration file not found at ./appsettings.json" -ForegroundColor Red
    Write-Host "Please configure your Azure AI Search and OpenAI credentials first." -ForegroundColor Yellow
    exit 1
}

Write-Host "Running RAG indexer with sample data..." -ForegroundColor Cyan
Write-Host "Command: dotnet run -- ./data/sample-instructions.json" -ForegroundColor Cyan
Write-Host ""

dotnet run -- ./data/sample-instructions.json

Write-Host ""
Write-Host "Example completed." -ForegroundColor Green
Write-Host "Make sure to update appsettings.json with your actual Azure credentials before production use." -ForegroundColor Yellow