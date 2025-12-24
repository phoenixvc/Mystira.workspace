using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Identity.Web;
using Mystira.Shared.Authentication;

namespace Mystira.Shared.Extensions;

/// <summary>
/// Extension methods for configuring Mystira authentication.
/// </summary>
public static class AuthenticationExtensions
{
    /// <summary>
    /// Adds Mystira authentication services using Microsoft Entra ID.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMystiraAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var jwtOptions = new JwtOptions();
        configuration.GetSection(JwtOptions.SectionName).Bind(jwtOptions);
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        if (jwtOptions.IsExternalId)
        {
            // Configure for Entra External ID (B2C-style)
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(configuration, JwtOptions.SectionName);
        }
        else
        {
            // Configure for Entra ID (standard Azure AD)
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(options =>
                {
                    configuration.GetSection(JwtOptions.SectionName).Bind(options);
                    options.TokenValidationParameters.ValidateIssuer = jwtOptions.ValidateIssuer;
                    options.TokenValidationParameters.ValidateAudience = jwtOptions.ValidateAudience;
                    options.TokenValidationParameters.ValidateLifetime = jwtOptions.ValidateLifetime;
                },
                options =>
                {
                    configuration.GetSection(JwtOptions.SectionName).Bind(options);
                });
        }

        // Register JWT service
        services.AddScoped<IJwtService, JwtService>();

        return services;
    }

    /// <summary>
    /// Adds Mystira authentication for service-to-service communication using Managed Identity.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection AddMystiraServiceAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        // For service-to-service, we validate tokens but don't need full MSAL
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var jwtOptions = new JwtOptions();
                configuration.GetSection(JwtOptions.SectionName).Bind(jwtOptions);

                options.Authority = jwtOptions.Authority;
                options.Audience = jwtOptions.Audience;
                options.TokenValidationParameters.ValidateIssuer = jwtOptions.ValidateIssuer;
                options.TokenValidationParameters.ValidateAudience = jwtOptions.ValidateAudience;
                options.TokenValidationParameters.ValidateLifetime = jwtOptions.ValidateLifetime;
            });

        services.AddScoped<IJwtService, JwtService>();

        return services;
    }
}
