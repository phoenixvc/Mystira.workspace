using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Mystira.Domain.Models;
using Mystira.Domain.Enums;
using Mystira.Domain.ValueObjects;

namespace Mystira.App.Infrastructure.Data;

/// <summary>
/// DbContext for Mystira App following Hexagonal Architecture
/// Located in Infrastructure.Data (outer layer) as per Ports and Adapters pattern
/// </summary>
public partial class MystiraAppDbContext : DbContext
{
    public MystiraAppDbContext(DbContextOptions<MystiraAppDbContext> options)
        : base(options)
    {
    }

    // User and Profile Data
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserBadge> UserBadges { get; set; }
    public DbSet<Account> Accounts { get; set; }

    // Scenario Management
    public DbSet<Scenario> Scenarios { get; set; }
    public DbSet<ContentBundle> ContentBundles { get; set; }
    public DbSet<CharacterMap> CharacterMaps { get; set; }
    public DbSet<BadgeConfiguration> BadgeConfigurations { get; set; }
    public DbSet<CompassAxisDefinition> CompassAxes { get; set; }
    public DbSet<ArchetypeDefinition> ArchetypeDefinitions { get; set; }
    public DbSet<EchoTypeDefinition> EchoTypeDefinitions { get; set; }
    public DbSet<FantasyThemeDefinition> FantasyThemeDefinitions { get; set; }
    public DbSet<AgeGroupDefinition> AgeGroupDefinitions { get; set; }

    // Badge System
    public DbSet<AxisAchievement> AxisAchievements { get; set; }
    public DbSet<Badge> Badges { get; set; }
    public DbSet<BadgeImage> BadgeImages { get; set; }

    // Media Management
    public DbSet<MediaAsset> MediaAssets { get; set; }
    public DbSet<MediaMetadataFile> MediaMetadataFiles { get; set; }
    public DbSet<CharacterMediaMetadataFile> CharacterMediaMetadataFiles { get; set; }
    public DbSet<CharacterMapFile> CharacterMapFiles { get; set; }
    public DbSet<AvatarConfigurationFile> AvatarConfigurationFiles { get; set; }

    // Game Session Management
    public DbSet<GameSession> GameSessions { get; set; }

    // Scoring and Analytics
    public DbSet<PlayerScenarioScore> PlayerScenarioScores { get; set; }

    // Tracking and Analytics
    public DbSet<CompassTracking> CompassTrackings { get; set; }

