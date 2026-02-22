using System.Linq;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace Mystira.Admin.Api.Configuration;

/// <summary>
/// Swagger operation filter to handle IFormFile parameters for file uploads
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        // Get all parameters from the action descriptor
        var actionParameters = context.ApiDescription.ActionDescriptor.Parameters;

        // Check if any parameters are IFormFile or have [FromForm]
        var formParameters = actionParameters
            .Where(p => p.BindingInfo?.BindingSource?.Id == "Form" ||
                        p.ParameterType == typeof(IFormFile) ||
                        p.ParameterType == typeof(IFormFile[]))
            .ToList();

        // Only process if we have file or form parameters
        if (!formParameters.Any())
        {
            return;
        }

        // Remove all form parameters from the operation parameters list
        // They will be moved to the request body
        if (operation.Parameters != null)
        {
            var paramNamesToRemove = formParameters.Select(p => p.Name).ToHashSet();
            operation.Parameters = operation.Parameters
                .Where(p => !paramNamesToRemove.Contains(p.Name))
                .ToList();
        }

        // Create request body for multipart/form-data
        var requestBody = new OpenApiRequestBody
        {
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["multipart/form-data"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>()
                    }
                }
            }
        };

        // Add all form parameters (including IFormFile) to request body
        foreach (var param in formParameters)
        {
            OpenApiSchema schema;

            if (param.ParameterType == typeof(IFormFile))
            {
                schema = new OpenApiSchema
                {
                    Type = "string",
                    Format = "binary"
                };
            }
            else if (param.ParameterType == typeof(IFormFile[]))
            {
                schema = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    }
                };
            }
            else
            {
                schema = GetSchemaForType(param.ParameterType);
            }

            requestBody.Content["multipart/form-data"].Schema.Properties[param.Name] = schema;
        }

        operation.RequestBody = requestBody;
    }

    private static OpenApiSchema GetSchemaForType(Type type)
    {
        if (type == typeof(string))
        {
            return new OpenApiSchema { Type = "string" };
        }
        if (type == typeof(bool) || type == typeof(bool?))
        {
            return new OpenApiSchema { Type = "boolean" };
        }
        if (type == typeof(int) || type == typeof(int?))
        {
            return new OpenApiSchema { Type = "integer", Format = "int32" };
        }
        if (type.IsEnum)
        {
            return new OpenApiSchema { Type = "string" };
        }
        if (Nullable.GetUnderlyingType(type) != null)
        {
            return GetSchemaForType(Nullable.GetUnderlyingType(type)!);
        }

        return new OpenApiSchema { Type = "string" };
    }
}
