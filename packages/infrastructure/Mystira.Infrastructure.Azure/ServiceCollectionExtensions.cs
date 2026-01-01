using System;
using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Mystira.Application.Ports.Media;
using Mystira.Application.Ports.Storage;
using Mystira.Infrastructure.Azure.Configuration;
using Mystira.Infrastructure.Azure.HealthChecks;
using Mystira.Infrastructure.Azure.Services;

namespace Mystira.Infrastructure.Azure;

/// <summary>
/// Extension methods for configuring Azure infrastructure services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add Azure infrastructure services to the service collection
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Configure Azure options
        services.Configure<AzureOptions>(_ => configuration.GetSection(AzureOptions.SectionName));
        var azureOptions = new AzureOptions();
        configuration.GetSection(AzureOptions.SectionName).Bind(azureOptions);

        services.Configure<AudioTranscodingOptions>(configuration.GetSection(AudioTranscodingOptions.SectionName));
        services.AddSingleton<IAudioTranscodingService, FfmpegAudioTranscodingService>();

        // Add Cosmos DB
        services.AddCosmosDb(configuration, azureOptions.CosmosDb);

        // Add Blob Storage
        services.AddBlobStorage(configuration, azureOptions.BlobStorage);

        // Add Health Checks
        services.AddAzureHealthChecks();

        return services;
    }

    /// <summary>
    /// Add Cosmos DB services
    /// </summary>
    /// <typeparam name="TContext">The DbContext type for Cosmos DB.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCosmosDb<TContext>(this IServiceCollection services, IConfiguration configuration)
        where TContext : DbContext
    {
        var cosmosConnectionString = configuration.GetConnectionString("CosmosDb");

        if (!string.IsNullOrEmpty(cosmosConnectionString))
        {
            services.AddDbContext<TContext>(options =>
                options.UseCosmos(cosmosConnectionString, "MystiraAppDb"));
        }
        else
        {
            // Fallback to in-memory database for development
            services.AddDbContext<TContext>(options =>
                options.UseInMemoryDatabase("MystiraAppInMemoryDb"));
        }

        return services;
    }

    /// <summary>
    /// Add Cosmos DB services (internal method for infrastructure setup)
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="options">The Cosmos DB configuration options.</param>
    /// <returns>The service collection for chaining.</returns>
    private static IServiceCollection AddCosmosDb(this IServiceCollection services, IConfiguration configuration, CosmosDbOptions options)
    {
        var cosmosConnectionString = configuration.GetConnectionString("CosmosDb");

        if (!string.IsNullOrEmpty(cosmosConnectionString) && !options.UseInMemoryDatabase)
        {
            // Cosmos DB will be configured by the specific DbContext in the API project
            // This method just sets up the configuration
        }

        return services;
    }

    /// <summary>
    /// Add Azure Blob Storage services
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <param name="options">The Blob Storage configuration options.</param>
    /// <returns>The service collection for chaining.</returns>
    private static IServiceCollection AddBlobStorage(this IServiceCollection services, IConfiguration configuration, BlobStorageOptions options)
    {
        var blobConnectionString = configuration.GetConnectionString("AzureStorage");

        if (!string.IsNullOrEmpty(blobConnectionString))
        {
            services.AddSingleton(new BlobServiceClient(blobConnectionString));
        }
        else
        {
            // For development, use default Azure credentials
            services.AddSingleton(provider =>
            {
                var accountName = configuration["Azure:Storage:AccountName"] ?? "mystiraappdev";
                return new BlobServiceClient(
                    new Uri($"https://{accountName}.blob.core.windows.net"),
                    new DefaultAzureCredential());
            });
        }

        services.AddScoped<IBlobService, AzureBlobService>();

        return services;
    }

    /// <summary>
    /// Add Azure health checks
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    private static IServiceCollection AddAzureHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck<BlobStorageHealthCheck>("blob_storage")
            .AddCheck<CosmosDbHealthCheck>("cosmos_db");

        return services;
    }

    /// <summary>
    /// Add Azure Blob Storage services with explicit configuration
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddAzureBlobStorage(this IServiceCollection services, IConfiguration configuration)
    {
        var blobConnectionString = configuration.GetConnectionString("AzureStorage");

        if (!string.IsNullOrEmpty(blobConnectionString))
        {
            services.AddSingleton(new BlobServiceClient(blobConnectionString));
        }
        else
        {
            // For development, use default Azure credentials
            services.AddSingleton(provider =>
            {
                var accountName = configuration["Azure:Storage:AccountName"] ?? "mystiraappdev";
                return new BlobServiceClient(
                    new Uri($"https://{accountName}.blob.core.windows.net"),
                    new DefaultAzureCredential());
            });
        }

        services.AddScoped<IBlobService, AzureBlobService>();

        return services;
    }
}
