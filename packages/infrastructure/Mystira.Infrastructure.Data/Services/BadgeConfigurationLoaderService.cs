using System.Text.Json;
using System.Text.Json.Serialization;
using Json.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mystira.Domain.Models;

namespace Mystira.Infrastructure.Data.Services;

/// <summary>
/// Service for loading and seeding badge configurations from JSON files into the database.
/// Validates badge configuration JSON against schema before seeding.
/// </summary>
public class BadgeConfigurationLoaderService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BadgeConfigurationLoaderService> _logger;
    private const long MaxImageSizeBytes = 5 * 1024 * 1024;

    // Cosmos EF Core provider may translate Any/Exists into an unsupported EXISTS query.
    // Use a TOP 1 style projection to check for existence safely.
    private static async Task<bool> HasAnyAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default)
    {
        return await query.Select(_ => 1).Take(1).FirstOrDefaultAsync(cancellationToken) == 1;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BadgeConfigurationLoaderService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for creating scoped contexts.</param>
    /// <param name="logger">The logger instance.</param>
    public BadgeConfigurationLoaderService(
        IServiceProvider serviceProvider,
        ILogger<BadgeConfigurationLoaderService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Loads badge configuration files for all age groups and seeds them into the database.
    /// Validates each configuration file against the JSON schema before seeding.
    /// Skips seeding if badge data already exists in the database.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task LoadAndSeedAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MystiraAppDbContext>();

        if (await HasAnyAsync(context.AxisAchievements) || await HasAnyAsync(context.Badges))
        {
            _logger.LogInformation("Badge configuration already seeded, skipping");
            return;
        }

        var schema = await LoadSchemaAsync();
        if (schema == null)
        {
            _logger.LogWarning("Badge configuration schema not found, skipping badge seeding");
            return;
        }

        var ageGroups = new[] { "1-2", "3-5", "6-9", "10-12", "13-18" };
        foreach (var ageGroup in ageGroups)
        {
            await LoadAndSeedBadgeConfigurationAsync(context, schema, ageGroup);
        }
    }

    private async Task<JsonSchema?> LoadSchemaAsync()
    {
        var schemaPath = GetSchemaFilePath();
        if (!File.Exists(schemaPath))
        {
            _logger.LogWarning("Schema file not found at {Path}", schemaPath);
            return null;
        }

        try
        {
            var schemaJson = await File.ReadAllTextAsync(schemaPath);
            return JsonSchema.FromText(schemaJson);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load badge configuration schema from {Path}", schemaPath);
            return null;
        }
    }

    private async Task LoadAndSeedBadgeConfigurationAsync(MystiraAppDbContext context, JsonSchema schema, string ageGroupId)
    {
        var configPath = GetBadgeConfigFilePath(ageGroupId);
        if (!File.Exists(configPath))
        {
            _logger.LogWarning("Badge configuration file not found for age group {AgeGroup} at {Path}", ageGroupId, configPath);
            return;
        }

        try
        {
            var json = await File.ReadAllTextAsync(configPath);
            var jsonDoc = JsonDocument.Parse(json);

            var validationResults = schema.Evaluate(jsonDoc.RootElement);
            if (!validationResults.IsValid)
            {
                _logger.LogError("Badge configuration for age group {AgeGroup} failed schema validation: {Errors}",
                    ageGroupId, string.Join(", ", validationResults.Errors?.Select(e => e.Value) ?? Array.Empty<string>()));
                return;
            }

            var config = JsonSerializer.Deserialize<BadgeConfigurationJson>(json, GetJsonOptions());
            if (config == null)
            {
                _logger.LogWarning("Failed to deserialize badge configuration for age group {AgeGroup}", ageGroupId);
                return;
            }

            await SeedBadgesAsync(context, config);

            _logger.LogInformation("Loaded badge configuration for age group {AgeGroup}: {BadgeCount} badges",
                ageGroupId, config.Badges.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to load badge configuration for age group {AgeGroup}", ageGroupId);
        }
    }

    // Note: SeedAxisAchievementsAsync was removed - AxisAchievement in the domain model
    // is for tracking user compass axis progress, not for storing achievement definitions.
    // Axis achievement definitions are now derived from badge configurations.

    private async Task SeedBadgesAsync(MystiraAppDbContext context, BadgeConfigurationJson config)
    {
        foreach (var badge in config.Badges)
        {
            var entity = new Badge
            {
                Id = GenerateDeterministicId("badge", config.AgeGroupId, badge.CompassAxisId, badge.Tier, badge.TierOrder.ToString()),
                AgeGroupId = config.AgeGroupId,
                CompassAxisId = badge.CompassAxisId,
                Tier = badge.Tier,
                TierOrder = badge.TierOrder,
                Name = badge.Title, // Title maps to Name in domain
                Description = badge.Description,
                RequiredScore = (int)badge.RequiredScore, // Cast from float to int
                ImageId = badge.ImageId
            };

            await context.Badges.AddAsync(entity);
        }

        await context.SaveChangesAsync();
    }

    private static string GetSchemaFilePath()
    {
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        var schemaDir = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", "..", "src", "Mystira.Domain", "Schemas"));

        var possiblePaths = new[]
        {
            Path.Combine(schemaDir, "badge-configuration.schema.json"),
            Path.Combine(currentDir, "Schemas", "badge-configuration.schema.json"),
            Path.Combine(currentDir, "badge-configuration.schema.json")
        };

        return possiblePaths.Select(Path.GetFullPath).FirstOrDefault(File.Exists)
               ?? Path.Combine(schemaDir, "badge-configuration.schema.json");
    }

    private static string GetBadgeConfigFilePath(string ageGroupId)
    {
        var currentDir = AppDomain.CurrentDomain.BaseDirectory;
        var badgesDir = Path.GetFullPath(Path.Combine(currentDir, "..", "..", "..", "..", "src", "Mystira.Domain", "Data", "Badges"));

        var possiblePaths = new[]
        {
            Path.Combine(badgesDir, $"{ageGroupId}.json"),
            Path.Combine(currentDir, "Data", "Badges", $"{ageGroupId}.json"),
            Path.Combine(currentDir, "Badges", $"{ageGroupId}.json")
        };

        return possiblePaths.Select(Path.GetFullPath).FirstOrDefault(File.Exists)
               ?? Path.Combine(badgesDir, $"{ageGroupId}.json");
    }

    private static JsonSerializerOptions GetJsonOptions()
    {
        return new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    private static string GenerateDeterministicId(string entityType, params string[] parts)
    {
        var input = $"{entityType}:{string.Join(":", parts.Select(p => p.ToLowerInvariant()))}";
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        return new Guid(guidBytes).ToString();
    }

    private class BadgeConfigurationJson
    {
        [JsonPropertyName("Age_Group_Id")]
        public string AgeGroupId { get; set; } = string.Empty;

        [JsonPropertyName("Axis_Achievements")]
        public List<AxisAchievementJson> AxisAchievements { get; set; } = new();

        [JsonPropertyName("Badges")]
        public List<BadgeJson> Badges { get; set; } = new();
    }

    private class AxisAchievementJson
    {
        [JsonPropertyName("Compass_Axis_Id")]
        public string CompassAxisId { get; set; } = string.Empty;

        [JsonPropertyName("Axes_Direction")]
        public string AxesDirection { get; set; } = string.Empty;

        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;
    }

    private class BadgeJson
    {
        [JsonPropertyName("Tier")]
        public string Tier { get; set; } = string.Empty;

        [JsonPropertyName("Tier_Order")]
        public int TierOrder { get; set; }

        [JsonPropertyName("Title")]
        public string Title { get; set; } = string.Empty;

        [JsonPropertyName("Description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("Required_Score")]
        public float RequiredScore { get; set; }

        [JsonPropertyName("Compass_Axis_Id")]
        public string CompassAxisId { get; set; } = string.Empty;

        [JsonPropertyName("Image_Id")]
        public string ImageId { get; set; } = string.Empty;
    }
}