    // COPPA Compliance
    public DbSet<ParentalConsent> ParentalConsents { get; set; }
    public DbSet<DataDeletionRequest> DataDeletionRequests { get; set; }
    public DbSet<PendingSignup> PendingSignups { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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
            entity.Ignore(e => e.AgeGroup);
            entity.Ignore(e => e.Theme);

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
                // Ignore alias properties
                sp.Ignore(s => s.RegistrationTxHash);
                sp.Ignore(s => s.RoyaltyModuleId);
                sp.OwnsMany(s => s.Contributors);
            });
        });

        // Configure CharacterMap
        modelBuilder.Entity<CharacterMap>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Ignore(e => e.Character);
            entity.Ignore(e => e.Archetype);

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
            entity.Ignore(e => e.Axis);
            entity.Ignore(e => e.Archetype);
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
            entity.Ignore(e => e.Axis);
            entity.Ignore(e => e.AgeGroup);

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
            // Ignore computed value-object / navigation properties
            entity.Ignore(e => e.AgeGroup);
            entity.Ignore(e => e.Theme);
            entity.Ignore(e => e.StartScene);
            entity.Ignore(e => e.Image);

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
                palette.Property(p => p.DefaultProfile)
                       .ToJsonProperty("DefaultProfile")
                       .HasConversion(
                           v => v.ToString(),
                           v => Enum.Parse<MusicProfile>(v, true));

                palette.Property(p => p.TracksByProfile)
                       .ToJsonProperty("TracksByProfile")
                       .HasConversion(isInMemoryDatabase
                           ? (ValueConverter)new InMemoryDictionaryConverter()
                           : (ValueConverter)new CosmosDictionaryConverter())
                       .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, List<string>>>(
                           (d1, d2) => d1 != null && d2 != null && d1.Count == d2.Count && !d1.Except(d2).Any(),
                           d => d.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.Aggregate(0, (a2, v2) => HashCode.Combine(a2, v2.GetHashCode())))),
                           d => new Dictionary<string, List<string>>(d, StringComparer.OrdinalIgnoreCase)));
            });

            entity.OwnsMany(e => e.Characters, character =>
            {
                // Ignore computed Archetype value object property; persist ArchetypeId string
                character.Ignore(c => c.Archetype);

                character.OwnsOne(c => c.Metadata, metadata =>
                {
                    // Ignore computed value-object properties; persist only the string ID lists
                    metadata.Ignore(m => m.Roles);
                    metadata.Ignore(m => m.Role);
                    metadata.Ignore(m => m.Archetypes);
                    metadata.Ignore(m => m.Archetype);
                    metadata.Ignore(m => m.Traits);
                    metadata.Ignore(m => m.Species);

                    metadata.Property(m => m.RoleIds)
                            .HasConversion(
                                v => string.Join(',', v),
                                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                                c => c.ToList()));

                    metadata.Property(m => m.ArchetypeIds)
                            .HasConversion(
                                v => string.Join(',', v),
                                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                                c => c.ToList()));

                    metadata.Property(m => m.TraitIds)
                            .HasConversion(
                                v => string.Join(',', v),
                                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                            .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                                (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                                c => c.ToList()));
                });
            });

            entity.OwnsMany(e => e.Scenes, scene =>
            {
                scene.OwnsOne(s => s.Media);
                scene.OwnsOne(s => s.Music, music =>
                {
                    music.ToJsonProperty("Music");
                    music.Property(m => m.Profile).ToJsonProperty("Profile")
                         .HasConversion(v => v.ToString(), v => Enum.Parse<MusicProfile>(v, true));
                    music.Property(m => m.Energy).ToJsonProperty("Energy");
                    music.Property(m => m.Continuity).ToJsonProperty("Continuity")
                         .HasConversion(v => v.ToString(), v => Enum.Parse<MusicContinuity>(v, true));
                    music.Property(m => m.TransitionHint).ToJsonProperty("TransitionHint")
                         .HasConversion(v => v.ToString(), v => Enum.Parse<MusicTransitionHint>(v, true));
                    music.Property(m => m.Priority).ToJsonProperty("Priority")
                         .HasConversion(v => v.ToString(), v => Enum.Parse<MusicPriority>(v, true));
                    music.Property(m => m.Ducking).ToJsonProperty("Ducking")
                         .HasConversion(v => v.ToString(), v => Enum.Parse<MusicDucking>(v, true));
                });
                scene.OwnsMany(s => s.SoundEffects, sfx =>
                {
                    sfx.ToJsonProperty("SoundEffects");
                    sfx.Property(s => s.Track).ToJsonProperty("Track");
                    sfx.Property(s => s.Loopable).ToJsonProperty("Loopable");
                    sfx.Property(s => s.Energy).ToJsonProperty("Energy");
                });
                // Ignore computed alias properties on Scene
                scene.Ignore(s => s.Description);
                scene.Ignore(s => s.CompassChanges);

                scene.OwnsMany(s => s.Branches, branch =>
                {
                    // Ignore computed alias properties on Branch
                    branch.Ignore(b => b.Choice);
                    branch.Ignore(b => b.NextSceneId);
                    branch.Ignore(b => b.CompassChanges);

                    branch.OwnsOne(b => b.EchoLog, echoLog =>
                    {
                        // Map stored EchoTypeId string; ignore computed EchoType value object and Description alias
                        echoLog.Property(e => e.EchoTypeId);
                        echoLog.Ignore(e => e.EchoType);
                        echoLog.Ignore(e => e.Description);
                    });
                    branch.OwnsOne(b => b.CompassChange, cc =>
                    {
                        // Ignore computed Axis value object
                        cc.Ignore(c => c.Axis);
                    });
                });
                scene.OwnsMany(s => s.EchoReveals);
            });

            // Own StoryProtocol metadata to avoid separate entity with PK requirement in tests
            entity.OwnsOne(e => e.StoryProtocol, sp =>
            {
                // Ignore alias properties
                sp.Ignore(s => s.RegistrationTxHash);
                sp.Ignore(s => s.RoyaltyModuleId);
                sp.OwnsMany(s => s.Contributors);
            });
        });

        // Configure GameSession
        modelBuilder.Entity<GameSession>(entity =>
        {
            entity.HasKey(e => e.Id);
            // Ignore computed alias / derived properties
            entity.Ignore(e => e.StartTime);
            entity.Ignore(e => e.EndTime);
            entity.Ignore(e => e.Duration);
            entity.Ignore(e => e.IsActive);
            entity.Ignore(e => e.Choices);
            entity.Ignore(e => e.PlayerAssignments);
            entity.Ignore(e => e.Scenario);

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

            entity.OwnsMany(e => e.ChoiceHistory, choice =>
            {
                // EchoGenerated is a bool property, not an owned type
                choice.Property(c => c.EchoGenerated);
                choice.OwnsOne(c => c.CompassChange, cc =>
                {
                    // Ignore computed Axis value object
                    cc.Ignore(c => c.Axis);
                });
            });

            entity.OwnsMany(e => e.EchoHistory, echo =>
            {
                // Map the stored EchoTypeId string; ignore the computed EchoType value object
                echo.Property(e => e.EchoTypeId);
                echo.Ignore(e => e.EchoType);
                echo.Ignore(e => e.Description);
            });
            entity.OwnsMany(e => e.Achievements);

            entity.Property(e => e.PlayerCompassProgressTotals)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<Dictionary<string, int>>(v, (JsonSerializerOptions?)null) ?? new())
                  .Metadata.SetValueComparer(new ValueComparer<Dictionary<string, int>>(
                      (c1, c2) => c1 != null && c2 != null && c1.Count == c2.Count && !c1.Except(c2).Any(),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.Key.GetHashCode(), v.Value.GetHashCode())),
                      c => new Dictionary<string, int>(c)));

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
                assignment.Ignore(a => a.Character);
                assignment.Ignore(a => a.Player);

                assignment.OwnsOne(a => a.PlayerAssignment, pa =>
                {
                    pa.Property(p => p.Type).HasConversion<int>();
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

            // Configure CompassValues as a JSON property
            entity.Property(e => e.CompassValues)
                  .HasConversion(
                      v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                      v => JsonSerializer.Deserialize<List<CompassTracking>>(v, (JsonSerializerOptions?)null) ?? new()
                  )
                  .Metadata.SetValueComparer(new ValueComparer<List<CompassTracking>>(
                      (c1, c2) => c1 != null && c2 != null && c1.Count == c2.Count,
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
            // Ignore computed alias / derived properties
            entity.Ignore(e => e.MediaId);
            entity.Ignore(e => e.FileSizeBytes);
            entity.Ignore(e => e.Description);
            entity.Ignore(e => e.Hash);
            entity.Ignore(e => e.SizeFormatted);
            entity.Ignore(e => e.Extension);

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

            // MetadataJson is a plain string property; no owned type mapping needed
            entity.Property(e => e.MetadataJson);

            // Only apply indexes when using in-memory database (Cosmos DB doesn't support HasIndex)
            if (isInMemoryDatabase)
            {
                entity.HasIndex(e => e.Id).IsUnique();
                entity.HasIndex(e => e.MediaType);
                entity.HasIndex(e => e.CreatedAt);
            }
        });

        // Configure MediaAsset.Tags
        modelBuilder.Entity<MediaAsset>()
            .Property(m => m.Tags)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList(),
                new ValueComparer<List<string>>(
                    (c1, c2) => (c1 == null && c2 == null) || (c1 != null && c2 != null && c1.SequenceEqual(c2)),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList()
                )
            );

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

        // Configure ParentalConsent (COPPA compliance)
        modelBuilder.Entity<ParentalConsent>(entity =>
        {
            entity.HasKey(e => e.Id);

            if (!isInMemoryDatabase)
            {
                entity.Property(e => e.Id).ToJsonProperty("id");
                entity.ToContainer("ParentalConsents")
                      .HasPartitionKey(e => e.ChildProfileId);
            }

            // DeletionAuditEntry is not embedded here - it's on DataDeletionRequest
        });

        // Configure DataDeletionRequest (COPPA compliance)
        modelBuilder.Entity<DataDeletionRequest>(entity =>
        {
            entity.HasKey(e => e.Id);

            if (!isInMemoryDatabase)
            {
                entity.Property(e => e.Id).ToJsonProperty("id");
                entity.ToContainer("DataDeletionRequests")
                      .HasPartitionKey(e => e.ChildProfileId);
            }

            // Embed audit trail entries as owned collection
            entity.OwnsMany(e => e.AuditTrail);

            // Embed deletion scope as JSON
            entity.Property(e => e.DeletionScope)
                  .HasConversion(
                      v => string.Join(',', v),
                      v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
                  .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                      (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                      c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                      c => c.ToList()));
        });

        // Configure PendingSignup (magic link auth)
        modelBuilder.Entity<PendingSignup>(entity =>
        {
            entity.HasKey(e => e.Id);

            if (!isInMemoryDatabase)
            {
                entity.Property(e => e.Id).ToJsonProperty("id");
                entity.ToContainer("PendingSignups")
                      .HasPartitionKey(e => e.Email);
            }
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

            entity.Ignore(e => e.AxisValues);
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
            {
                return new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            }

            var dict = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, int>>(json, Options);
            return dict == null
                ? new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
                : new Dictionary<string, int>(dict, StringComparer.OrdinalIgnoreCase);
        }
    }

    private static Dictionary<string, List<string>> DeserializeDictionary(object? input)
    {
        if (input == null)
        {
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        }

        if (input is Dictionary<string, List<string>> dict)
        {
            return dict;
        }

        string? s;
        try
        {
            // If it's already a JToken (common in Cosmos provider)
            if (input is Newtonsoft.Json.Linq.JToken token)
            {
                if (token.Type == Newtonsoft.Json.Linq.JTokenType.Object)
                {
                    return token.ToObject<Dictionary<string, List<string>>>()
                           ?? new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
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
                return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(s)
                       ?? new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            }

            // Otherwise, it might be a JSON-serialized string containing JSON
            var innerJson = Newtonsoft.Json.JsonConvert.DeserializeObject<string>(s);
            if (string.IsNullOrWhiteSpace(innerJson))
                return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, List<string>>>(innerJson)
                   ?? new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        }
        catch
        {
            return new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
        }
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

public class ClassificationTagListConverter : ValueConverter<List<ClassificationTag>, string>
{
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

public class ClassificationTagComparer : IEqualityComparer<ClassificationTag>
{
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

    public int GetHashCode(ClassificationTag obj)
    {
        return HashCode.Combine(obj.Key, obj.Value);
    }
}

public class ModifierListConverter : ValueConverter<List<MetadataModifier>, string>
{
    public ModifierListConverter()
        : base(
            modifiers => ConvertToString(modifiers),
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

public class ModifierComparer : IEqualityComparer<MetadataModifier>
{
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

    public int GetHashCode(MetadataModifier obj)
    {
        return HashCode.Combine(obj.Key, obj.Value);
    }
}
