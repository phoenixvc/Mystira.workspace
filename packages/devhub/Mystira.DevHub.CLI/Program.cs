using System.Text.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Mystira.DevHub.CLI.Commands;
using Mystira.DevHub.CLI.Models;
using Mystira.DevHub.Services.Migration;

namespace Mystira.DevHub.CLI;

/// <summary>
/// CLI entry point for Cosmos DB migration operations.
/// Simplified to focus on migration functionality without external dependencies.
/// </summary>
internal class Program
{
    private static async Task<int> Main(string[] args)
    {
        try
        {
            // Build configuration
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                .AddEnvironmentVariables()
                .Build();

            // Setup dependency injection
            var services = new ServiceCollection();

            // Add logging (only errors to console)
            services.AddLogging(builder =>
            {
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
            });

            // Add configuration
            services.AddSingleton<IConfiguration>(configuration);

            // Add migration service
            services.AddScoped<IMigrationService, MigrationService>();

            // Add command handler
            services.AddScoped<MigrationCommands>();

            var serviceProvider = services.BuildServiceProvider();

            // Read JSON command from stdin
            string? jsonInput = null;
            if (args.Length > 0)
            {
                // Command provided as argument (for testing)
                jsonInput = string.Join(" ", args);
            }
            else
            {
                // Read from stdin (normal mode for Tauri)
                jsonInput = await Console.In.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(jsonInput))
            {
                var errorResponse = CommandResponse.Fail("No input provided");
                Console.WriteLine(JsonSerializer.Serialize(errorResponse));
                return 1;
            }

            // Parse the command
            CommandRequest? request;
            try
            {
                request = JsonSerializer.Deserialize<CommandRequest>(jsonInput);
                if (request == null)
                {
                    var errorResponse = CommandResponse.Fail("Invalid JSON input");
                    Console.WriteLine(JsonSerializer.Serialize(errorResponse));
                    return 1;
                }
            }
            catch (JsonException ex)
            {
                var errorResponse = CommandResponse.Fail($"JSON parse error: {ex.Message}");
                Console.WriteLine(JsonSerializer.Serialize(errorResponse));
                return 1;
            }

            // Route the command to the appropriate handler
            CommandResponse response;
            try
            {
                response = request.Command.ToLower() switch
                {
                    "migration.run" => await serviceProvider.GetRequiredService<MigrationCommands>().RunAsync(request.Args),
                    _ => CommandResponse.Fail($"Unknown command: {request.Command}. Available commands: migration.run")
                };
            }
            catch (Exception ex)
            {
                response = CommandResponse.Fail(ex.Message, ex.StackTrace);
            }

            // Write response as JSON to stdout
            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false
            });
            Console.WriteLine(jsonResponse);

            return response.Success ? 0 : 1;
        }
        catch (Exception ex)
        {
            // Last-resort error handler
            var errorResponse = CommandResponse.Fail($"Fatal error: {ex.Message}", ex.StackTrace);
            Console.WriteLine(JsonSerializer.Serialize(errorResponse));
            return 1;
        }
    }
}
