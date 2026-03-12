using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mystira.Core.Ports.Messaging;
using Mystira.App.Infrastructure.WhatsApp.Configuration;
using Mystira.App.Infrastructure.WhatsApp.Services;

namespace Mystira.App.Infrastructure.WhatsApp.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddWhatsAppBot_ShouldRegisterRequiredServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WhatsApp:ConnectionString"] = "test-connection",
                ["WhatsApp:ChannelRegistrationId"] = "test-channel",
                ["WhatsApp:PhoneNumberId"] = "test-phone"
            })
            .Build();

        services.AddLogging();
        services.AddWhatsAppBot(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var options = serviceProvider.GetRequiredService<IOptions<WhatsAppOptions>>().Value;
        options.ConnectionString.Should().Be("test-connection");
        options.ChannelRegistrationId.Should().Be("test-channel");
    }

    [Fact]
    public void AddWhatsAppBot_ShouldConfigureOptions()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WhatsApp:ConnectionString"] = "test-connection",
                ["WhatsApp:PhoneNumberId"] = "test-phone",
                ["WhatsApp:DefaultTimeoutSeconds"] = "45",
                ["WhatsApp:MaxRetryAttempts"] = "5"
            })
            .Build();

        services.AddLogging();
        services.AddWhatsAppBot(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<WhatsAppOptions>>().Value;

        options.DefaultTimeoutSeconds.Should().Be(45);
        options.MaxRetryAttempts.Should().Be(5);
    }

    [Fact]
    public void AddWhatsAppBotAsDefault_ShouldRegisterIChatBotService()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WhatsApp:ConnectionString"] = "test-connection",
                ["WhatsApp:PhoneNumberId"] = "test-phone"
            })
            .Build();

        services.AddLogging();
        services.AddWhatsAppBotAsDefault(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var chatBotService = serviceProvider.GetService<IChatBotService>();

        chatBotService.Should().NotBeNull();
        chatBotService.Should().BeOfType<WhatsAppBotService>();
    }

    [Fact]
    public void AddWhatsAppBotAsDefault_ShouldRegisterIMessagingService()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WhatsApp:ConnectionString"] = "test-connection",
                ["WhatsApp:PhoneNumberId"] = "test-phone"
            })
            .Build();

        services.AddLogging();
        services.AddWhatsAppBotAsDefault(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var messagingService = serviceProvider.GetService<IMessagingService>();

        messagingService.Should().NotBeNull();
        messagingService.Should().BeOfType<WhatsAppBotService>();
    }

    [Fact]
    public void AddWhatsAppBotKeyed_ShouldRegisterKeyedServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WhatsApp:ConnectionString"] = "test-connection",
                ["WhatsApp:PhoneNumberId"] = "test-phone"
            })
            .Build();

        services.AddLogging();
        services.AddWhatsAppBotKeyed(configuration);

        var serviceProvider = services.BuildServiceProvider();

        var keyedMessaging = serviceProvider.GetKeyedService<IMessagingService>("whatsapp");
        var keyedChatBot = serviceProvider.GetKeyedService<IChatBotService>("whatsapp");
        var keyedBotCommand = serviceProvider.GetKeyedService<IBotCommandService>("whatsapp");

        keyedMessaging.Should().NotBeNull();
        keyedChatBot.Should().NotBeNull();
        keyedBotCommand.Should().NotBeNull();
    }

    [Fact]
    public void AddWhatsAppBotKeyed_WithCustomKey_ShouldUseCustomKey()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["WhatsApp:ConnectionString"] = "test-connection",
                ["WhatsApp:PhoneNumberId"] = "test-phone"
            })
            .Build();

        services.AddLogging();
        services.AddWhatsAppBotKeyed(configuration, "my-whatsapp-bot");

        var serviceProvider = services.BuildServiceProvider();

        var keyedService = serviceProvider.GetKeyedService<IChatBotService>("my-whatsapp-bot");
        keyedService.Should().NotBeNull();

        var defaultKeyService = serviceProvider.GetKeyedService<IChatBotService>("whatsapp");
        defaultKeyService.Should().BeNull();
    }
}
