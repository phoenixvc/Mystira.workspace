using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Mystira.StoryGenerator.Web;
using Mystira.StoryGenerator.Web.Services;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

#if DEBUG
Console.WriteLine("[DEBUG_LOG] Program.cs: Entering Program.cs");
Console.WriteLine("[DEBUG_LOG] Program.cs: Starting WebAssemblyHost build...");
#endif
var builder = WebAssemblyHostBuilder.CreateDefault(args);
#if DEBUG
Console.WriteLine("[DEBUG_LOG] Program.cs: Builder created.");
#endif
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

#if DEBUG
Console.WriteLine("[DEBUG_LOG] Program.cs: Registering services...");
#endif
builder.Services.AddSingleton(builder.Configuration);

// Register Syncfusion license
var syncfusionLicenseKey = builder.Configuration["Syncfusion:LicenseKey"];
if (!string.IsNullOrWhiteSpace(syncfusionLicenseKey))
{
#if DEBUG
    Console.WriteLine("[DEBUG_LOG] Program.cs: Registering Syncfusion license...");
#endif
    SyncfusionLicenseProvider.RegisterLicense(syncfusionLicenseKey);
}

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

// Add Syncfusion Blazor services
builder.Services.AddSyncfusionBlazor();

// Add Chat Session Service
builder.Services.AddScoped<IChatSessionService, ChatSessionService>();

// Add Story API Service
builder.Services.AddScoped<IStoryApiService, StoryApiService>();

// Add AI model settings service
builder.Services.AddScoped<IAiModelSettingsService, AiModelSettingsService>();

// Add Chat Completion Service
builder.Services.AddScoped<IChatService, ChatService>();

// Add YAML Import Service
builder.Services.AddScoped<IYamlImportService, YamlImportService>();

// Add Story Continuity Service
builder.Services.AddScoped<WebStoryContinuityService>();

// Add Scenario Dominator Path Analysis Service
builder.Services.AddScoped<WebScenarioDominatorPathAnalysisService>();

// Add Agent Session Service
builder.Services.AddScoped<IAgentSessionService, AgentSessionService>();

// Add SSE JavaScript Interop Service
builder.Services.AddScoped<SseJsInteropService>();

builder.Services.AddScoped(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var apiBaseUrl = configuration["Api:BaseUrl"];

    if (string.IsNullOrWhiteSpace(apiBaseUrl))
    {
        apiBaseUrl = builder.HostEnvironment.BaseAddress;
    }

#if DEBUG
    Console.WriteLine($"[DEBUG_LOG] Program.cs: HttpClient configured with BaseAddress: {apiBaseUrl}");
#endif

    // Ensure long-running API calls (LLM operations) are not cut off by the default 100s timeout
    return new HttpClient
    {
        BaseAddress = new Uri(apiBaseUrl),
        Timeout = TimeSpan.FromSeconds(600)
    };
});

#if DEBUG
Console.WriteLine("[DEBUG_LOG] Program.cs: Services registered. Building host...");
#endif
var host = builder.Build();
#if DEBUG
Console.WriteLine("[DEBUG_LOG] Program.cs: Host built. Running...");
#endif
await host.RunAsync();
#if DEBUG
Console.WriteLine("[DEBUG_LOG] Program.cs: RunAsync finished.");
#endif
