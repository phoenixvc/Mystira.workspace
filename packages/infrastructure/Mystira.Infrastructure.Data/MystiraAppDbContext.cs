using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mystira.Domain.Enums;
using Mystira.Domain.Models;
using Mystira.Domain.ValueObjects;

namespace Mystira.Infrastructure.Data;

/// <summary>
/// DbContext for Mystira App following Hexagonal Architecture
/// Located in Infrastructure.Data (outer layer) as per Ports and Adapters pattern
/// </summary>
public partial class MystiraAppDbContext : DbContext
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MystiraAppDbContext"/> class.
    /// </summary>
    /// <param name="options">The options to configure the context.</param>
    public MystiraAppDbContext(DbContextOptions<MystiraAppDbContext> options)
        : base(options)
    {
    }

    /// <summary>Gets or sets the user profiles.</summary>
    public DbSet<UserProfile> UserProfiles { get; set; }
    /// <summary>Gets or sets the user badges.</summary>
    public DbSet<UserBadge> UserBadges { get; set; }
    /// <summary>Gets or sets the accounts.</summary>
    public DbSet<Account> Accounts { get; set; }

    /// <summary>Gets or sets the scenarios.</summary>
    public DbSet<Scenario> Scenarios { get; set; }
    /// <summary>Gets or sets the content bundles.</summary>
    public DbSet<ContentBundle> ContentBundles { get; set; }
    /// <summary>Gets or sets the character maps.</summary>
    public DbSet<CharacterMap> CharacterMaps { get; set; }
    /// <summary>Gets or sets the badge configurations.</summary>
    public DbSet<BadgeConfiguration> BadgeConfigurations { get; set; }
    /// <summary>Gets or sets the compass axis definitions.</summary>
    public DbSet<CompassAxisDefinition> CompassAxes { get; set; }
    /// <summary>Gets or sets the archetype definitions.</summary>
    public DbSet<ArchetypeDefinition> ArchetypeDefinitions { get; set; }
    /// <summary>Gets or sets the echo type definitions.</summary>
    public DbSet<EchoTypeDefinition> EchoTypeDefinitions { get; set; }
    /// <summary>Gets or sets the fantasy theme definitions.</summary>
    public DbSet<FantasyThemeDefinition> FantasyThemeDefinitions { get; set; }
    /// <summary>Gets or sets the age group definitions.</summary>
    public DbSet<AgeGroupDefinition> AgeGroupDefinitions { get; set; }

    /// <summary>Gets or sets the axis achievements.</summary>
    public DbSet<AxisAchievement> AxisAchievements { get; set; }
    /// <summary>Gets or sets the badges.</summary>
    public DbSet<Badge> Badges { get; set; }
    /// <summary>Gets or sets the badge images.</summary>
    public DbSet<BadgeImage> BadgeImages { get; set; }

    /// <summary>Gets or sets the media assets.</summary>
    public DbSet<MediaAsset> MediaAssets { get; set; }
    /// <summary>Gets or sets the media metadata files.</summary>
    public DbSet<MediaMetadataFile> MediaMetadataFiles { get; set; }
    /// <summary>Gets or sets the character media metadata files.</summary>
    public DbSet<CharacterMediaMetadataFile> CharacterMediaMetadataFiles { get; set; }
    /// <summary>Gets or sets the character map files.</summary>
    public DbSet<CharacterMapFile> CharacterMapFiles { get; set; }
    /// <summary>Gets or sets the avatar configuration files.</summary>
    public DbSet<AvatarConfigurationFile> AvatarConfigurationFiles { get; set; }

    /// <summary>Gets or sets the game sessions.</summary>
    public DbSet<GameSession> GameSessions { get; set; }

    /// <summary>Gets or sets the player scenario scores.</summary>
    public DbSet<PlayerScenarioScore> PlayerScenarioScores { get; set; }

    /// <summary>Gets or sets the compass trackings.</summary>
    public DbSet<CompassTracking> CompassTrackings { get; set; }

    /// <inheritdoc/>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Apply global query filters for soft-deletable entities
        ApplyGlobalQueryFilters(modelBuilder);

        // Check if we're using in-memory database (for testing)
        var isInMemoryDatabase = Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory";

        // Configure UserProfile
        // In in-memory provider used by tests, ensure EF doesn't try to map value objects like AgeGroup as entities
        if (isInMemoryDatabase)
        {
            modelBuilder.Ignore<AgeGroup>();
        }

        modelBuilder.Entity<UserProfile>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Do not map computed value-object property AgeGroup; only persist AgeGroupName (string)
            entity.Ignore(e => e.AgeGroup);
            // Only apply Cosmos DB configurations when not using in-memory database
            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                // Existing Cosmos container 'UserProfiles' uses partition key path '/id' (lowercase).
                // Use the Id property directly as the partition key.
                entity.ToContainer("UserProfiles")
                      .HasPartitionKey(e => e.Id);
            }

            entity.Property(e => e.PreferredFantasyThemes)
                  .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                  .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                      (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                      c => c.ToList()));

            // Configure UserBadge as owned by UserProfile only for Cosmos provider
            if (!isInMemoryDatabase)
            {
                entity.OwnsMany(p => p.EarnedBadges, badges =>
                {
                    badges.WithOwner().HasForeignKey(b => b.UserProfileId);
                    badges.HasKey(b => b.Id);

                    // No ToContainer for owned entities in Cosmos, they are embedded
                    badges.Property(b => b.UserProfileId).IsRequired();
                    badges.Property(b => b.BadgeConfigurationId).IsRequired();
                    badges.Property(b => b.BadgeName).IsRequired();
                    badges.Property(b => b.BadgeMessage).IsRequired();
                    badges.Property(b => b.Axis).IsRequired();
                });
            }
        });

        // When using InMemory provider for tests, configure UserBadge as a standalone entity
        if (isInMemoryDatabase)
        {
            modelBuilder.Entity<UserBadge>(entity =>
            {
                entity.HasKey(b => b.Id);
                entity.Property(b => b.UserProfileId).IsRequired();
                entity.Property(b => b.BadgeConfigurationId).IsRequired();
                entity.Property(b => b.BadgeName).IsRequired();
                entity.Property(b => b.BadgeMessage).IsRequired();
                entity.Property(b => b.Axis).IsRequired();

                entity.HasIndex(b => b.UserProfileId);
                entity.HasIndex(b => new { b.UserProfileId, b.BadgeConfigurationId }).IsUnique();
            });
        }


        // Configure Account
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Only apply Cosmos DB configurations when not using in-memory database
            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                // Existing Cosmos container 'Accounts' uses partition key path '/id' (lowercase).
                // Use the Id property directly as the partition key.
                entity.ToContainer("Accounts")
                      .HasPartitionKey(e => e.Id);
            }

            entity.Property(e => e.UserProfileIds)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                  .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                      (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                      c => c.ToList()));

            entity.OwnsOne(e => e.Subscription, subscription =>
            {
                subscription.Property(s => s.PurchasedScenarios)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                    .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));
            });

            entity.OwnsOne(e => e.Settings);
        });

        // Configure ContentBundle
        modelBuilder.Entity<ContentBundle>(entity =>
        {
            entity.HasKey(e => e.Id);

            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                // Existing Cosmos container 'ContentBundles' uses partition key path '/id' (lowercase).
                // Use the Id property directly as the partition key.
                entity.ToContainer("ContentBundles")
                      .HasPartitionKey(e => e.Id);
            }

            entity.Property(e => e.ScenarioIds)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                  .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                      (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                      c => c.ToList()));

            entity.Property(e => e.Prices)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<List<BundlePrice>>(v, (JsonSerializerOptions?)null) ?? new List<BundlePrice>())
                  .Metadata.SetValueComparer(new ValueComparer<List<BundlePrice>>(
                      (c1, c2) => c1 != null && c2 != null && c1.Count == c2.Count && c1.Zip(c2).All(x => x.First.Value == x.Second.Value && x.First.Currency == x.Second.Currency),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Value.GetHashCode(), v.Currency.GetHashCode())),
                      c => c.Select(p => new BundlePrice { Value = p.Value, Currency = p.Currency }).ToList()));

            // Own StoryProtocol metadata to avoid separate entity with PK requirement in tests
            entity.OwnsOne(e => e.StoryProtocol, sp =>
            {
                sp.OwnsMany(s => s.Contributors);
            });
        });

        // Configure CharacterMap
        modelBuilder.Entity<CharacterMap>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Only apply Cosmos DB configurations when not using in-memory database
            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                // Existing Cosmos container 'CharacterMaps' uses partition key path '/id' (lowercase).
                // Use the Id property directly as the partition key.
                entity.ToContainer("CharacterMaps")
                      .HasPartitionKey(e => e.Id);
            }

            entity.OwnsOne(e => e.Metadata, metadata =>
            {
                metadata.Property(m => m.Traits)
                    .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                    .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));
            });
        });

        // Configure BadgeConfiguration
        modelBuilder.Entity<BadgeConfiguration>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Only apply Cosmos DB configurations when not using in-memory database
            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                // Existing Cosmos container 'BadgeConfigurations' uses partition key path '/id' (lowercase).
                // Use the Id property directly as the partition key.
                entity.ToContainer("BadgeConfigurations")
                      .HasPartitionKey(e => e.Id);
            }
        });

        // Configure AxisAchievement (new badge system)
        modelBuilder.Entity<AxisAchievement>(entity =>
        {
            entity.HasKey(e => e.Id);

            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                entity.ToContainer("AxisAchievements")
                      .HasPartitionKey(e => e.Id);
            }
        });

        // Configure Badge (new badge system)
        modelBuilder.Entity<Badge>(entity =>
        {
            entity.HasKey(e => e.Id);

            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                entity.ToContainer("Badges")
                      .HasPartitionKey(e => e.Id);
            }
        });

        // Configure BadgeImage (new badge system)
        modelBuilder.Entity<BadgeImage>(entity =>
        {
            entity.HasKey(e => e.Id);

            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                entity.ToContainer("BadgeImages")
                      .HasPartitionKey(e => e.Id);
            }
        });

        // Configure CompassAxisDefinition
        modelBuilder.Entity<CompassAxisDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Only apply Cosmos DB configurations when not using in-memory database
            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                // Existing Cosmos container 'CompassAxes' uses partition key path '/id' (lowercase).
                // Use the Id property directly as the partition key.
                entity.ToContainer("CompassAxes")
                      .HasPartitionKey(e => e.Id);
            }
        });

        // Configure ArchetypeDefinition
        modelBuilder.Entity<ArchetypeDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Only apply Cosmos DB configurations when not using in-memory database
            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                // Existing Cosmos container 'ArchetypeDefinitions' uses partition key path '/id' (lowercase).
                // Map partition key to the entity key so EF Core targets '/id'.
                entity.ToContainer("ArchetypeDefinitions")
                      .HasPartitionKey(e => e.Id);
            }
        });

        // Configure EchoTypeDefinition
        modelBuilder.Entity<EchoTypeDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);

            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                // Existing Cosmos container 'EchoTypeDefinitions' uses partition key path '/id' (lowercase).
                // Use the Id property directly as the partition key.
                entity.ToContainer("EchoTypeDefinitions")
                      .HasPartitionKey(e => e.Id);
            }
        });

        // Configure FantasyThemeDefinition
        modelBuilder.Entity<FantasyThemeDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);

            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                // Existing Cosmos container 'FantasyThemeDefinitions' uses partition key path '/id' (lowercase).
                // Use the Id property directly as the partition key.
                entity.ToContainer("FantasyThemeDefinitions")
                      .HasPartitionKey(e => e.Id);
            }
        });

        // Configure AgeGroupDefinition
        modelBuilder.Entity<AgeGroupDefinition>(entity =>
        {
            entity.HasKey(e => e.Id);

            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to satisfy EF Core Cosmos requirement and standardize on /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                entity.ToContainer("AgeGroupDefinitions")
                      .HasPartitionKey(e => e.Id);
            }
        });

        // Configure Scenario
        modelBuilder.Entity<Scenario>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Only apply Cosmos DB configurations when not using in-memory database
            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                // Existing Cosmos container 'Scenarios' uses partition key path '/id' (lowercase).
                // Use the Id property directly as the partition key.
                entity.ToContainer("Scenarios")
                      .HasPartitionKey(e => e.Id);
            }

            entity.Property(e => e.Tags)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                  )
                  .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                      (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                      c => c.ToList()));

            entity.Property(e => e.Archetypes)
                  .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                  .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

            entity.Property(e => e.CoreAxes)
                  .HasConversion(
                        v => string.Join(',', v),
                        v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                  .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                        (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                        c => c.ToList()));

            entity.OwnsOne(e => e.MusicPalette, palette =>
            {
                palette.ToJsonProperty("MusicPalette");
                palette.Property(p => p.DefaultMood)
                       .ToJsonProperty("DefaultMood");

                palette.Property(p => p.MoodTracks)
                       .ToJsonProperty("MoodTracks")
                       .HasConversion(isInMemoryDatabase
                           ? (ValueConverter)new InMemoryDictionaryConverter()
                           : (ValueConverter)new CosmosDictionaryConverter())
                       .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, List<string>>>(
                           (d1, d2) => d1 != null && d2 != null && d1.Count == d2.Count && !d1.Except(d2).Any(),
                           d => d.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.Aggregate(0, (a2, v2) => HashCode.Combine(a2, v2.GetHashCode())))),
                           d => new Dictionary<string, List<string>>(d, StringComparer.OrdinalIgnoreCase)));
            });

            // Characters - ScenarioCharacter entities with simple properties (no Metadata complex type)
            entity.OwnsMany(e => e.Characters);

            entity.OwnsMany(e => e.Scenes, scene =>
            {
                scene.OwnsOne(s => s.Media);
                scene.OwnsOne(s => s.Music, music =>
                {
                    music.ToJsonProperty("Music");
                    music.Property(m => m.MoodProfile).ToJsonProperty("MoodProfile");
                    music.Property(m => m.TrackId).ToJsonProperty("TrackId");
                    music.Property(m => m.Volume).ToJsonProperty("Volume");
                    music.Property(m => m.Loop).ToJsonProperty("Loop");
                    music.Property(m => m.FadeInSeconds).ToJsonProperty("FadeInSeconds");
                    music.Property(m => m.FadeOutSeconds).ToJsonProperty("FadeOutSeconds");
                });
                scene.OwnsMany(s => s.SoundEffects, sfx =>
                {
                    sfx.ToJsonProperty("SoundEffects");
                    sfx.Property(s => s.TrackId).ToJsonProperty("TrackId");
                    sfx.Property(s => s.Volume).ToJsonProperty("Volume");
                    sfx.Property(s => s.Loop).ToJsonProperty("Loop");
                    sfx.Property(s => s.TriggerType).ToJsonProperty("TriggerType");
                    sfx.Property(s => s.DelaySeconds).ToJsonProperty("DelaySeconds");
                });
                scene.OwnsMany(s => s.Branches, branch =>
                {
                    branch.OwnsOne(b => b.EchoLog, echoLog =>
                    {
                        // EchoType is now a plain string
                        echoLog.Property(el => el.EchoType);
                    });
                    branch.OwnsOne(b => b.CompassChange);
                });
                scene.OwnsMany(s => s.EchoReveals, reveal =>
                {
                    // EchoType is now a plain string
                    reveal.Property(r => r.EchoType);
                });
            });

            // Own StoryProtocol metadata to avoid separate entity with PK requirement in tests
            entity.OwnsOne(e => e.StoryProtocol, sp =>
            {
                sp.OwnsMany(s => s.Contributors);
            });
        });

        // Configure GameSession
        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Only apply Cosmos DB configurations when not using in-memory database
            if (!isInMemoryDatabase)
            {
                // Map AccountId property to match container partition key path
                // Note: Different from other entities, GameSession uses AccountId as partition key
                entity.Property(e => e.AccountId).ToJsonProperty("accountId");

                entity.ToContainer("GameSessions")
                      .HasPartitionKey(e => e.AccountId);
            }

            entity.Property(e => e.PlayerNames)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                  )
                  .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                      (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                      c => c.ToList()));

            // ChoiceHistory - SessionChoice entities stored as owned JSON array
            entity.OwnsMany(e => e.ChoiceHistory, choice =>
            {
                // EchoGenerated is a bool property, not a complex type
                // CompassChange is a complex type that should be owned
                choice.OwnsOne(c => c.CompassChange);
            });

            // EchoHistory - EchoLog entities stored as owned JSON array
            entity.OwnsMany(e => e.EchoHistory);

            // Achievements - SessionAchievement entities stored as owned JSON array
            entity.OwnsMany(e => e.Achievements);

            // PlayerCompassProgressTotals is a Dictionary<string, int>, use JSON conversion
            entity.Property(e => e.PlayerCompassProgressTotals)
                .HasConversion(
                    v => System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                    v => System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(v, (System.Text.Json.JsonSerializerOptions?)null) ?? new Dictionary<string, int>())
                .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, int>>(
                    (d1, d2) => d1 != null && d2 != null && d1.Count == d2.Count && d1.All(kv => d2.ContainsKey(kv.Key) && d2[kv.Key] == kv.Value),
                    d => d == null ? 0 : d.Aggregate(0, (a, kv) => HashCode.Combine(a, kv.Key.GetHashCode(), kv.Value.GetHashCode())),
                    d => d == null ? new Dictionary<string, int>() : new Dictionary<string, int>(d)));

            // CharacterAssignments owned collection with nested owned PlayerAssignment
            entity.OwnsMany(e => e.CharacterAssignments, assignment =>
            {
                assignment.WithOwner();
                assignment.Property(a => a.CharacterId).IsRequired();
                assignment.Property(a => a.CharacterName).IsRequired(false);
                assignment.Property(a => a.Role).IsRequired(false);
                assignment.Property(a => a.Archetype).IsRequired(false);
                assignment.Property(a => a.Image).IsRequired(false);
                assignment.Property(a => a.Audio).IsRequired(false);
                assignment.Property(a => a.IsUnused).IsRequired();

                assignment.OwnsOne(a => a.PlayerAssignment, pa =>
                {
                    pa.Property(p => p.Type).IsRequired(false);
                    pa.Property(p => p.ProfileId).IsRequired(false);
                    pa.Property(p => p.ProfileName).IsRequired(false);
                    pa.Property(p => p.ProfileImage).IsRequired(false);
                    pa.Property(p => p.SelectedAvatarMediaId).IsRequired(false);
                    pa.Property(p => p.GuestName).IsRequired(false);
                    pa.Property(p => p.GuestAgeRange).IsRequired(false);
                    pa.Property(p => p.GuestAvatar).IsRequired(false);
                    pa.Property(p => p.SaveAsProfile).IsRequired();
                });
            });

            // Configure CompassValues as a JSON property (it's a List, not Dictionary)
            entity.Property(e => e.CompassValues)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<List<CompassTracking>>(v, (JsonSerializerOptions?)null) ?? new()
                  )
                  .Metadata.SetValueComparer(new ValueComparer<List<CompassTracking>>(
                      (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                      c => c.ToList()));
        });

        // Configure PlayerScenarioScore
        modelBuilder.Entity<PlayerScenarioScore>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Cosmos container mapping (only when not using in-memory provider)
            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' as required by Cosmos
                entity.Property(e => e.Id).ToJsonProperty("id");

                // Store PlayerScenarioScore items in their own container, partitioned by ProfileId
                // This optimizes lookups like GetByProfileIdAsync and aligns with the API endpoint usage
                entity.ToContainer("PlayerScenarioScores")
                      .HasPartitionKey(e => e.ProfileId);
            }

            // Store AxisScores as JSON string to work with both Cosmos and InMemory providers
            var dictComparer = new ValueComparer<Dictionary<string, int>>(
                (d1, d2) =>
                    d1 != null && d2 != null && d1.Count == d2.Count &&
                    d1.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)
                      .SequenceEqual(d2.OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase)),
                d => d == null ? 0 : d.Aggregate(0, (a, kv) => HashCode.Combine(a,
                    StringComparer.OrdinalIgnoreCase.GetHashCode(kv.Key), kv.Value.GetHashCode())),
                d => d == null ? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                                : new Dictionary<string, int>(d, StringComparer.OrdinalIgnoreCase));

            entity.Property(e => e.AxisScores)
                  .HasConversion(new ValueConverter<Dictionary<string, int>, string>(
                      v => AxisScoresSerializer.Serialize(v),
                      v => AxisScoresSerializer.Deserialize(v)))
                  .Metadata.SetValueComparer(dictComparer);
        });

        // Configure MediaAsset
        modelBuilder.Entity<MediaAsset>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Only apply Cosmos DB configurations when not using in-memory database
            if (!isInMemoryDatabase)
            {
                // Map MediaType property to match container partition key path
                // Note: Different from other entities, MediaAsset uses MediaType as partition key
                entity.Property(e => e.MediaType).ToJsonProperty("mediaType");

                entity.ToContainer("MediaAssets")
                      .HasPartitionKey(e => e.MediaType);
            }

            entity.Property(e => e.Tags)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
                  );

            // MediaAsset uses MetadataJson (string) for metadata storage, not an owned entity

            // Only apply indexes when using in-memory database (Cosmos DB doesn't support HasIndex)
            if (isInMemoryDatabase)
            {
                entity.HasIndex(e => e.MediaId).IsUnique();
                entity.HasIndex(e => e.MediaType);
                entity.HasIndex(e => e.CreatedAt);
            }
        });

        // Configure MediaMetadataFile
        modelBuilder.Entity<MediaMetadataFile>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Only apply Cosmos DB configurations when not using in-memory database
            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                // Existing Cosmos container 'MediaMetadataFiles' uses partition key path '/id' (lowercase).
                // Use the Id property directly as the partition key.
                entity.ToContainer("MediaMetadataFiles")
                      .HasPartitionKey(e => e.Id);
            }

            // Use OwnsMany for proper JSON handling in Cosmos DB
            entity.OwnsMany(e => e.Entries, entry =>
            {
                entry.Property(e => e.ClassificationTags)
                    .HasConversion(new ClassificationTagListConverter())
                    .Metadata.SetValueComparer(new ValueComparer<List<ClassificationTag>>(
                        (c1, c2) => c1 != null && c2 != null &&
                                    c1.Count == c2.Count &&
                                    !c1.Except(c2, new ClassificationTagComparer()).Any(),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.GetHashCode())),
                        c => c.Select(x => new ClassificationTag { Key = x.Key, Value = x.Value }).ToList()
                    ));

                entry.Property(e => e.Modifiers)
                    .HasConversion(new ModifierListConverter())
                    .Metadata.SetValueComparer(new ValueComparer<List<MetadataModifier>>(
                        (c1, c2) => c1 != null && c2 != null &&
                                    c1.Count == c2.Count &&
                                    !c1.Except(c2, new ModifierComparer()).Any(),
                        c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.GetHashCode())),
                        c => c.Select(x => new MetadataModifier { Key = x.Key, Value = x.Value }).ToList()
                    ));
            });

        });

        // Configure CharacterMediaMetadataFile
        modelBuilder.Entity<CharacterMediaMetadataFile>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Only apply Cosmos DB configurations when not using in-memory database
            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                // Existing Cosmos container 'CharacterMediaMetadataFiles' uses partition key path '/id' (lowercase).
                // Use the Id property directly as the partition key.
                entity.ToContainer("CharacterMediaMetadataFiles")
                      .HasPartitionKey(e => e.Id);
            }

            // Use OwnsMany for proper JSON handling in Cosmos DB
            entity.OwnsMany(e => e.Entries, entry =>
            {
                entry.Property(e => e.Tags)
                     .HasConversion(
                         v => string.Join(',', v),
                         v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                     .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                         (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                         c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                         c => c.ToList()));
            });
        });

        // Configure CharacterMapFile
        modelBuilder.Entity<CharacterMapFile>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Only apply Cosmos DB configurations when not using in-memory database
            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                // Existing Cosmos container 'CharacterMapFiles' uses partition key path '/id' (lowercase).
                // Use the Id property directly as the partition key.
                entity.ToContainer("CharacterMapFiles")
                      .HasPartitionKey(e => e.Id);
            }

            // Use OwnsMany for proper JSON handling in Cosmos DB
            // Note: Characters property uses CharacterMapFileCharacter from Domain
            entity.OwnsMany(e => e.Characters, character =>
            {
                character.OwnsOne(c => c.Metadata, metadata =>
                {
                    metadata.Property(m => m.Roles)
                            .HasConversion(
                                v => string.Join(',', v),
                                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                                c => c.ToList()));

                    metadata.Property(m => m.Archetypes)
                            .HasConversion(
                                v => string.Join(',', v),
                                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                                c => c.ToList()));

                    metadata.Property(m => m.Traits)
                            .HasConversion(
                                v => string.Join(',', v),
                                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                                c => c.ToList()));
                });
            });
        });

        // Configure AvatarConfigurationFile
        modelBuilder.Entity<AvatarConfigurationFile>(entity =>
        {
            entity.HasKey(e => e.Id);

            // Only apply Cosmos DB configurations when not using in-memory database
            if (!isInMemoryDatabase)
            {
                // Map Id property to lowercase 'id' to match container partition key path /id
                entity.Property(e => e.Id).ToJsonProperty("id");

                // Existing Cosmos container 'AvatarConfigurationFiles' uses partition key path '/id' (lowercase).
                // Use the Id property directly as the partition key.
                entity.ToContainer("AvatarConfigurationFiles")
                      .HasPartitionKey(e => e.Id);
            }

            // Convert Dictionary<string, List<string>> for storage
            entity.Property(e => e.AgeGroupAvatars)
                  .HasConversion(isInMemoryDatabase
                      ? (ValueConverter)new InMemoryDictionaryConverter()
                      : (ValueConverter)new CosmosDictionaryConverter())
                  .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, List<string>>>(
                      (c1, c2) => c1 != null && c2 != null && c1.Count == c2.Count &&
                                  c1.Keys.All(k => c2.ContainsKey(k) && c1[k].SequenceEqual(c2[k])),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.Aggregate(0, (a2, s) => HashCode.Combine(a2, s.GetHashCode())))),
                      c => new Dictionary<string, List<string>>(c.ToDictionary(kvp => kvp.Key, kvp => new List<string>(kvp.Value)))));
        });

        // Configure CompassTracking as a separate container for analytics
        modelBuilder.Entity<CompassTracking>(entity =>
        {
            entity.HasKey(e => e.Axis);

            if (!isInMemoryDatabase)
            {
                // Cosmos DB requires an 'id' JSON property. Map Axis to 'id' so the key aligns with Cosmos expectations.
                entity.Property(e => e.Axis).ToJsonProperty("id");

                entity.ToContainer("CompassTrackings")
                      .HasPartitionKey(e => e.Axis);
            }

            entity.OwnsMany(e => e.History);
        });
    }

    // Helper for serializing AxisScores dictionaries with case-insensitive keys
    private static class AxisScoresSerializer
    {
        private static readonly System.Text.Json.JsonSerializerOptions Options = new();

        public static string Serialize(Dictionary<string, int> value)
            => System.Text.Json.JsonSerializer.Serialize(value, Options);

        public static Dictionary<string, int> Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(json, Options);
            return dict == null
                ? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, int>(dict, StringComparer.OrdinalIgnoreCase);
        }
    }

    private static Dictionary<string, List<string>> DeserializeDictionary(object? input)
    {
        if (input == null)
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        if (input is Dictionary<string, List<string>> dict)
        {
            // Wrap with case-insensitive comparer if not already
            return dict.Comparer == StringComparer.OrdinalIgnoreCase
                ? dict
                : new Dictionary<string, List<string>>(dict, StringComparer.OrdinalIgnoreCase);
        }

        string? s;
        try
        {
            // If it's already a JToken (common in Cosmos provider)
            if (input is Newtonsoft.Json.Linq.JToken token)
            {
                if (token.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                {
                    var deserialized = token.ToObject<Dictionary<string, List<string>>>();
                    return WrapWithCaseInsensitiveComparer(deserialized);
                }
                s = token.ToString();
            }
            else
            {
                s = input.ToString();
            }

            if (string.IsNullOrWhiteSpace(s))
                return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            // First, try to see if it's a raw JSON object string
            var trimmed = s.Trim();
            if (trimmed.StartsWith("{"))
            {
                var deserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(s);
                return WrapWithCaseInsensitiveComparer(deserialized);
            }

            // Otherwise, it might be a JSON-serialized string containing JSON
            var innerJson = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(s);
            if (string.IsNullOrWhiteSpace(innerJson))
                return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            var innerDeserialized = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(innerJson);
            return WrapWithCaseInsensitiveComparer(innerDeserialized);
        }
        catch (Exception ex)
        {
            // Log the deserialization error for troubleshooting
            System.Diagnostics.Debug.WriteLine(
                $"[MystiraAppDbContext] Failed to deserialize Dictionary<string, List<string>> from input type {input?.GetType().Name ?? "null"}: {ex.Message}");
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        }
    }

    private static Dictionary<string, List<string>> WrapWithCaseInsensitiveComparer(Dictionary<string, List<string>>? dict)
    {
        if (dict == null)
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        return new Dictionary<string, List<string>>(dict, StringComparer.OrdinalIgnoreCase);
    }

    private class CosmosDictionaryConverter : ValueConverter<Dictionary<string, List<string>>, Newtonsoft.Json.Linq.JObject>
    {
        public CosmosDictionaryConverter()
            : base(
                v => Newtonsoft.Json.Linq.JObject.FromObject(v ?? new Dictionary<string, List<string>>()),
                v => DeserializeDictionary(v))
        {
        }
    }

    private class InMemoryDictionaryConverter : ValueConverter<Dictionary<string, List<string>>, string>
    {
        public InMemoryDictionaryConverter()
            : base(
                v => Newtonsoft.Json.JsonConvert.SerializeObject(v),
                v => DeserializeDictionary(v))
        {
        }
    }
}

