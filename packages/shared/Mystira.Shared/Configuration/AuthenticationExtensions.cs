using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Identity.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;

namespace Mystira.Shared.Configuration;

/// <summary>
/// Options for configuring Mystira authentication.
/// </summary>
public class AuthenticationOptions
{
    /// <summary>
    /// Paths that should skip JWT authentication validation.
    /// </summary>
    public string[] SkipAuthenticationPaths { get; set; } = [];
    
    /// <summary>
    /// Enable security metrics tracking for authentication events.
    /// </summary>
    public bool EnableSecurityMetrics { get; set; } = true;
}

/// <summary>
/// Extension methods for adding Mystira authentication to ASP.NET Core applications.
/// </summary>
public static class AuthenticationExtensions
{
    private static readonly string[] DefaultSkipPaths =
    [
        "/api/auth/refresh",
        "/api/auth/signin",
        "/api/auth/verify",
        "/api/auth/config",
        "/api/auth/magic/request",
        "/api/auth/magic/resend",
        "/api/auth/magic/verify",
        "/api/auth/magic/consume"
    ];

    private static readonly Regex EmailRegex = new(@"[^@]+@[^@]+", RegexOptions.Compiled);
    private static readonly Regex IpRegex = new(@"\d{1,3}\.\d{1,3}\.\d{1,3}\.\d{1,3}", RegexOptions.Compiled);

    /// <summary>
    /// Sanitizes potentially sensitive data for logging.
    /// </summary>
    public static string SanitizeForLog(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return string.Empty;

        var result = input;
        
        result = EmailRegex.Replace(result, "[EMAIL_REDACTED]");
        result = IpRegex.Replace(result, "[IP_REDACTED]");
        
        if (result.Length > 200)
            result = result[..200] + "...[truncated]";
            
        return result;
    }

    /// <summary>
    /// Hashes an identifier for privacy-safe logging.
    /// </summary>
    public static string HashId(string? input)
    {
        if (string.IsNullOrEmpty(input))
            return "[unknown]";
            
        using var sha256 = SHA256.Create();
        var bytes = Encoding.UTF8.GetBytes(input);
        var hash = sha256.ComputeHash(bytes);
        return Convert.ToBase64String(hash)[..12] + "...";
    }

