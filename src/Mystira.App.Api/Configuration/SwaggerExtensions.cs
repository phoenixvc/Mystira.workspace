using Microsoft.OpenApi.Models;

namespace Mystira.App.Api.Configuration;

public static class SwaggerExtensions
{
    public static IServiceCollection AddMystiraSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Mystira API",
                Version = "v1",
                Description = "Backend API for Mystira - Dynamic Story App for Child Development",
                Contact = new OpenApiContact
                {
                    Name = "Mystira Team",
                    Email = "support@mystira.app"
                }
            });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            c.CustomSchemaIds(type =>
            {
                if (type == typeof(Mystira.App.Domain.Models.CharacterMetadata))
                {
                    return "DomainCharacterMetadata";
                }

                if (type == typeof(Mystira.App.Api.Models.CharacterMetadata))
                {
                    return "ApiCharacterMetadata";
                }

                return type.Name;
            });
        });

        return services;
    }
}
