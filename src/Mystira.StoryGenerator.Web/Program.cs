using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Mystira.StoryGenerator.Web;
using Mystira.StoryGenerator.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddSingleton(builder.Configuration);

// Add Blazored LocalStorage
builder.Services.AddBlazoredLocalStorage();

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

builder.Services.AddScoped(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var apiBaseUrl = configuration["Api:BaseUrl"];

    if (string.IsNullOrWhiteSpace(apiBaseUrl))
    {
        apiBaseUrl = builder.HostEnvironment.BaseAddress;
    }

    return new HttpClient { BaseAddress = new Uri(apiBaseUrl) };
});

await builder.Build().RunAsync();
