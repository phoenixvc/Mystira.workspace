param(
    [string]$Configuration = "Debug"
)

$solutionPath = Join-Path $PSScriptRoot "Mystira.StoryGenerator.sln"

dotnet restore $solutionPath
dotnet build $solutionPath -c $Configuration
