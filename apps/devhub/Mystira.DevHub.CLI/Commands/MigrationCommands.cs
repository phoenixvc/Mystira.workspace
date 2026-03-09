using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Mystira.DevHub.CLI.Models;
using Mystira.DevHub.Services.Migration;

namespace Mystira.DevHub.CLI.Commands;

/// <summary>
/// CLI commands for Cosmos DB and Blob Storage migrations.
/// Uses generic/dynamic migrations to work without external domain dependencies.
/// </summary>
public class MigrationCommands
{
    private readonly IMigrationService _migrationService;
    private readonly IConfiguration _configuration;

    // Known containers with their partition key paths
    private static readonly Dictionary<string, string> ContainerPartitionKeys = new()
    {
        // Core content
        { "Scenarios", "/id" },
        { "ContentBundles", "/id" },
        { "MediaAssets", "/id" },

        // User data
        { "UserProfiles", "/Name" },
        { "Accounts", "/id" },
        { "UserBadges", "/id" },
        { "PendingSignups", "/id" },

        // Game data
        { "GameSessions", "/id" },
        { "CompassTrackings", "/id" },

        // Reference data
        { "CharacterMaps", "/id" },
        { "CharacterMapFiles", "/id" },
        { "CharacterMediaMetadataFiles", "/id" },
        { "AvatarConfigurationFiles", "/id" },
        { "BadgeConfigurations", "/id" },
    };

    public MigrationCommands(IMigrationService migrationService, IConfiguration configuration)
    {
        _migrationService = migrationService;
        _configuration = configuration;
    }

