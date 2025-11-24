using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Web.Models;

namespace Mystira.StoryGenerator.Web.Services;

public interface IDynamicModelService
{
    Task InitializeAsync();
    Task<IReadOnlyList<ProviderModels>> GetAvailableProvidersAsync();
    Task<ProviderModels?> GetProviderAsync(string providerName);
    Task<ChatModelInfo?> GetModelAsync(string providerName, string modelId);
    event EventHandler? ModelsChanged;
}

public class DynamicModelService : IDynamicModelService
{
    private const string ModelsCacheKey = "mystira_dynamic_models_cache";
    private const string CacheTimestampKey = "mystira_dynamic_models_timestamp";
    private const int CacheValidityMinutes = 30; // Cache for 30 minutes

    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<DynamicModelService> _logger;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    private ChatModelsResponse _modelsResponse = new();
    private bool _initialized;

    public event EventHandler? ModelsChanged;

    public DynamicModelService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        NavigationManager navigationManager,
        ILogger<DynamicModelService> logger)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _navigationManager = navigationManager;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        if (_initialized)
        {
            return;
        }

        await _initializationLock.WaitAsync();
        try
        {
            if (_initialized)
            {
                return;
            }

            var cachedResponse = await TryLoadCachedModelsAsync();
            if (cachedResponse != null)
            {
                _modelsResponse = cachedResponse;
            }
            else
            {
                await RefreshModelsAsync();
            }

            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async Task<IReadOnlyList<ProviderModels>> GetAvailableProvidersAsync()
    {
        await InitializeAsync();
        return _modelsResponse.Providers.ToList();
    }

    public async Task<ProviderModels?> GetProviderAsync(string providerName)
    {
        await InitializeAsync();
        return _modelsResponse.Providers.FirstOrDefault(p => 
            p.Provider.Equals(providerName, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<ChatModelInfo?> GetModelAsync(string providerName, string modelId)
    {
        await InitializeAsync();
        var provider = await GetProviderAsync(providerName);
        return provider?.Models.FirstOrDefault(m => 
            m.Id.Equals(modelId, StringComparison.OrdinalIgnoreCase));
    }

    public async Task RefreshModelsAsync()
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ChatModelsResponse>("/api/chat/models", _jsonOptions);
            if (response != null)
            {
                _modelsResponse = response;
                await CacheModelsAsync(response);
                ModelsChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to refresh models from API");
            throw;
        }
    }

    private async Task<ChatModelsResponse?> TryLoadCachedModelsAsync()
    {
        try
        {
            var timestamp = await _localStorage.GetItemAsync<DateTime?>(CacheTimestampKey);
            if (timestamp.HasValue && DateTime.UtcNow.Subtract(timestamp.Value).TotalMinutes < CacheValidityMinutes)
            {
                var cached = await _localStorage.GetItemAsync<ChatModelsResponse>(ModelsCacheKey);
                return cached;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load cached models");
        }

        return null;
    }

    private async Task CacheModelsAsync(ChatModelsResponse response)
    {
        try
        {
            await _localStorage.SetItemAsync(ModelsCacheKey, response);
            await _localStorage.SetItemAsync(CacheTimestampKey, DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache models");
        }
    }

    public static AiModelDefinition ToAiModelDefinition(ProviderModels provider, ChatModelInfo model)
    {
        return new AiModelDefinition
        {
            Id = model.Id,
            DisplayName = model.DisplayName,
            Provider = provider.Provider,
            Deployment = model.Id,
            DefaultMaxTokens = model.MaxTokens,
            MaxTokensLimit = model.MaxTokens,
            MinTokens = 1,
            DefaultTemperature = model.DefaultTemperature,
            MinTemperature = model.MinTemperature,
            MaxTemperature = model.MaxTemperature,
            Description = model.Description
        };
    }

    public static EffectiveAiModelSettings ToEffectiveSettings(ProviderModels provider, ChatModelInfo model)
    {
        return new EffectiveAiModelSettings
        {
            ModelId = model.Id,
            Provider = provider.Provider,
            Deployment = model.Id,
            MaxTokens = model.MaxTokens,
            Temperature = model.DefaultTemperature,
            Definition = ToAiModelDefinition(provider, model)
        };
    }
}