using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Mystira.App.Application.Ports.Payments;
using Mystira.App.Infrastructure.Payments.Configuration;
using Mystira.App.Infrastructure.Payments.Services.Mock;

namespace Mystira.App.Infrastructure.Payments.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPaymentServices_WithMockEnabled_RegistersMockPaymentService()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Payments:Enabled"] = "true",
                ["Payments:UseMockImplementation"] = "true"
            })
            .Build();

        services.AddLogging();
        services.AddPaymentServices(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var paymentService = serviceProvider.GetService<IPaymentService>();

        paymentService.Should().NotBeNull();
        paymentService.Should().BeOfType<MockPaymentService>();
    }

    [Fact]
    public void AddPaymentServices_WhenDisabled_RegistersMockPaymentService()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Payments:Enabled"] = "false"
            })
            .Build();

        services.AddLogging();
        services.AddPaymentServices(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var paymentService = serviceProvider.GetService<IPaymentService>();

        paymentService.Should().NotBeNull();
        paymentService.Should().BeOfType<MockPaymentService>();
    }

    [Fact]
    public void AddPaymentServices_ShouldConfigureOptions()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Payments:Enabled"] = "true",
                ["Payments:UseMockImplementation"] = "true",
                ["Payments:DefaultCurrency"] = "USD",
                ["Payments:MaxRetryAttempts"] = "5",
                ["Payments:TimeoutSeconds"] = "60"
            })
            .Build();

        services.AddLogging();
        services.AddPaymentServices(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<PaymentOptions>>().Value;

        options.DefaultCurrency.Should().Be("USD");
        options.MaxRetryAttempts.Should().Be(5);
        options.TimeoutSeconds.Should().Be(60);
    }

    [Fact]
    public void AddPaymentServices_WithDefaultConfig_UsesDefaultValues()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddLogging();
        services.AddPaymentServices(configuration);

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<PaymentOptions>>().Value;

        options.DefaultCurrency.Should().Be("ZAR");
        options.MaxRetryAttempts.Should().Be(3);
        options.TimeoutSeconds.Should().Be(30);
    }
}
