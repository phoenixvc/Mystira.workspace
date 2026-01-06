using Microsoft.AspNetCore.Http;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace Mystira.Admin.Api.Configuration;

/// <summary>
/// Swagger operation filter to handle IFormFile parameters for file uploads
/// </summary>
public class FileUploadOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var fileParameters = context.ApiDescription.ActionDescriptor.Parameters
            .Where(p => p.ParameterType == typeof(IFormFile) || p.ParameterType == typeof(IFormFile[]))
            .ToList();

        if (fileParameters.Any())
        {
            operation.RequestBody = new OpenApiRequestBody
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

            // Remove IFormFile parameters from parameters list since they're now in the request body
            if (operation.Parameters != null)
            {
                var formFileParams = operation.Parameters
                    .Where(p => fileParameters.Any(fp => fp.Name == p.Name))
                    .ToList();

                foreach (var param in formFileParams)
                {
                    operation.Parameters.Remove(param);
                }
            }

            // Add form parameters to request body schema
            foreach (var param in fileParameters)
            {
                var schema = param.ParameterType == typeof(IFormFile[])
                    ? new OpenApiSchema
                    {
                        Type = "array",
                        Items = new OpenApiSchema
                        {
                            Type = "string",
                            Format = "binary"
                        }
                    }
                    : new OpenApiSchema
                    {
                        Type = "string",
                        Format = "binary"
                    };

                operation.RequestBody.Content["multipart/form-data"].Schema.Properties[param.Name] = schema;
            }

            // Add other [FromForm] parameters that aren't IFormFile
            var otherFormParams = context.ApiDescription.ActionDescriptor.Parameters
                .Where(p => p.BindingInfo?.BindingSource?.Id == "Form" && 
                           p.ParameterType != typeof(IFormFile) && 
                           p.ParameterType != typeof(IFormFile[]))
                .ToList();

            foreach (var param in otherFormParams)
            {
                var schema = GetSchemaForType(param.ParameterType);
                operation.RequestBody.Content["multipart/form-data"].Schema.Properties[param.Name] = schema;
            }
        }
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
