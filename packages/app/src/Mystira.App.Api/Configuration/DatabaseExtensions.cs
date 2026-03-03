using Microsoft.EntityFrameworkCore;
using Mystira.App.Infrastructure.Data;
using Serilog;

namespace Mystira.App.Api.Configuration;

public static class DatabaseExtensions
{
    private static readonly SocketsHttpHandler _cosmosSocketHandler = new()
    {
        PooledConnectionLifetime = TimeSpan.FromMinutes(15)
    };

    private static readonly HttpClient _cosmosHttpClient = new(_cosmosSocketHandler, disposeHandler: false)
    {
        Timeout = TimeSpan.FromSeconds(30)
    };

    /// <summary>
    /// Configures Cosmos DB or In-Memory database, plus optional PostgreSQL.
    /// Returns (useCosmosDb, usePostgres) flags for later use.
    /// </summary>
    public static (bool UseCosmosDb, bool UsePostgres) AddMystiraDatabase(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Primary database: Cosmos DB or In-Memory
        var cosmosConnectionString = configuration.GetConnectionString("CosmosDb");
        var useCosmosDb = !string.IsNullOrEmpty(cosmosConnectionString);

        if (useCosmosDb)
        {
            services.AddDbContext<MystiraAppDbContext>(options =>
            {
                options.UseCosmos(cosmosConnectionString!, "MystiraAppDb", cosmosOptions =>
                {
                    cosmosOptions.HttpClientFactory(() => _cosmosHttpClient);
                    cosmosOptions.RequestTimeout(TimeSpan.FromSeconds(30));
                })
                .AddInterceptors(new PartitionKeyInterceptor());
            });
        }
        else
        {
            services.AddDbContext<MystiraAppDbContext>(options =>
                options.UseInMemoryDatabase("MystiraAppInMemoryDb_Local"));
        }

        services.AddScoped<DbContext>(sp => sp.GetRequiredService<MystiraAppDbContext>());

        // Secondary database: PostgreSQL for analytics
        var postgresConnectionString = configuration.GetConnectionString("PostgreSql");
        var usePostgres = !string.IsNullOrEmpty(postgresConnectionString);

        if (usePostgres)
        {
            services.AddDbContext<PostgresDbContext>(options =>
            {
                options.UseNpgsql(postgresConnectionString, npgsqlOptions =>
                {
                    npgsqlOptions.CommandTimeout(30);
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 3,
                        maxRetryDelay: TimeSpan.FromSeconds(5),
                        errorCodesToAdd: null);
                });
            });

            Log.Information("PostgreSQL configured for analytics/reporting");
        }
        else
        {
            Log.Information("PostgreSQL not configured - running with Cosmos only");
        }

        return (useCosmosDb, usePostgres);
    }

    /// <summary>
    /// Initializes the database and optionally seeds master data at startup.
    /// </summary>
    public static async Task InitializeDatabaseAsync(
        this WebApplication app,
        IConfiguration configuration,
        bool useCosmosDb)
    {
        var initializeDb = configuration.GetValue<bool>("InitializeDatabaseOnStartup", true);

        if (!initializeDb)
        {
            var startupLogger = app.Services.GetRequiredService<ILogger<Program>>();
            startupLogger.LogInformation("Database initialization skipped (InitializeDatabaseOnStartup=false). Ensure database and containers are pre-configured in Azure.");
            return;
        }

        using var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<MystiraAppDbContext>();
        var startupLog = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
        var isInMemory = !useCosmosDb;

        try
        {
            startupLog.LogInformation("Starting database initialization (InitializeDatabaseOnStartup={Init}, InMemory={InMemory})...", initializeDb, isInMemory);

            var initTask = context.Database.EnsureCreatedAsync();
            var timeoutTask = Task.Delay(TimeSpan.FromSeconds(30));
            var completedTask = await Task.WhenAny(initTask, timeoutTask);

            if (completedTask == timeoutTask)
            {
                startupLog.LogWarning("Database initialization timed out after 30 seconds. The application will start without database initialization. Ensure Azure Cosmos DB is accessible and configured correctly. Set 'InitializeDatabaseOnStartup'=false to skip this check.");

                _ = initTask.ContinueWith(t =>
                {
                    if (t.IsFaulted)
                    {
                        startupLog.LogError(t.Exception, "Background database initialization failed after timeout");
                    }
                    else if (t.IsCompletedSuccessfully)
                    {
                        startupLog.LogInformation("Background database initialization completed successfully after initial timeout");
                    }
                }, TaskScheduler.Default);
            }
            else
            {
                await initTask;
                startupLog.LogInformation("Database initialization succeeded. Verified containers for current model are present.");

                var seedOnStartup = configuration.GetValue<bool>("SeedMasterDataOnStartup");
                if (seedOnStartup || isInMemory)
                {
                    await SeedMasterDataAsync(scope, startupLog, seedOnStartup, isInMemory);
                }
            }
        }
        catch (Exception ex)
        {
            startupLog.LogCritical(ex, "Failed to initialize database during startup. Ensure Azure Cosmos DB database 'MystiraAppDb' exists and app identity has permissions to create/read containers. Expected containers include: CompassAxes (PK /Id), BadgeConfigurations (PK /Id), CharacterMaps (PK /Id), ContentBundles (PK /Id), Scenarios (PK /Id), MediaMetadataFiles (PK /Id), CharacterMediaMetadataFiles (PK /Id), CharacterMapFiles (PK /Id), UserProfiles (PK /Id), Accounts (PK /Id), PendingSignups (PK /email), GameSessions (PK /AccountId), MediaAssets (PK /MediaType).");
            throw;
        }
    }

    private static async Task SeedMasterDataAsync(
        IServiceScope scope,
        Microsoft.Extensions.Logging.ILogger startupLog,
        bool seedOnStartup,
        bool isInMemory)
    {
        try
        {
            var seeder = scope.ServiceProvider.GetRequiredService<Infrastructure.Data.Services.MasterDataSeederService>();

            var seedTask = seeder.SeedAllAsync();
            var seedTimeoutTask = Task.Delay(TimeSpan.FromSeconds(60));
            var seedCompletedTask = await Task.WhenAny(seedTask, seedTimeoutTask);

            if (seedCompletedTask == seedTimeoutTask)
            {
                startupLog.LogWarning("Master data seeding timed out after 60 seconds. The application will continue to start.");
            }
            else
            {
                await seedTask;
                startupLog.LogInformation("Master data seeding completed (SeedMasterDataOnStartup={Seed}, InMemory={InMemory}).", seedOnStartup, isInMemory);
            }
        }
        catch (Exception seedEx)
        {
            startupLog.LogError(seedEx, "Master data seeding failed. The application will continue to start. Set 'SeedMasterDataOnStartup'=false to skip seeding or use InMemory provider for local dev seeding.");
        }
    }
}
