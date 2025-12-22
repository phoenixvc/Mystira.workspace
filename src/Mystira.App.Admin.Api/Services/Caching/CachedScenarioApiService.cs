using Mystira.App.Domain.Models;
using CreateScenarioRequest = Mystira.App.Contracts.Requests.Scenarios.CreateScenarioRequest;
using ScenarioListResponse = Mystira.App.Contracts.Responses.Scenarios.ScenarioListResponse;
using ScenarioQueryRequest = Mystira.App.Contracts.Requests.Scenarios.ScenarioQueryRequest;
using ScenarioReferenceValidation = Mystira.App.Contracts.Responses.Scenarios.ScenarioReferenceValidation;

namespace Mystira.App.Admin.Api.Services.Caching;

/// <summary>
/// Caching decorator for IScenarioApiService.
/// Wraps the actual service and adds Redis/in-memory caching.
/// </summary>
public class CachedScenarioApiService : IScenarioApiService
{
    private readonly IScenarioApiService _inner;
    private readonly ICacheService _cache;
    private readonly ILogger<CachedScenarioApiService> _logger;

    public CachedScenarioApiService(
        IScenarioApiService inner,
        ICacheService cache,
        ILogger<CachedScenarioApiService> logger)
    {
        _inner = inner;
        _cache = cache;
        _logger = logger;
    }

    public async Task<Scenario?> GetScenarioByIdAsync(string id)
    {
        var key = CacheKeys.Scenario(id);
        return await _cache.GetOrSetAsync(
            key,
            async ct => await _inner.GetScenarioByIdAsync(id));
    }

    public async Task<ScenarioListResponse> GetScenariosAsync(ScenarioQueryRequest request)
    {
        // Only cache default/simple queries
        if (IsDefaultQuery(request))
        {
            var cached = await _cache.GetAsync<ScenarioListResponse>(CacheKeys.ScenariosList);
            if (cached is not null)
            {
                return cached;
            }
        }

        var result = await _inner.GetScenariosAsync(request);

        // Cache default queries only
        if (IsDefaultQuery(request))
        {
            await _cache.SetAsync(CacheKeys.ScenariosList, result);
        }

        return result;
    }

    public async Task<Scenario> CreateScenarioAsync(CreateScenarioRequest request)
    {
        var result = await _inner.CreateScenarioAsync(request);

        // Invalidate list cache
        await _cache.RemoveAsync(CacheKeys.ScenariosList);
        _logger.LogDebug("Cache invalidated after scenario creation: {ScenarioId}", result.Id);

        return result;
    }

    public async Task<Scenario?> UpdateScenarioAsync(string id, CreateScenarioRequest request)
    {
        var result = await _inner.UpdateScenarioAsync(id, request);

        if (result is not null)
        {
            // Invalidate both specific and list caches
            await _cache.RemoveAsync(CacheKeys.Scenario(id));
            await _cache.RemoveAsync(CacheKeys.ScenariosList);
            _logger.LogDebug("Cache invalidated after scenario update: {ScenarioId}", id);
        }

        return result;
    }

    public async Task<bool> DeleteScenarioAsync(string id)
    {
        var result = await _inner.DeleteScenarioAsync(id);

        if (result)
        {
            // Invalidate both specific and list caches
            await _cache.RemoveAsync(CacheKeys.Scenario(id));
            await _cache.RemoveAsync(CacheKeys.ScenariosList);
            _logger.LogDebug("Cache invalidated after scenario deletion: {ScenarioId}", id);
        }

        return result;
    }

    public async Task<List<Scenario>> GetScenariosByAgeGroupAsync(string ageGroup)
    {
        // Age group queries are not cached (low frequency, variable results)
        return await _inner.GetScenariosByAgeGroupAsync(ageGroup);
    }

    public async Task<List<Scenario>> GetFeaturedScenariosAsync()
    {
        var key = $"{CacheKeys.Prefix}scenarios:featured";
        var cached = await _cache.GetAsync<List<Scenario>>(key);
        if (cached is not null)
        {
            return cached;
        }

        var result = await _inner.GetFeaturedScenariosAsync();
        await _cache.SetAsync(key, result);
        return result;
    }

    public async Task ValidateScenarioAsync(Scenario scenario)
    {
        // Validation is not cached
        await _inner.ValidateScenarioAsync(scenario);
    }

    public async Task<ScenarioReferenceValidation> ValidateScenarioReferencesAsync(string scenarioId, bool includeMetadataValidation = true)
    {
        // Validation is not cached
        return await _inner.ValidateScenarioReferencesAsync(scenarioId, includeMetadataValidation);
    }

    public async Task<List<ScenarioReferenceValidation>> ValidateAllScenarioReferencesAsync(bool includeMetadataValidation = true)
    {
        // Validation is not cached
        return await _inner.ValidateAllScenarioReferencesAsync(includeMetadataValidation);
    }

    private static bool IsDefaultQuery(ScenarioQueryRequest request)
    {
        // Consider a query "default" if it has no specific filters
        return string.IsNullOrEmpty(request.AgeGroup) &&
               string.IsNullOrEmpty(request.SearchQuery) &&
               request.PageNumber <= 1 &&
               request.PageSize >= 50;
    }
}
