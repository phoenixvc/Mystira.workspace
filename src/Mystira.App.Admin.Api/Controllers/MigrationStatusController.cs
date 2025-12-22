using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Mystira.App.Admin.Api.Configuration;

namespace Mystira.App.Admin.Api.Controllers;

/// <summary>
/// Controller for monitoring data migration status.
/// Provides endpoints for tracking the hybrid data strategy migration phases.
/// See: docs/planning/hybrid-data-strategy-roadmap.md
/// </summary>
[ApiController]
[Route("api/admin/migration")]
[Authorize(Policy = "AdminOnly")]
public class MigrationStatusController : ControllerBase
{
    private readonly AdminDataMigrationOptions _migrationOptions;
    private readonly IConfiguration _configuration;
    private readonly ILogger<MigrationStatusController> _logger;

    public MigrationStatusController(
        IOptions<AdminDataMigrationOptions> migrationOptions,
        IConfiguration configuration,
        ILogger<MigrationStatusController> logger)
    {
        _migrationOptions = migrationOptions.Value;
        _configuration = configuration;
        _logger = logger;
    }

    /// <summary>
    /// Get the current migration status and configuration.
    /// </summary>
    /// <returns>Current migration phase and configuration details.</returns>
    [HttpGet("status")]
    [ProducesResponseType(typeof(MigrationStatusResponse), StatusCodes.Status200OK)]
    public IActionResult GetMigrationStatus()
    {
        var cosmosConfigured = !string.IsNullOrEmpty(_configuration.GetConnectionString("CosmosDb"));
        var postgresConfigured = !string.IsNullOrEmpty(_configuration.GetConnectionString("PostgreSQL"));
        var redisConfigured = !string.IsNullOrEmpty(_configuration.GetConnectionString("Redis"));

        var response = new MigrationStatusResponse
        {
            CurrentPhase = _migrationOptions.Phase,
            PhaseName = GetPhaseName(_migrationOptions.Phase),
            PhaseDescription = GetPhaseDescription(_migrationOptions.Phase),
            MigrationEnabled = _migrationOptions.Enabled,
            ReadOnlyPostgresAccess = _migrationOptions.ReadOnlyPostgresAccess,
            ContentDualWriteEnabled = _migrationOptions.EnableContentDualWrite,
            ContentCachingEnabled = _migrationOptions.EnableContentCaching,
            Infrastructure = new InfrastructureStatus
            {
                CosmosDbConfigured = cosmosConfigured,
                PostgreSqlConfigured = postgresConfigured,
                RedisConfigured = redisConfigured
            },
            NextPhase = GetNextPhaseInfo(_migrationOptions.Phase),
            Timestamp = DateTimeOffset.UtcNow
        };

        _logger.LogInformation("Migration status requested. Current phase: {Phase}", _migrationOptions.Phase);

        return Ok(response);
    }

    /// <summary>
    /// Get migration phase recommendations based on current configuration.
    /// </summary>
    [HttpGet("recommendations")]
    [ProducesResponseType(typeof(MigrationRecommendations), StatusCodes.Status200OK)]
    public IActionResult GetRecommendations()
    {
        var recommendations = new List<string>();
        var warnings = new List<string>();
        var readyForNextPhase = true;

        var cosmosConfigured = !string.IsNullOrEmpty(_configuration.GetConnectionString("CosmosDb"));
        var postgresConfigured = !string.IsNullOrEmpty(_configuration.GetConnectionString("PostgreSQL"));
        var redisConfigured = !string.IsNullOrEmpty(_configuration.GetConnectionString("Redis"));

        switch (_migrationOptions.Phase)
        {
            case MigrationPhase.CosmosOnly:
                if (!postgresConfigured)
                {
                    recommendations.Add("Configure PostgreSQL connection string to prepare for Phase 1");
                    readyForNextPhase = false;
                }
                if (!redisConfigured)
                {
                    recommendations.Add("Configure Redis connection string for content caching");
                }
                if (postgresConfigured && redisConfigured)
                {
                    recommendations.Add("Ready to enable Phase 1 (DualWriteCosmosRead). Set DataMigration:Phase=1 and DataMigration:Enabled=true");
                }
                break;

            case MigrationPhase.DualWriteCosmosRead:
                recommendations.Add("Monitor dual-write sync success rate before proceeding to Phase 2");
                recommendations.Add("Verify data consistency between Cosmos DB and PostgreSQL");
                if (!_migrationOptions.EnableContentDualWrite)
                {
                    warnings.Add("ContentDualWrite is disabled. Enable to sync content to PostgreSQL.");
                }
                break;

            case MigrationPhase.DualWritePostgresRead:
                recommendations.Add("Monitor PostgreSQL read performance");
                recommendations.Add("Verify all queries return correct data from PostgreSQL");
                recommendations.Add("Run reconciliation reports to ensure data consistency");
                break;

            case MigrationPhase.PostgresOnly:
                if (cosmosConfigured)
                {
                    recommendations.Add("Consider archiving Cosmos DB data and removing connection");
                }
                recommendations.Add("Migration complete! Monitor PostgreSQL performance.");
                break;
        }

        if (!_migrationOptions.Enabled && _migrationOptions.Phase != MigrationPhase.CosmosOnly)
        {
            warnings.Add("Migration is disabled but phase is not CosmosOnly. This may cause unexpected behavior.");
        }

        return Ok(new MigrationRecommendations
        {
            Recommendations = recommendations,
            Warnings = warnings,
            ReadyForNextPhase = readyForNextPhase
        });
    }