    public async Task<CommandResponse> RunAsync(JsonElement argsJson)
    {
        try
        {
            var args = JsonSerializer.Deserialize<MigrationArgs>(argsJson.GetRawText());
            if (args == null || string.IsNullOrEmpty(args.Type))
            {
                return CommandResponse.Fail("Type is required (scenarios, bundles, user-profiles, game-sessions, accounts, blobs, all, container:<name>)");
            }

            // Get connection strings from args, environment, or configuration
            var sourceCosmosConnection = args.SourceCosmosConnection
                ?? Environment.GetEnvironmentVariable("SOURCE_COSMOS_CONNECTION")
                ?? _configuration.GetConnectionString("SourceCosmosDb")
                ?? "";

            var destCosmosConnection = args.DestCosmosConnection
                ?? Environment.GetEnvironmentVariable("DEST_COSMOS_CONNECTION")
                ?? _configuration.GetConnectionString("DestCosmosDb")
                ?? _configuration.GetConnectionString("CosmosDb")
                ?? "";

            var sourceStorageConnection = args.SourceStorageConnection
                ?? Environment.GetEnvironmentVariable("SOURCE_STORAGE_CONNECTION")
                ?? _configuration.GetConnectionString("SourceStorage")
                ?? "";

            var destStorageConnection = args.DestStorageConnection
                ?? Environment.GetEnvironmentVariable("DEST_STORAGE_CONNECTION")
                ?? _configuration.GetConnectionString("DestStorage")
                ?? _configuration.GetConnectionString("AzureStorage")
                ?? "";

            var results = new List<MigrationResult>();

            // Helper to validate cosmos connections
            bool HasCosmosConnections() => !string.IsNullOrEmpty(sourceCosmosConnection) && !string.IsNullOrEmpty(destCosmosConnection);

            // Create migration options
            var migrationOptions = new MigrationOptions
            {
                DryRun = args.DryRun,
                MaxRetries = args.MaxRetries,
                UseBulkOperations = args.UseBulkOperations
            };

            // Helper to migrate a container by name
            async Task<MigrationResult> MigrateContainer(string containerName)
            {
                var partitionKey = ContainerPartitionKeys.GetValueOrDefault(containerName, "/id");
                return await _migrationService.MigrateContainerAsync(
                    sourceCosmosConnection,
                    destCosmosConnection,
                    args.SourceDatabaseName,
                    args.DestDatabaseName,
                    containerName,
                    partitionKey,
                    migrationOptions);
            }

            var migrationType = args.Type.ToLower();

            // Handle custom container migration (container:ContainerName)
            if (migrationType.StartsWith("container:"))
            {
                if (!HasCosmosConnections())
                    return CommandResponse.Fail("Source and destination Cosmos DB connection strings are required");

                var containerName = args.Type.Substring("container:".Length);
                results.Add(await MigrateContainer(containerName));
            }
            else
            {
                switch (migrationType)
                {
                    // Core content containers
                    case "scenarios":
                        if (!HasCosmosConnections())
                            return CommandResponse.Fail("Source and destination Cosmos DB connection strings are required");
                        results.Add(await MigrateContainer("Scenarios"));
                        break;

                    case "bundles":
                    case "content-bundles":
                        if (!HasCosmosConnections())
                            return CommandResponse.Fail("Source and destination Cosmos DB connection strings are required");
                        results.Add(await MigrateContainer("ContentBundles"));
                        break;

                    case "media-metadata":
                    case "media-assets":
                        if (!HasCosmosConnections())
                            return CommandResponse.Fail("Source and destination Cosmos DB connection strings are required");
                        results.Add(await MigrateContainer("MediaAssets"));
                        break;

                    // User data containers
                    case "user-profiles":
                        if (!HasCosmosConnections())
                            return CommandResponse.Fail("Source and destination Cosmos DB connection strings are required");
                        results.Add(await MigrateContainer("UserProfiles"));
                        break;

                    case "accounts":
                        if (!HasCosmosConnections())
                            return CommandResponse.Fail("Source and destination Cosmos DB connection strings are required");
                        results.Add(await MigrateContainer("Accounts"));
                        break;

                    case "game-sessions":
                        if (!HasCosmosConnections())
                            return CommandResponse.Fail("Source and destination Cosmos DB connection strings are required");
                        results.Add(await MigrateContainer("GameSessions"));
                        break;

                    case "compass-trackings":
                        if (!HasCosmosConnections())
                            return CommandResponse.Fail("Source and destination Cosmos DB connection strings are required");
                        results.Add(await MigrateContainer("CompassTrackings"));
                        break;

                    // Reference data containers
                    case "character-maps":
                        if (!HasCosmosConnections())
                            return CommandResponse.Fail("Source and destination Cosmos DB connection strings are required");
                        results.Add(await MigrateContainer("CharacterMaps"));
                        break;

                    case "character-map-files":
                        if (!HasCosmosConnections())
                            return CommandResponse.Fail("Source and destination Cosmos DB connection strings are required");
                        results.Add(await MigrateContainer("CharacterMapFiles"));
                        break;

                    case "character-media-metadata-files":
                        if (!HasCosmosConnections())
                            return CommandResponse.Fail("Source and destination Cosmos DB connection strings are required");
                        results.Add(await MigrateContainer("CharacterMediaMetadataFiles"));
                        break;

                    case "avatar-configuration-files":
                        if (!HasCosmosConnections())
                            return CommandResponse.Fail("Source and destination Cosmos DB connection strings are required");
                        results.Add(await MigrateContainer("AvatarConfigurationFiles"));
                        break;

                    case "badge-configurations":
                        if (!HasCosmosConnections())
                            return CommandResponse.Fail("Source and destination Cosmos DB connection strings are required");
                        results.Add(await MigrateContainer("BadgeConfigurations"));
                        break;

                    // Blob storage
                    case "blobs":
                        if (string.IsNullOrEmpty(sourceStorageConnection) || string.IsNullOrEmpty(destStorageConnection))
                            return CommandResponse.Fail("Source and destination storage connection strings are required");
                        results.Add(await _migrationService.MigrateBlobStorageAsync(
                            sourceStorageConnection,
                            destStorageConnection,
                            args.ContainerName,
                            migrationOptions));
                        break;

                    // Migrate all known containers
                    case "all":
                        if (!HasCosmosConnections())
                            return CommandResponse.Fail("Source and destination Cosmos DB connection strings are required");

                        // Migrate all known containers
                        foreach (var container in ContainerPartitionKeys)
                        {
                            results.Add(await _migrationService.MigrateContainerAsync(
                                sourceCosmosConnection,
                                destCosmosConnection,
                                args.SourceDatabaseName,
                                args.DestDatabaseName,
                                container.Key,
                                container.Value,
                                migrationOptions));
                        }

                        // Migrate Blob Storage if connections provided
                        if (!string.IsNullOrEmpty(sourceStorageConnection) && !string.IsNullOrEmpty(destStorageConnection))
                        {
                            results.Add(await _migrationService.MigrateBlobStorageAsync(
                                sourceStorageConnection,
                                destStorageConnection,
                                args.ContainerName,
                                migrationOptions));
                        }
                        break;

                    default:
                        var validTypes = string.Join(", ", new[]
                        {
                            "scenarios", "bundles", "media-metadata",
                            "user-profiles", "accounts", "game-sessions", "compass-trackings",
                            "character-maps", "character-map-files", "character-media-metadata-files",
                            "avatar-configuration-files", "badge-configurations",
                            "blobs", "all", "container:<name>"
                        });
                        return CommandResponse.Fail($"Unknown migration type: {args.Type}. Valid types: {validTypes}");
                }
            }

            var overallSuccess = results.All(r => r.Success);
            var totalItems = results.Sum(r => r.TotalItems);
            var totalSuccess = results.Sum(r => r.SuccessCount);
            var totalFailures = results.Sum(r => r.FailureCount);

            return CommandResponse.Ok(new
            {
                overallSuccess,
                totalItems,
                totalSuccess,
                totalFailures,
                results
            }, $"Migration completed: {totalSuccess}/{totalItems} successful");
        }
        catch (Exception ex)
        {
            return CommandResponse.Fail(ex.Message);
        }
    }
}
