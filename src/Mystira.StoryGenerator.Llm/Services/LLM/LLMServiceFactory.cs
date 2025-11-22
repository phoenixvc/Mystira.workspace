using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Mystira.StoryGenerator.Contracts.Chat;
using Mystira.StoryGenerator.Contracts.Configuration;
using Mystira.StoryGenerator.Domain.Services;

namespace Mystira.StoryGenerator.Llm.Services.LLM;

/// <summary>
/// Implementation of LLM service factory (moved from API to LLM project)
/// </summary>
public class LLMServiceFactory : ILLMServiceFactory
{
    // Keep a reference to LLM-project ILLMService implementations to use IsAvailable(),
    // while exposing Domain-level interfaces to consumers.
    private readonly Dictionary<string, ILLMService> _services;
    private readonly AiSettings _settings;
    private readonly ILogger<LLMServiceFactory> _logger;

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

    public ILLMService? GetService(string providerName)
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
                _logger.LogDebug("Found available service for provider: {Provider}", providerName);
                return Adapt(service);
            }

            _logger.LogWarning("Service for provider {Provider} is not properly configured", providerName);
        }
        else
        {
            _logger.LogWarning("Unknown provider requested: {Provider}", providerName);
        }

        return null;
    }

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

            // Fallback to first available service
            service = GetAvailableServices().FirstOrDefault();
            if (service != null)
            {
                _logger.LogInformation("Using fallback provider: {Provider}", service.ProviderName);
            }
        }

        return service;
    }

    public IEnumerable<ILLMService> GetAvailableServices()
    {
        return _services.Values.Where(s => s.IsAvailable())
            .Select(Adapt);
    }

    private static ILLMService Adapt(ILLMService service)
    {
        if (service is ILLMService domain)
        {
            return domain;
        }

        return new LlmServiceAdapter(service);
    }

    private sealed class LlmServiceAdapter : ILLMService
    {
        private readonly ILLMService _inner;
        public LlmServiceAdapter(ILLMService inner) => _inner = inner;
        public string ProviderName => _inner.ProviderName;
        public Task<ChatCompletionResponse> CompleteAsync(ChatCompletionRequest request, CancellationToken cancellationToken = default)
            => _inner.CompleteAsync(request, cancellationToken);
        public bool IsAvailable() => _inner.IsAvailable();
    }
}