    private static string GetPhaseName(MigrationPhase phase) => phase switch
    {
        MigrationPhase.CosmosOnly => "Phase 0: Cosmos DB Only",
        MigrationPhase.DualWriteCosmosRead => "Phase 1: Dual-Write (Cosmos Read)",
        MigrationPhase.DualWritePostgresRead => "Phase 2: Dual-Write (PostgreSQL Read)",
        MigrationPhase.PostgresOnly => "Phase 3: PostgreSQL Only",
        _ => "Unknown"
    };

    private static string GetPhaseDescription(MigrationPhase phase) => phase switch
    {
        MigrationPhase.CosmosOnly => "All reads and writes go to Cosmos DB. This is the initial state.",
        MigrationPhase.DualWriteCosmosRead => "Writes go to both Cosmos DB and PostgreSQL. Reads come from Cosmos DB.",
        MigrationPhase.DualWritePostgresRead => "Writes go to both databases. Reads come from PostgreSQL.",
        MigrationPhase.PostgresOnly => "All reads and writes go to PostgreSQL. Cosmos DB is archived.",
        _ => "Unknown phase"
    };

    private static NextPhaseInfo? GetNextPhaseInfo(MigrationPhase currentPhase) => currentPhase switch
    {
        MigrationPhase.CosmosOnly => new NextPhaseInfo
        {
            Phase = MigrationPhase.DualWriteCosmosRead,
            Name = "Phase 1: Dual-Write (Cosmos Read)",
            Prerequisites = new[]
            {
                "PostgreSQL connection configured",
                "Redis connection configured",
                "EF Core migrations applied to PostgreSQL"
            }
        },
        MigrationPhase.DualWriteCosmosRead => new NextPhaseInfo
        {
            Phase = MigrationPhase.DualWritePostgresRead,
            Name = "Phase 2: Dual-Write (PostgreSQL Read)",
            Prerequisites = new[]
            {
                "Data sync validation complete",
                "All data reconciled between Cosmos DB and PostgreSQL",
                "PostgreSQL query performance validated"
            }
        },
        MigrationPhase.DualWritePostgresRead => new NextPhaseInfo
        {
            Phase = MigrationPhase.PostgresOnly,
            Name = "Phase 3: PostgreSQL Only",
            Prerequisites = new[]
            {
                "Extended production validation period",
                "Cosmos DB data archived",
                "Rollback plan tested"
            }
        },
        MigrationPhase.PostgresOnly => null,
        _ => null
    };
}

public record MigrationStatusResponse
{
    public MigrationPhase CurrentPhase { get; init; }
    public string PhaseName { get; init; } = string.Empty;
    public string PhaseDescription { get; init; } = string.Empty;
    public bool MigrationEnabled { get; init; }
    public bool ReadOnlyPostgresAccess { get; init; }
    public bool ContentDualWriteEnabled { get; init; }
    public bool ContentCachingEnabled { get; init; }
    public InfrastructureStatus Infrastructure { get; init; } = new();
    public NextPhaseInfo? NextPhase { get; init; }
    public DateTimeOffset Timestamp { get; init; }
}

public record InfrastructureStatus
{
    public bool CosmosDbConfigured { get; init; }
    public bool PostgreSqlConfigured { get; init; }
    public bool RedisConfigured { get; init; }
}

public record NextPhaseInfo
{
    public MigrationPhase Phase { get; init; }
    public string Name { get; init; } = string.Empty;
    public string[] Prerequisites { get; init; } = Array.Empty<string>();
}

public record MigrationRecommendations
{
    public List<string> Recommendations { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
    public bool ReadyForNextPhase { get; init; }
}
