using System.Net.Http.Json;
using System.Text.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using Mystira.StoryGenerator.Web.Models;

namespace Mystira.StoryGenerator.Web.Services;

public interface IAiModelSettingsService
{
    Task InitializeAsync();
    Task<IReadOnlyList<AiModelDefinition>> GetAvailableModelsAsync();
    Task<EffectiveAiModelSettings> GetEffectiveSettingsAsync();
    Task UpdateSelectionAsync(string modelId, int maxTokens, double temperature);
    event EventHandler? SettingsChanged;
}

public class AiModelSettingsService : IAiModelSettingsService
{
    private const string ConfigPath = "config/ai-models.json";
    private const string ConfigCacheKey = "mystira_ai_model_config_cache";
    private const string SelectionCacheKey = "mystira_ai_model_selection";

    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly NavigationManager _navigationManager;
    private readonly ILogger<AiModelSettingsService> _logger;
    private readonly SemaphoreSlim _initializationLock = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    private AiModelConfiguration _configuration = CreateFallbackConfiguration();
    private AiModelSelection _selection;
    private bool _initialized;

    public event EventHandler? SettingsChanged;

    public AiModelSettingsService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        NavigationManager navigationManager,
        ILogger<AiModelSettingsService> logger)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _navigationManager = navigationManager;
        _logger = logger;
        _selection = CreateDefaultSelection(_configuration);
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

            var remoteConfig = await TryLoadRemoteConfigAsync();
            if (remoteConfig != null)
            {
                _configuration = remoteConfig;
                await CacheConfigAsync(remoteConfig);
            }
            else
            {
                var cachedConfig = await TryLoadCachedConfigAsync();
                if (cachedConfig != null)
                {
                    _configuration = cachedConfig;
                }
            }

            var storedSelection = await LoadSelectionAsync();
            if (!TryNormalizeSelection(storedSelection, out _selection))
            {
                _selection = CreateDefaultSelection(_configuration);
                await SaveSelectionAsync(_selection);
            }

            _initialized = true;
        }
        finally
        {
            _initializationLock.Release();
        }
    }

    public async Task<IReadOnlyList<AiModelDefinition>> GetAvailableModelsAsync()
    {
        await InitializeAsync();
        return _configuration.Models.ToList();
    }

    public async Task<EffectiveAiModelSettings> GetEffectiveSettingsAsync()
    {
        await InitializeAsync();
        var definition = ResolveModel(_selection.ModelId) ?? CreateSyntheticDefinition(_selection.ModelId);

        var normalizedSelection = new AiModelSelection
        {
            ModelId = definition.Id,
            MaxTokens = ClampTokens(definition, _selection.MaxTokens > 0 ? _selection.MaxTokens : definition.DefaultMaxTokens),
            Temperature = ClampTemperature(definition, _selection.Temperature)
        };

        if (!SelectionsEqual(_selection, normalizedSelection))
        {
            _selection = normalizedSelection;
            await SaveSelectionAsync(_selection);
        }

        return CreateEffectiveSettings(definition, _selection);
    }

    public async Task UpdateSelectionAsync(string modelId, int maxTokens, double temperature)
    {
        await InitializeAsync();
        var definition = ResolveModel(modelId) ?? CreateSyntheticDefinition(modelId);

        var updatedSelection = new AiModelSelection
        {
            ModelId = definition.Id,
            MaxTokens = ClampTokens(definition, maxTokens),
            Temperature = ClampTemperature(definition, temperature)
        };

        _selection = updatedSelection;
        await SaveSelectionAsync(_selection);
        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    private static AiModelConfiguration CreateFallbackConfiguration() => AiModelDefaults.CreateConfiguration();

    private static AiModelSelection CreateDefaultSelection(AiModelConfiguration configuration)
    {
        var model = AiModelDefaults.ResolveDefaultModel(configuration);
        return new AiModelSelection
        {
            ModelId = model.Id,
            MaxTokens = model.DefaultMaxTokens,
            Temperature = model.DefaultTemperature
        };
    }

    private EffectiveAiModelSettings CreateEffectiveSettings(AiModelDefinition definition, AiModelSelection selection)
    {
        return new EffectiveAiModelSettings
        {
            ModelId = definition.Id,
            Provider = string.IsNullOrWhiteSpace(definition.Provider) ? AiModelDefaults.DefaultProvider : definition.Provider,
            Deployment = string.IsNullOrWhiteSpace(definition.Deployment) ? definition.Id : definition.Deployment,
            MaxTokens = ClampTokens(definition, selection.MaxTokens),
            Temperature = ClampTemperature(definition, selection.Temperature),
            Definition = definition
        };
    }

    private async Task<AiModelConfiguration?> TryLoadRemoteConfigAsync()
    {
        try
        {
            var configUri = new Uri(new Uri(_navigationManager.BaseUri), ConfigPath);
            var config = await _httpClient.GetFromJsonAsync<AiModelConfiguration>(configUri, _jsonOptions);
            return NormalizeConfiguration(config);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load AI model configuration from {ConfigPath}", ConfigPath);
            return null;
        }
    }

    private async Task<AiModelConfiguration?> TryLoadCachedConfigAsync()
    {
        try
        {
            var cached = await _localStorage.GetItemAsync<AiModelConfiguration>(ConfigCacheKey);
            return NormalizeConfiguration(cached);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read cached AI model configuration");
            return null;
        }
    }

    private async Task CacheConfigAsync(AiModelConfiguration configuration)
    {
        try
        {
            await _localStorage.SetItemAsync(ConfigCacheKey, configuration);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to cache AI model configuration");
        }
    }

    private async Task<AiModelSelection?> LoadSelectionAsync()
    {
        try
        {
            return await _localStorage.GetItemAsync<AiModelSelection>(SelectionCacheKey);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to load AI model selection from storage");
            return null;
        }
    }

    private async Task SaveSelectionAsync(AiModelSelection selection)
    {
        try
        {
            await _localStorage.SetItemAsync(SelectionCacheKey, selection);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to persist AI model selection");
        }
    }

    private bool TryNormalizeSelection(AiModelSelection? selection, out AiModelSelection normalized)
    {
        if (selection == null)
        {
            normalized = default!;
            return false;
        }

        var definition = ResolveModel(selection.ModelId) ?? CreateSyntheticDefinition(selection.ModelId);

        normalized = new AiModelSelection
        {
            ModelId = definition.Id,
            MaxTokens = ClampTokens(definition, selection.MaxTokens > 0 ? selection.MaxTokens : definition.DefaultMaxTokens),
            Temperature = ClampTemperature(definition, selection.Temperature)
        };

        return true;
    }

    private AiModelDefinition? ResolveModel(string? modelId)
    {
        if (string.IsNullOrWhiteSpace(modelId))
        {
            return null;
        }

        return _configuration.Models.FirstOrDefault(m => string.Equals(m.Id, modelId, StringComparison.OrdinalIgnoreCase));
    }

    private static int ClampTokens(AiModelDefinition definition, int value)
    {
        var min = definition.MinTokens.HasValue && definition.MinTokens.Value > 0
            ? definition.MinTokens.Value
            : 1;
        var max = definition.MaxTokensLimit.HasValue && definition.MaxTokensLimit.Value > 0
            ? Math.Max(definition.MaxTokensLimit.Value, definition.DefaultMaxTokens)
            : Math.Max(definition.DefaultMaxTokens, min);

        if (value <= 0)
        {
            value = definition.DefaultMaxTokens;
        }

        return Math.Clamp(value, min, max);
    }

    private static double ClampTemperature(AiModelDefinition definition, double value)
    {
        var min = definition.MinTemperature ?? 0.0;
        var max = definition.MaxTemperature ?? 2.0;
        if (max < min)
        {
            (min, max) = (max, min);
        }

        if (double.IsNaN(value) || double.IsInfinity(value))
        {
            value = definition.DefaultTemperature;
        }

        return Math.Clamp(value, min, max);
    }

    private AiModelConfiguration? NormalizeConfiguration(AiModelConfiguration? configuration)
    {
        if (configuration == null)
        {
            return null;
        }

        var models = configuration.Models
            .Where(m => !string.IsNullOrWhiteSpace(m.Id))
            .Select(NormalizeDefinition)
            .ToList();

        if (models.Count == 0)
        {
            return null;
        }

        var defaultModelId = string.IsNullOrWhiteSpace(configuration.DefaultModelId)
            ? models.First().Id
            : configuration.DefaultModelId;

        return new AiModelConfiguration
        {
            DefaultModelId = defaultModelId,
            Models = models
        };
    }

    private static AiModelDefinition NormalizeDefinition(AiModelDefinition definition)
    {
        var provider = string.IsNullOrWhiteSpace(definition.Provider) ? AiModelDefaults.DefaultProvider : definition.Provider;
        var deployment = string.IsNullOrWhiteSpace(definition.Deployment) ? definition.Id : definition.Deployment;
        var minTokens = definition.MinTokens.HasValue && definition.MinTokens.Value > 0 ? definition.MinTokens.Value : 1;
        var defaultMaxTokens = definition.DefaultMaxTokens > 0 ? definition.DefaultMaxTokens : 1500;
        var maxTokensLimit = definition.MaxTokensLimit.HasValue && definition.MaxTokensLimit.Value > 0
            ? Math.Max(definition.MaxTokensLimit.Value, defaultMaxTokens)
            : (int?)null;

        if (maxTokensLimit.HasValue && maxTokensLimit.Value < minTokens)
        {
            maxTokensLimit = minTokens;
        }

        var minTemperature = definition.MinTemperature ?? 0.0;
        var maxTemperature = definition.MaxTemperature ?? 2.0;
        if (maxTemperature < minTemperature)
        {
            (minTemperature, maxTemperature) = (maxTemperature, minTemperature);
        }

        var defaultTemperature = definition.DefaultTemperature;
        if (defaultTemperature < minTemperature || defaultTemperature > maxTemperature)
        {
            defaultTemperature = Math.Clamp(defaultTemperature, minTemperature, maxTemperature);
        }

        return new AiModelDefinition
        {
            Id = definition.Id,
            DisplayName = string.IsNullOrWhiteSpace(definition.DisplayName) ? definition.Id : definition.DisplayName,
            Provider = provider,
            Deployment = deployment,
            DefaultMaxTokens = defaultMaxTokens,
            MaxTokensLimit = maxTokensLimit,
            MinTokens = minTokens,
            DefaultTemperature = defaultTemperature,
            MinTemperature = minTemperature,
            MaxTemperature = maxTemperature,
            Description = definition.Description
        };
    }

    private AiModelDefinition CreateSyntheticDefinition(string? modelId)
    {
        // When the selected model isn't part of the static config, synthesize a definition
        // using sensible defaults based on the current default model. This allows dynamic
        // models discovered at runtime to be persisted and used consistently.
        var baseDef = AiModelDefaults.ResolveDefaultModel(_configuration);
        var id = string.IsNullOrWhiteSpace(modelId) ? baseDef.Id : modelId;

        return new AiModelDefinition
        {
            Id = id,
            DisplayName = string.IsNullOrWhiteSpace(modelId) ? baseDef.DisplayName : modelId,
            Provider = string.IsNullOrWhiteSpace(baseDef.Provider) ? AiModelDefaults.DefaultProvider : baseDef.Provider,
            Deployment = id,
            DefaultMaxTokens = baseDef.DefaultMaxTokens,
            MaxTokensLimit = baseDef.MaxTokensLimit,
            MinTokens = baseDef.MinTokens,
            DefaultTemperature = baseDef.DefaultTemperature,
            MinTemperature = baseDef.MinTemperature,
            MaxTemperature = baseDef.MaxTemperature,
            Description = baseDef.Description
        };
    }

    private static bool SelectionsEqual(AiModelSelection a, AiModelSelection b)
    {
        return string.Equals(a.ModelId, b.ModelId, StringComparison.OrdinalIgnoreCase)
               && a.MaxTokens == b.MaxTokens
               && Math.Abs(a.Temperature - b.Temperature) < 0.0001;
    }
}
