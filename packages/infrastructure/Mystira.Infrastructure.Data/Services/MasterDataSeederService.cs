using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mystira.Domain.Models;
using Mystira.Domain.ValueObjects;

namespace Mystira.Infrastructure.Data.Services;

/// <summary>
/// Service for seeding master data (CompassAxes, Archetypes, EchoTypes, FantasyThemes, AgeGroups)
/// from domain value objects into the database.
/// </summary>
public class MasterDataSeederService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<MasterDataSeederService> _logger;

    // Cosmos EF Core provider may translate Any/Exists into an unsupported EXISTS query.
    // Avoid constant projection + FirstOrDefault which can cause LIMIT/OFFSET in a subquery for Cosmos.
    // Instead materialize up to 1 entity and check if any returned.
    private static async Task<bool> HasAnyAsync<T>(IQueryable<T> query, CancellationToken cancellationToken = default) where T : class
    {
        var list = await query.AsNoTracking().Take(1).ToListAsync(cancellationToken);
        return list.Count > 0;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="MasterDataSeederService"/> class.
    /// </summary>
    /// <param name="serviceProvider">The service provider for creating scoped contexts.</param>
    /// <param name="logger">The logger instance.</param>
    public MasterDataSeederService(
        IServiceProvider serviceProvider,
        ILogger<MasterDataSeederService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Seeds all master data entities (CompassAxes, Archetypes, EchoTypes, FantasyThemes, AgeGroups)
    /// from domain value objects into the database. Skips seeding for entities that already exist.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task SeedAllAsync()
    {
        using var scope = _serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MystiraAppDbContext>();

        await SeedCompassAxesAsync(context);
        await SeedArchetypesAsync(context);
        await SeedEchoTypesAsync(context);
        await SeedFantasyThemesAsync(context);
        await SeedAgeGroupsAsync(context);
    }

    private async Task SeedCompassAxesAsync(MystiraAppDbContext context)
    {
        if (await HasAnyAsync(context.CompassAxes))
        {
            _logger.LogInformation("CompassAxes already seeded, skipping");
            return;
        }

        var coreAxes = new[]
        {
            CoreAxis.Courage,
            CoreAxis.Kindness,
            CoreAxis.Honesty,
            CoreAxis.Loyalty,
            CoreAxis.Justice,
            CoreAxis.Wisdom,
            CoreAxis.Compassion,
            CoreAxis.Humility
        };

        var entities = coreAxes.Select(axis => new CompassAxisDefinition
        {
            Id = GenerateDeterministicId("compass-axis", axis.Value),
            Name = axis.DisplayName,
            Description = axis.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await context.CompassAxes.AddRangeAsync(entities);
        await context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} compass axes from value objects", entities.Count);
    }

    private async Task SeedArchetypesAsync(MystiraAppDbContext context)
    {
        if (await HasAnyAsync(context.ArchetypeDefinitions))
        {
            _logger.LogInformation("Archetypes already seeded, skipping");
            return;
        }

        var archetypes = new[]
        {
            Archetype.Hero,
            Archetype.Sage,
            Archetype.Explorer,
            Archetype.Rebel,
            Archetype.Magician,
            Archetype.Innocent,
            Archetype.Caregiver,
            Archetype.Creator,
            Archetype.Ruler,
            Archetype.Jester,
            Archetype.Everyperson,
            Archetype.Lover
        };

        var entities = archetypes.Select(archetype => new ArchetypeDefinition
        {
            Id = GenerateDeterministicId("archetype", archetype.Value),
            Name = archetype.DisplayName,
            Description = $"The {archetype.DisplayName} archetype",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await context.ArchetypeDefinitions.AddRangeAsync(entities);
        await context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} archetypes from value objects", entities.Count);
    }

    private async Task SeedEchoTypesAsync(MystiraAppDbContext context)
    {
        if (await HasAnyAsync(context.EchoTypeDefinitions))
        {
            _logger.LogInformation("EchoTypes already seeded, skipping");
            return;
        }

        var echoTypes = new[]
        {
            EchoType.Memory,
            EchoType.Vision,
            EchoType.Secret,
            EchoType.Emotion,
            EchoType.Connection,
            EchoType.Warning,
            EchoType.Legacy,
            EchoType.Revelation
        };

        var entities = echoTypes.Select(echoType => new EchoTypeDefinition
        {
            Id = GenerateDeterministicId("echo-type", echoType.Value),
            Name = echoType.DisplayName,
            Description = echoType.Description,
            Category = echoType.Category,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await context.EchoTypeDefinitions.AddRangeAsync(entities);
        await context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} echo types from value objects", entities.Count);
    }

    private async Task SeedFantasyThemesAsync(MystiraAppDbContext context)
    {
        if (await HasAnyAsync(context.FantasyThemeDefinitions))
        {
            _logger.LogInformation("FantasyThemes already seeded, skipping");
            return;
        }

        var fantasyThemes = new[]
        {
            FantasyTheme.HighFantasy,
            FantasyTheme.LowFantasy,
            FantasyTheme.UrbanFantasy,
            FantasyTheme.FairyTale,
            FantasyTheme.Mythology,
            FantasyTheme.Steampunk,
            FantasyTheme.ScienceFantasy,
            FantasyTheme.DarkFantasy,
            FantasyTheme.Whimsical,
            FantasyTheme.Historical,
            FantasyTheme.AnimalFantasy,
            FantasyTheme.PortalFantasy
        };

        var entities = fantasyThemes.Select(theme => new FantasyThemeDefinition
        {
            Id = GenerateDeterministicId("fantasy-theme", theme.Value),
            Name = theme.DisplayName,
            Description = theme.Description,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await context.FantasyThemeDefinitions.AddRangeAsync(entities);
        await context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} fantasy themes from value objects", entities.Count);
    }

    private async Task SeedAgeGroupsAsync(MystiraAppDbContext context)
    {
        if (await HasAnyAsync(context.AgeGroupDefinitions))
        {
            _logger.LogInformation("AgeGroups already seeded, skipping");
            return;
        }

        var ageGroups = AgeGroup.All;

        var entities = ageGroups.Select(ageGroup => new AgeGroupDefinition
        {
            Id = GenerateDeterministicId("age-group", ageGroup.Id),
            Name = ageGroup.Name,
            Value = ageGroup.Id,
            MinimumAge = ageGroup.MinAge,
            MaximumAge = ageGroup.MaxAge,
            Description = $"Age group for ages {ageGroup.MinAge}-{ageGroup.MaxAge}",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        }).ToList();

        await context.AgeGroupDefinitions.AddRangeAsync(entities);
        await context.SaveChangesAsync();
        _logger.LogInformation("Seeded {Count} age groups from value objects", entities.Count);
    }

    /// <summary>
    /// Generates a deterministic ID based on entity type and name.
    /// This ensures idempotent seeding operations.
    /// </summary>
    private static string GenerateDeterministicId(string entityType, string name)
    {
        var input = $"{entityType}:{name.ToLowerInvariant()}";
        using var sha256 = System.Security.Cryptography.SHA256.Create();
        var hash = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        // Create a GUID-like format from the first 16 bytes of the hash
        var guidBytes = new byte[16];
        Array.Copy(hash, guidBytes, 16);
        return new Guid(guidBytes).ToString();
    }
}
