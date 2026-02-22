using System.Text;
using Microsoft.IdentityModel.Tokens;
using Mystira.App.Application.Helpers;
using Mystira.Shared.Telemetry;
using Serilog;

namespace Mystira.App.Api.Configuration;

public static class AuthenticationExtensions
{
    public static IServiceCollection AddMystiraAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        var jwtIssuer = configuration["JwtSettings:Issuer"];
        var jwtAudience = configuration["JwtSettings:Audience"];
        var jwtRsaPublicKey = configuration["JwtSettings:RsaPublicKey"];
        var jwtKey = configuration["JwtSettings:SecretKey"];
        var jwksEndpoint = configuration["JwtSettings:JwksEndpoint"];

        if (string.IsNullOrWhiteSpace(jwtIssuer))
        {
            throw new InvalidOperationException("JWT Issuer (JwtSettings:Issuer) is not configured.");
        }

        if (string.IsNullOrWhiteSpace(jwtAudience))
        {
            throw new InvalidOperationException("JWT Audience (JwtSettings:Audience) is not configured.");
        }

        bool useAsymmetric = !string.IsNullOrWhiteSpace(jwtRsaPublicKey) || !string.IsNullOrWhiteSpace(jwksEndpoint);
        bool useSymmetric = !string.IsNullOrWhiteSpace(jwtKey);

        if (!useAsymmetric && !useSymmetric)
        {
            throw new InvalidOperationException(
                "JWT signing key not configured. Please provide either:\n" +
                "- JwtSettings:RsaPublicKey for asymmetric RS256 verification (recommended), OR\n" +
                "- JwtSettings:JwksEndpoint for JWKS-based key rotation (recommended), OR\n" +
                "- JwtSettings:SecretKey for symmetric HS256 verification (legacy)\n" +
                "Keys must be loaded from secure stores (Azure Key Vault, AWS Secrets Manager, etc.). " +
                "Never hardcode secrets in source code.");
        }

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
                options.DefaultScheme = "Bearer";
            })
            .AddJwtBearer("Bearer", options =>
            {
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtIssuer,
                    ValidAudience = jwtAudience,
                    ClockSkew = TimeSpan.FromMinutes(5),
                    RoleClaimType = "role",
                    NameClaimType = "name"
                };

                if (!string.IsNullOrWhiteSpace(jwksEndpoint))
                {
                    options.MetadataAddress = jwksEndpoint;
                    options.RequireHttpsMetadata = !environment.IsDevelopment();
                    options.RefreshInterval = TimeSpan.FromHours(1);
                    options.AutomaticRefreshInterval = TimeSpan.FromHours(24);
                    Log.Information("JWT configured to use JWKS endpoint: {JwksEndpoint}", jwksEndpoint);
                }
                else if (!string.IsNullOrWhiteSpace(jwtRsaPublicKey))
                {
                    try
                    {
                        using var rsa = System.Security.Cryptography.RSA.Create();
                        rsa.ImportFromPem(jwtRsaPublicKey);
                        validationParameters.IssuerSigningKey = new RsaSecurityKey(rsa.ExportParameters(false));
                    }
                    catch (System.Security.Cryptography.CryptographicException ex)
                    {
                        throw new InvalidOperationException(
                            "Failed to load RSA public key. Ensure JwtSettings:RsaPublicKey contains a valid PEM-encoded RSA public key " +
                            "from a secure store (Azure Key Vault, AWS Secrets Manager, etc.)", ex);
                    }
                    catch (FormatException ex)
                    {
                        throw new InvalidOperationException(
                            "Failed to load RSA public key. Ensure JwtSettings:RsaPublicKey contains a valid PEM-encoded RSA public key " +
                            "from a secure store (Azure Key Vault, AWS Secrets Manager, etc.)", ex);
                    }
                }
                else if (!string.IsNullOrWhiteSpace(jwtKey))
                {
                    validationParameters.IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
                    Log.Warning("Using symmetric HS256 JWT signing. Consider migrating to asymmetric RS256 with JWKS for better security.");
                }

                options.TokenValidationParameters = validationParameters;

                options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
                        string[] skipPrefixes =
                        [
                            "/api/auth/refresh",
                            "/api/auth/signin",
                            "/api/auth/verify",
                            "/api/auth",
                            "/api/discord/status"
                        ];

                        if (skipPrefixes.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                        {
                            var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                            logger.LogDebug("Skipping JWT bearer processing for auth route: {Path}", path);
                            context.NoResult();
                            return Task.CompletedTask;
                        }

                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        var ua = LogAnonymizer.SanitizeForLog(context.HttpContext.Request.Headers["User-Agent"].ToString());
                        var path = LogAnonymizer.SanitizeForLog(context.HttpContext.Request.Path.Value);
                        logger.LogError(context.Exception, "JWT authentication failed on {Path} (UA: {UserAgent})", path, ua);

                        var securityMetrics = context.HttpContext.RequestServices.GetService<ISecurityMetrics>();
                        var clientIp = context.HttpContext.Connection.RemoteIpAddress?.ToString();
                        var reason = context.Exception?.GetType().Name ?? "Unknown";
                        securityMetrics?.TrackTokenValidationFailed(LogAnonymizer.HashId(clientIp), reason);

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        var userId = context.Principal?.Identity?.Name;
                        logger.LogInformation("JWT token validated for user: {User}", LogAnonymizer.HashId(userId));

                        var securityMetrics = context.HttpContext.RequestServices.GetService<ISecurityMetrics>();
                        securityMetrics?.TrackAuthenticationSuccess("JWT", LogAnonymizer.HashId(userId));

                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetRequiredService<ILogger<Program>>();
                        logger.LogWarning("JWT challenge on {Path}: {Error} - {Description}",
                            LogAnonymizer.SanitizeForLog(context.HttpContext.Request.Path.Value),
                            LogAnonymizer.SanitizeForLog(context.Error),
                            LogAnonymizer.SanitizeForLog(context.ErrorDescription));
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
