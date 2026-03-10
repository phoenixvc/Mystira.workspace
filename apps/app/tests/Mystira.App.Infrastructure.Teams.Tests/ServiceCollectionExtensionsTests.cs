using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mystira.Application.Ports.Messaging;
using Mystira.App.Infrastructure.Teams.Configuration;
using Mystira.App.Infrastructure.Teams.Services;

namespace Mystira.App.Infrastructure.Teams.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddTeamsBot_ShouldRegisterRequiredServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Teams:MicrosoftAppId"] = "test-app-id",
                ["Teams:MicrosoftAppPassword"] = "test-password"
            })
            .Build();

        services.AddLogging();
        services.AddTeamsBot(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<TeamsOptions>>().Value;
        options.MicrosoftAppId.Should().Be("test-app-id");
    }

    [Fact]
    public void AddTeamsBot_ShouldConfigureOptions()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Teams:MicrosoftAppId"] = "my-app-id",
                ["Teams:MicrosoftAppPassword"] = "my-password",
                ["Teams:EnableAdaptiveCards"] = "false",
                ["Teams:DefaultTimeoutSeconds"] = "60"
            })
            .Build();

        services.AddLogging();
        services.AddTeamsBot(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<TeamsOptions>>().Value;

        options.MicrosoftAppId.Should().Be("my-app-id");
        options.EnableAdaptiveCards.Should().BeFalse();
        options.DefaultTimeoutSeconds.Should().Be(60);
    }

    [Fact]
    public void AddTeamsBotAsDefault_ShouldRegisterIChatBotService()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Teams:MicrosoftAppId"] = "test-app-id",
                ["Teams:MicrosoftAppPassword"] = "test-password"
            })
            .Build();

        services.AddLogging();
        services.AddTeamsBotAsDefault(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var chatBotService = serviceProvider.GetService<IChatBotService>();

        chatBotService.Should().NotBeNull();
        chatBotService.Should().BeOfType<TeamsBotService>();
    }

    [Fact]
    public void AddTeamsBotAsDefault_ShouldRegisterIMessagingService()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Teams:MicrosoftAppId"] = "test-app-id",
                ["Teams:MicrosoftAppPassword"] = "test-password"
            })
            .Build();

        services.AddLogging();
        services.AddTeamsBotAsDefault(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var messagingService = serviceProvider.GetService<IMessagingService>();

        messagingService.Should().NotBeNull();
        messagingService.Should().BeOfType<TeamsBotService>();
    }

    [Fact]
    public void AddTeamsBotKeyed_ShouldRegisterKeyedServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Teams:MicrosoftAppId"] = "test-app-id",
                ["Teams:MicrosoftAppPassword"] = "test-password"
            })
            .Build();

        services.AddLogging();
        services.AddTeamsBotKeyed(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var keyedMessaging = serviceProvider.GetKeyedService<IMessagingService>("teams");
        var keyedChatBot = serviceProvider.GetKeyedService<IChatBotService>("teams");
        var keyedBotCommand = serviceProvider.GetKeyedService<IBotCommandService>("teams");

        keyedMessaging.Should().NotBeNull();
        keyedChatBot.Should().NotBeNull();
        keyedBotCommand.Should().NotBeNull();
    }

    [Fact]
    public void AddTeamsBotKeyed_WithCustomKey_ShouldUseCustomKey()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Teams:MicrosoftAppId"] = "test-app-id",
                ["Teams:MicrosoftAppPassword"] = "test-password"
            })
            .Build();

        services.AddLogging();
        services.AddTeamsBotKeyed(configuration, "my-teams-bot");

        var serviceProvider = services.BuildServiceProvider();

        var keyedService = serviceProvider.GetKeyedService<IChatBotService>("my-teams-bot");
        keyedService.Should().NotBeNull();

        var defaultKeyService = serviceProvider.GetKeyedService<IChatBotService>("teams");
        defaultKeyService.Should().BeNull();
    }
}
