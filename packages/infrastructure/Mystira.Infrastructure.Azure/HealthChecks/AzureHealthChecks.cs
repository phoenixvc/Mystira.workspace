using System;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Mystira.Infrastructure.Azure.HealthChecks;

/// <summary>
/// Health check for Azure Cosmos DB connectivity.
/// </summary>
public class CosmosDbHealthCheck : IHealthCheck
{
    private readonly DbContext _context;

    /// <summary>
    /// Initializes a new instance of the <see cref="CosmosDbHealthCheck"/> class.
    /// </summary>
    /// <param name="context">The database context for Cosmos DB.</param>
    public CosmosDbHealthCheck(DbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    /// <summary>
    /// Checks the health of the Cosmos DB connection.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The health check result indicating whether the connection is healthy.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // EF Core Cosmos provider does not support CanConnectAsync.
            // Use the underlying CosmosClient to perform a lightweight connectivity check.
            var cosmosClient = _context.Database.GetCosmosClient();
            await cosmosClient.ReadAccountAsync().ConfigureAwait(false);
            return HealthCheckResult.Healthy("Cosmos DB connection is healthy");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Respect cancellation; let the hosting environment decide how to treat this.
            throw;
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Cosmos DB connection failed", ex);
        }
    }
}

/// <summary>
/// Health check for Azure Blob Storage connectivity.
/// </summary>
public class BlobStorageHealthCheck : IHealthCheck
{
    private readonly BlobServiceClient _blobServiceClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="BlobStorageHealthCheck"/> class.
    /// </summary>
    /// <param name="blobServiceClient">The Azure Blob Service client.</param>
    public BlobStorageHealthCheck(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
    }

    /// <summary>
    /// Checks the health of the Blob Storage connection.
    /// </summary>
    /// <param name="context">The health check context.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The health check result indicating whether the connection is healthy.</returns>
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Try to get blob service properties as a lightweight connectivity check.
            await _blobServiceClient.GetPropertiesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            return HealthCheckResult.Healthy("Blob storage connection is healthy");
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            // Respect cancellation; higher-layer health infrastructure decides how to interpret this.
            throw;
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Blob storage connection failed", ex);
        }
    }
}
