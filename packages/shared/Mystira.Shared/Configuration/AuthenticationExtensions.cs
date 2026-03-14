using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;

namespace Mystira.Shared.Configuration;

public class AuthenticationOptions
{
    public string[] SkipAuthenticationPaths { get; set; } = [];
    public bool EnableSecurityMetrics { get; set; } = true;
}

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

    public static IServiceCollection AddMystiraAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment environment)
    {
        return services.AddMystiraAuthentication(configuration, environment, _ => { });
    }

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

        if (!useAsymmetric && !useSymmetric)
        {
            if (environment.IsDevelopment())
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

        var isDevelopment = environment.IsDevelopment();

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
                    options.RequireHttpsMetadata = !isDevelopment;
                    options.RefreshInterval = TimeSpan.FromHours(1);
                    options.AutomaticRefreshInterval = TimeSpan.FromHours(24);
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

                options.TokenValidationParameters = validationParameters;

                options.Events = new JwtBearerEvents
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
                            var path = context.HttpContext.Request.Path.Value ?? "unknown";
                            logger.LogError(context.Exception, "JWT authentication failed on {Path}", path);
                        }

                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("Mystira.Shared.Authentication");
                        if (logger != null)
                        {
                            var userId = context.Principal?.Identity?.Name;
                            logger.LogInformation("JWT token validated for user: {User}", userId ?? "unknown");
                        }

                        return Task.CompletedTask;
                    },
                    OnChallenge = context =>
                    {
                        var logger = context.HttpContext.RequestServices.GetService<ILoggerFactory>()?.CreateLogger("Mystira.Shared.Authentication");
                        if (logger != null)
                        {
                            logger.LogWarning("JWT challenge on {Path}: {Error} - {Description}",
                                context.HttpContext.Request.Path.Value ?? "unknown",
                                context.Error ?? "none",
                                context.ErrorDescription ?? "none");
                        }
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization();

        return services;
    }
}
