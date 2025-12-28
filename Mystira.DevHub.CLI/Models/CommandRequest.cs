using System.Text.Json;
using System.Text.Json.Serialization;

namespace Mystira.DevHub.CLI.Models;

public class CommandRequest
{
    [JsonPropertyName("command")]
    public string Command { get; set; } = string.Empty;

    [JsonPropertyName("args")]
    public JsonElement Args { get; set; }
}

public class CosmosExportArgs
{
    [JsonPropertyName("outputPath")]
    public string OutputPath { get; set; } = string.Empty;
}

public class CosmosStatsArgs
{
    // No specific args needed for stats
}

public class MigrationArgs
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty; // scenarios, bundles, media-metadata, blobs, master-data, all

    [JsonPropertyName("sourceCosmosConnection")]
    public string? SourceCosmosConnection { get; set; }

    [JsonPropertyName("destCosmosConnection")]
    public string? DestCosmosConnection { get; set; }

    [JsonPropertyName("sourceStorageConnection")]
    public string? SourceStorageConnection { get; set; }

    [JsonPropertyName("destStorageConnection")]
    public string? DestStorageConnection { get; set; }

    [JsonPropertyName("sourceDatabaseName")]
    public string SourceDatabaseName { get; set; } = "MystiraDb";

    [JsonPropertyName("destDatabaseName")]
    public string DestDatabaseName { get; set; } = "MystiraAppDb";

    [JsonPropertyName("containerName")]
    public string ContainerName { get; set; } = "mystira-app-media";

    [JsonPropertyName("jsonFilesPath")]
    public string? JsonFilesPath { get; set; } // Path to master data JSON files

    [JsonPropertyName("dryRun")]
    public bool DryRun { get; set; } = false; // Preview mode - count items without migrating

    [JsonPropertyName("maxRetries")]
    public int MaxRetries { get; set; } = 3; // Max retries for failed items

    [JsonPropertyName("useBulkOperations")]
    public bool UseBulkOperations { get; set; } = true; // Use bulk upsert for better performance
}

public class InfrastructureArgs
{
    [JsonPropertyName("action")]
    public string Action { get; set; } = string.Empty; // validate, preview, deploy, destroy

    [JsonPropertyName("workflowFile")]
    public string WorkflowFile { get; set; } = "infrastructure-deploy-dev.yml";

    [JsonPropertyName("repository")]
    public string Repository { get; set; } = "phoenixvc/Mystira.App";

    [JsonPropertyName("confirm")]
    public bool Confirm { get; set; } = false; // For destroy operation
}
