using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Mystira.StoryGenerator.Web;
using Mystira.StoryGenerator.Web.Services;
using Syncfusion.Blazor;
using Syncfusion.Licensing;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(builder.Configuration);

// Register Syncfusion license
var syncfusionLicenseKey = builder.Configuration["Syncfusion:LicenseKey"];
if (!string.IsNullOrWhiteSpace(syncfusionLicenseKey))
{
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

builder.Services.AddScoped(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var apiBaseUrl = configuration["Api:BaseUrl"];

    if (string.IsNullOrWhiteSpace(apiBaseUrl))
    {
        apiBaseUrl = builder.HostEnvironment.BaseAddress;
    }

    // Ensure long-running API calls (LLM operations) are not cut off by the default 100s timeout
    return new HttpClient
    {
        BaseAddress = new Uri(apiBaseUrl),
        Timeout = TimeSpan.FromSeconds(300)
    };
});

await builder.Build().RunAsync();