/// <summary>
/// Value converter for converting List of ClassificationTag to/from a delimited string for database storage.
/// </summary>
public class ClassificationTagListConverter : ValueConverter<List<ClassificationTag>, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ClassificationTagListConverter"/> class.
    /// </summary>
    public ClassificationTagListConverter()
        : base(
            // Convert to DB type (List<ClassificationTag> -> string)
            tags => ConvertToString(tags),
            // Convert from DB type (string -> List<ClassificationTag>)
            dbString => ConvertFromString(dbString))
    {
    }

    private static string ConvertToString(List<ClassificationTag> tags)
    {
        if (tags == null || !tags.Any())
        {
            return string.Empty;
        }

        return string.Join("|", tags.Select(tag => $"{tag.Key}:{tag.Value}"));
    }

    private static List<ClassificationTag> ConvertFromString(string dbString)
    {
        if (string.IsNullOrEmpty(dbString))
        {
            return new List<ClassificationTag>();
        }

        return dbString.Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(s =>
            {
                var parts = s.Split(':', 2);
                return new ClassificationTag
                {
                    Key = parts[0],
                    Value = parts.Length > 1 ? parts[1] : string.Empty
                };
            })
            .ToList();
    }
}

/// <summary>
/// Equality comparer for ClassificationTag objects.
/// </summary>
public class ClassificationTagComparer : IEqualityComparer<ClassificationTag>
{
    /// <inheritdoc/>
    public bool Equals(ClassificationTag? x, ClassificationTag? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        return x.Key == y.Key && x.Value == y.Value;
    }