    /// <summary>
    /// Adds Mystira JWT authentication to the service collection.
    /// </summary>
    public static IServiceCollection AddMystiraAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        return services.AddMystiraAuthentication(configuration, environment, _ => { });
    }

    /// <summary>
    /// Adds Mystira JWT authentication to the service collection with custom options.
    /// </summary>
    public static IServiceCollection AddMystiraAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment,
        Action<AuthenticationOptions> configureOptions)
    {
        var options = new AuthenticationOptions();
        configureOptions(options);

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

        var isDevelopment = environment.EnvironmentName == "Development";

        if (!useAsymmetric && !useSymmetric)
        {
            if (isDevelopment)
            {
                jwtKey = Environment.GetEnvironmentVariable("DEV_JWT_SECRET") ?? "DevSecret-StableKey-ForLocalDevelopmentOnly-2024";
            }
            else
            {
                throw new InvalidOperationException(
                    "JWT signing key not configured. Please provide either:\n" +
                    "- JwtSettings:RsaPublicKey for asymmetric RS256 verification (recommended), OR\n" +
                    "- JwtSettings:JwksEndpoint for JWKS-based key rotation (recommended), OR\n" +
                    "- JwtSettings:SecretKey for symmetric HS256 verification (legacy)\n" +
                    "Keys must be loaded from secure stores (Azure Key Vault, AWS Secrets Manager, etc.). " +
                    "Never hardcode secrets in source code.");
            }
        }

        var skipPaths = options.SkipAuthenticationPaths.Length > 0
            ? options.SkipAuthenticationPaths
            : DefaultSkipPaths;

        services.AddAuthentication(authOptions =>
            {
                authOptions.DefaultAuthenticateScheme = "Bearer";
                authOptions.DefaultChallengeScheme = "Bearer";
                authOptions.DefaultScheme = "Bearer";
            })
            .AddJwtBearer("Bearer", jwtOptions =>
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
                    jwtOptions.MetadataAddress = jwksEndpoint;
                    jwtOptions.RequireHttpsMetadata = !isDevelopment;
                    jwtOptions.RefreshInterval = TimeSpan.FromHours(1);
                    jwtOptions.AutomaticRefreshInterval = TimeSpan.FromHours(24);
                }
                else if (!string.IsNullOrWhiteSpace(jwtRsaPublicKey))
                {
                    try
                    {
                        using var rsa = RSA.Create();
                        rsa.ImportFromPem(jwtRsaPublicKey);
                        validationParameters.IssuerSigningKey = new RsaSecurityKey(rsa.ExportParameters(false));
                    }
                    catch (CryptographicException ex)
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
                }

                jwtOptions.TokenValidationParameters = validationParameters;

                jwtOptions.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var path = context.HttpContext.Request.Path.Value ?? string.Empty;
                        if (skipPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase)))
                        {
                            context.NoResult();
                        }
                        return Task.CompletedTask;
                    },
                    OnAuthenticationFailed = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("Mystira.Shared.Authentication");
                        if (logger != null && context.Exception != null)
                        {
                            var path = SanitizeForLog(context.HttpContext.Request.Path.Value);
                            var ua = SanitizeForLog(context.HttpContext.Request.Headers["User-Agent"].ToString());
                            logger.LogError(context.Exception, "JWT authentication failed on {Path} (UA: {UserAgent})", path, ua);
                        }

                        if (options.EnableSecurityMetrics)
                        {
                            TryTrackSecurityMetric(context, "TrackTokenValidationFailed", 
                                HashId(context.HttpContext.Connection.RemoteIpAddress?.ToString()),
                                context.Exception?.GetType().Name ?? "Unknown");
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("Mystira.Shared.Authentication");
                        if (logger != null)
                        {
                            var userId = context.Principal?.Identity?.Name;
                            logger.LogInformation("JWT token validated for user: {User}", HashId(userId));
                        }

                        if (options.EnableSecurityMetrics)
                        {
                            var userId = context.Principal?.Identity?.Name;
                            TryTrackSecurityMetric(context, "TrackAuthenticationSuccess", "JWT", HashId(userId));
                        }

                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("Mystira.Shared.Authentication");
                        if (logger != null)
                        {
                            logger.LogWarning("JWT challenge on {Path}: {Error} - {Description}",
                                SanitizeForLog(context.HttpContext.Request.Path.Value),
                                SanitizeForLog(context.Error),
                                SanitizeForLog(context.ErrorDescription));
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }

    private static void TryTrackSecurityMetric(Microsoft.AspNetCore.Authentication.JwtBearer.AuthenticationFailedContext context, string methodName, params string[] args)
    {
        try
        {
            var metricsType = Type.GetType("Mystira.Shared.Telemetry.ISecurityMetrics, Mystira.Shared.Observability");
            if (metricsType == null) return;
            
            var securityMetrics = context.HttpContext.RequestServices.GetService(metricsType);
            if (securityMetrics != null)
            {
                var method = securityMetrics.GetType().GetMethod(methodName);
                method?.Invoke(securityMetrics, args);
            }
        }
        catch
        {
            // Silently fail if security metrics not available
        }
    }

    private static void TryTrackSecurityMetric(Microsoft.AspNetCore.Authentication.JwtBearer.TokenValidatedContext context, string methodName, params string[] args)
    {
        try
        {
            var metricsType = Type.GetType("Mystira.Shared.Telemetry.ISecurityMetrics, Mystira.Shared.Observability");
            if (metricsType == null) return;
            
            var securityMetrics = context.HttpContext.RequestServices.GetService(metricsType);
            if (securityMetrics != null)
            {
                var method = securityMetrics.GetType().GetMethod(methodName);
                method?.Invoke(securityMetrics, args);
            }
        }
        catch
        {
            // Silently fail if security metrics not available
        }
    }

    /// <summary>
    /// Adds Microsoft Entra ID (Azure AD) authentication to the service collection.
    /// Call this after AddMystiraAuthentication to support both JWT and Entra ID.
    /// </summary>
    public static IServiceCollection AddMystiraEntraIdAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var azureAdSection = configuration.GetSection("AzureAd");
        var entraIdConfigured = !string.IsNullOrEmpty(azureAdSection["TenantId"]) &&
                                !string.IsNullOrEmpty(azureAdSection["ClientId"]);

        if (!entraIdConfigured)
        {
            var logger = services.BuildServiceProvider().GetService<ILoggerFactory>()?.CreateLogger("Mystira.Shared.Authentication");
            logger?.LogWarning("Microsoft Entra ID authentication not configured. Set AzureAd:TenantId and AzureAd:ClientId to enable.");
            return services;
        }

        services.AddAuthentication()
            .AddMicrosoftIdentityWebApi(azureAdSection, jwtBearerScheme: "AzureAd");

        var logger2 = services.BuildServiceProvider().GetService<ILoggerFactory>()?.CreateLogger("Mystira.Shared.Authentication");
        logger2?.LogInformation("Microsoft Entra ID authentication configured (TenantId: {TenantId})",
            azureAdSection["TenantId"]?[..Math.Min(8, azureAdSection["TenantId"]?.Length ?? 0)] + "...");

        return services;
    }
}
