using Serilog;

namespace Mystira.App.Api.Configuration;

public static class CorsExtensions
{
    public const string PolicyName = "MystiraAppPolicy";

    private static readonly string[] DevFallbackOrigins =
    [
        "http://localhost:7000",
        "https://localhost:7000"
    ];

    public static IServiceCollection AddMystiraCors(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        var allowedOriginsConfig = configuration.GetSection("CorsSettings:AllowedOrigins").Get<string>();
        string[] originsToUse;

        if (!string.IsNullOrWhiteSpace(allowedOriginsConfig))
        {
            originsToUse = allowedOriginsConfig.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        }
        else if (environment.IsDevelopment())
        {
            Log.Warning("CorsSettings:AllowedOrigins not configured; using localhost-only fallback for development");
            originsToUse = DevFallbackOrigins;
        }
        else
        {
            throw new InvalidOperationException(
                "CorsSettings:AllowedOrigins must be configured in non-development environments. " +
                "Set a comma-separated list of allowed origins in configuration.");
        }

        services.AddCors(options =>
        {
            options.AddPolicy(PolicyName, policy =>
            {
                policy.WithOrigins(originsToUse);

                policy.WithHeaders(
                    "Content-Type",
                    "Authorization",
                    "X-Requested-With",
                    "X-Correlation-Id",
                    "Accept",
                    "Origin",
                    "User-Agent",
                    "Cache-Control",
                    "Pragma");

                policy.WithMethods(
                    HttpMethod.Get.Method,
                    HttpMethod.Post.Method,
                    HttpMethod.Put.Method,
                    HttpMethod.Patch.Method,
                    HttpMethod.Delete.Method,
                    HttpMethod.Options.Method);

                policy.AllowCredentials();
                policy.WithExposedHeaders("X-Correlation-Id");
                policy.SetPreflightMaxAge(TimeSpan.FromHours(24));
            });
        });

        return services;
    }
}
