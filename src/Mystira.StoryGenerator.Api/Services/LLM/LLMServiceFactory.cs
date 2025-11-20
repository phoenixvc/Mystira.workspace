using Microsoft.Extensions.Options;
using Mystira.StoryGenerator.Contracts.Configuration;

namespace Mystira.StoryGenerator.Api.Services.LLM;

/// <summary>
/// Factory for creating LLM service instances based on provider name
/// </summary>
public interface ILLMServiceFactory
{
    /// <summary>
    /// Get an LLM service instance for the specified provider
    /// </summary>
    /// <param name="providerName">The provider name (e.g., "azure-openai", "google-gemini")</param>
    /// <returns>The LLM service instance, or null if not found</returns>
    ILLMService? GetService(string providerName);

    /// <summary>
    /// Get the default LLM service based on configuration
    /// </summary>
    /// <returns>The default LLM service instance</returns>
    ILLMService? GetDefaultService();

    /// <summary>
    /// Get all available LLM services
    /// </summary>
    /// <returns>Collection of all available LLM services</returns>
    IEnumerable<ILLMService> GetAvailableServices();
}

/// <summary>
/// Implementation of LLM service factory
/// </summary>
public class LLMServiceFactory : ILLMServiceFactory
{
    private readonly Dictionary<string, ILLMService> _services;
    private readonly AiSettings _settings;
    private readonly ILogger<LLMServiceFactory> _logger;

    public LLMServiceFactory(
        IEnumerable<ILLMService> services,
        IOptions<AiSettings> options,
        ILogger<LLMServiceFactory> logger)
    {
        _services = services.ToDictionary(s => s.ProviderName, s => s);
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
                return service;
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
        return _services.Values.Where(s => s.IsAvailable());
    }
}