    /// <inheritdoc/>
    public int GetHashCode(ClassificationTag obj)
    {
        return HashCode.Combine(obj.Key, obj.Value);
    }
}

/// <summary>
/// Value converter for converting List of MetadataModifier to/from a delimited string for database storage.
/// </summary>
public class ModifierListConverter : ValueConverter<List<MetadataModifier>, string>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ModifierListConverter"/> class.
    /// </summary>
    public ModifierListConverter()
        : base(
            // Convert to DB type (List<MetadataModifier> -> string)
            modifiers => ConvertToString(modifiers),
            // Convert from DB type (string -> List<MetadataModifier>)
            dbString => ConvertFromString(dbString))
    {
    }

    private static string ConvertToString(List<MetadataModifier> modifiers)
    {
        if (modifiers == null || !modifiers.Any())
        {
            return string.Empty;
        }

        return string.Join("|", modifiers.Select(mod => $"{mod.Key}:{mod.Value}"));
    }

    private static List<MetadataModifier> ConvertFromString(string dbString)
    {
        if (string.IsNullOrEmpty(dbString))
        {
            return new List<MetadataModifier>();
        }

        return dbString.Split('|', StringSplitOptions.RemoveEmptyEntries)
            .Select(s =>
            {
                var parts = s.Split(':', 2);
                return new MetadataModifier
                {
                    Key = parts[0],
                    Value = parts.Length > 1 ? parts[1] : string.Empty
                };
            })
            .ToList();
    }
}

/// <summary>
/// Equality comparer for MetadataModifier objects.
/// </summary>
public class ModifierComparer : IEqualityComparer<MetadataModifier>
{
    /// <inheritdoc/>
    public bool Equals(MetadataModifier? x, MetadataModifier? y)
    {
        if (x == null && y == null)
        {
            return true;
        }

        if (x == null || y == null)
        {
            return false;
        }

        return x.Key == y.Key && x.Value == y.Value;
    }

    /// <inheritdoc/>
    public int GetHashCode(MetadataModifier obj)
    {
        return HashCode.Combine(obj.Key, obj.Value);
    }
}
