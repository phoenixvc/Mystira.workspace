using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Mystira.Ai.Abstractions;
using Mystira.Ai.Configuration;
using Mystira.Contracts.StoryGenerator.Chat;

namespace Mystira.Ai.Providers;

/// <summary>
/// Factory for creating and managing LLM service instances.
/// </summary>
public class LLMServiceFactory : ILlmServiceFactory
{
    private readonly Dictionary<string, ILLMService> _services;
    private readonly AiSettings _settings;
    private readonly ILogger<LLMServiceFactory> _logger;

    /// <summary>
    /// Creates a new LLM service factory.
    /// </summary>
    public LLMServiceFactory(
        IEnumerable<ILLMService> services,
        IOptions<AiSettings> options,
        ILogger<LLMServiceFactory> logger)
    {
        _services = services.ToDictionary(s => s.ProviderName.ToLowerInvariant(), s => s);
        _settings = options.Value;
        _logger = logger;

        _logger.LogInformation("LLM Service Factory initialized with {Count} providers: {Providers}",
            _services.Count, string.Join(", ", _services.Keys));
    }

    /// <inheritdoc />
    public ILLMService? GetService(string providerName, string? deploymentNameOrModelId = null)
    {
        if (string.IsNullOrWhiteSpace(providerName))
        {
            _logger.LogWarning("Provider name is null or empty");
            return null;
        }

        if (_services.TryGetValue(providerName.ToLowerInvariant(), out var service))
        {
            if (service.IsAvailable())
            {
                _logger.LogDebug("Found available service for provider: {Provider} with deployment/model: {DeploymentOrModel}",
                    providerName, deploymentNameOrModelId ?? "default");
                return Adapt(service, deploymentNameOrModelId);
            }

            _logger.LogWarning("Service for provider {Provider} is not properly configured", providerName);
        }
        else
        {
            _logger.LogWarning("Unknown provider requested: {Provider}", providerName);
        }

        return null;
    }

    /// <inheritdoc />
    public ILLMService? GetDefaultService()
    {
        if (string.IsNullOrWhiteSpace(_settings.DefaultProvider))
        {
            _logger.LogWarning("No default provider configured");
            return null;
        }

        var service = GetService(_settings.DefaultProvider);
        if (service == null)
        {
            _logger.LogWarning("Default provider {Provider} is not available, trying to find any available service",
                _settings.DefaultProvider);

            service = GetAvailableServices().FirstOrDefault();
            if (service != null)
            {
                _logger.LogInformation("Using fallback provider: {Provider}", service.ProviderName);
            }
        }

        return service;
    }

    /// <inheritdoc />
    public IEnumerable<ProviderModels> GetAvailableModels()
    {
        var providerModels = new List<ProviderModels>();

        foreach (var service in _services.Values)
        {
            var isAvailable = service.IsAvailable();
            var models = isAvailable ? service.GetAvailableModels() : Enumerable.Empty<ChatModelInfo>();

            providerModels.Add(new ProviderModels(service.ProviderName, models));
        }

        return providerModels;
    }

    /// <summary>
    /// Gets all available LLM services.
    /// </summary>
    public IEnumerable<ILLMService> GetAvailableServices()
    {
        var result = new List<ILLMService>();

        foreach (var service in _services.Values)
        {
            if (!service.IsAvailable())
            {
                continue;
            }

            var models = service.GetAvailableModels()?.ToList() ?? [];
            if (models.Count > 0)
            {
                foreach (var model in models)
                {
                    result.Add(Adapt(service, model.Id));
                }
            }
            else
            {
                result.Add(Adapt(service, null));
            }
        }

        return result;
    }

    private static ILLMService Adapt(ILLMService service, string? deploymentNameOrModelId = null)
    {
        if (service is AzureOpenAIService azureService && !string.IsNullOrWhiteSpace(deploymentNameOrModelId))
        {
            azureService.SetDeploymentNameOrModelId(deploymentNameOrModelId);
        }

        if (service is AnthropicAIService anthropicService && !string.IsNullOrWhiteSpace(deploymentNameOrModelId))
        {
            anthropicService.SetModelNameOrId(deploymentNameOrModelId);
        }

        return new LlmServiceAdapter(service, deploymentNameOrModelId);
    }

    private sealed class LlmServiceAdapter : ILLMService
    {
        private readonly ILLMService _inner;
        private readonly string? _deploymentNameOrModelId;

        public LlmServiceAdapter(ILLMService inner, string? deploymentNameOrModelId = null)
        {
            _inner = inner;
            _deploymentNameOrModelId = deploymentNameOrModelId;
        }

        public string ProviderName => _inner.ProviderName;
        public string? DeploymentNameOrModelId => _deploymentNameOrModelId ?? _inner.DeploymentNameOrModelId;

        public Task<ChatCompletionResponse> CompleteAsync(
            ChatCompletionRequest request,
            CancellationToken cancellationToken = default)
            => _inner.CompleteAsync(request, cancellationToken);

        public bool IsAvailable() => _inner.IsAvailable();
        public IEnumerable<ChatModelInfo> GetAvailableModels() => _inner.GetAvailableModels();
    }
}
