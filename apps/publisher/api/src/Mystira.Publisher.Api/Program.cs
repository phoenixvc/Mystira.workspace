using Mystira.Shared.Configuration;
using Microsoft.ApplicationInsights.Extensibility;
using Serilog;
using Serilog.Events;
using Mystira.Shared.Telemetry;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting Mystira.Publisher.Api");

    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services)
        .Enrich.FromLogContext()
        .Enrich.WithMachineName()
        .Enrich.WithThreadId()
        .Enrich.WithCorrelationId()
        .Enrich.WithProperty("Application", "Mystira.Publisher.Api")
        .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
        .WriteTo.ApplicationInsights(
            services.GetService<TelemetryConfiguration>(),
            TelemetryConverter.Traces));

    // Telemetry & Observability
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.EnableDependencyTrackingTelemetryModule = true;
    });

    builder.Services.AddCustomMetrics(builder.Environment.EnvironmentName);
    builder.Services.AddSecurityMetrics(builder.Environment.EnvironmentName);

    // Controllers & JSON
    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        });

    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new() { Title = "Mystira.Publisher.Api", Version = "v1" });
    });

    // Authentication & Authorization (shared)
    builder.Services.AddMystiraAuthentication(builder.Configuration, builder.Environment);
    builder.Services.AddMystiraEntraIdAuthentication(builder.Configuration);
    builder.Services.AddMystiraAuthorizationPolicies();

    // HTTP Context Accessor
    builder.Services.AddHttpContextAccessor();

    var app = builder.Build();

    // Configure the HTTP request pipeline
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();

    Log.Information("Mystira.Publisher.Api started successfully");

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    return 1;
}
finally
{
    Log.CloseAndFlush();
}

return 0;
