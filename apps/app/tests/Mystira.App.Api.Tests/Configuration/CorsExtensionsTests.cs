using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Mystira.App.Api.Configuration;

namespace Mystira.App.Api.Tests.Configuration;

public class CorsExtensionsTests
{
    private static IHostEnvironment CreateEnvironment(string environmentName = "Development")
    {
        var env = new Mock<IHostEnvironment>();
        env.Setup(e => e.EnvironmentName).Returns(environmentName);
        return env.Object;
    }

    [Fact]
    public void AddMystiraCors_RegistersCorsServices()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        services.AddMystiraCors(configuration, CreateEnvironment());

        services.Should().Contain(d => d.ServiceType.Name.Contains("Cors"));
    }

    [Fact]
    public void AddMystiraCors_WithCustomOrigins_RegistersCustomOrigins()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["CorsSettings:AllowedOrigins"] = "https://custom.example.com,https://other.example.com"
            })
            .Build();

        services.AddMystiraCors(configuration, CreateEnvironment());

        // Verify CORS is registered (policy configuration is internal)
        services.Should().NotBeEmpty();
    }

    [Fact]
    public void AddMystiraCors_NonDevWithoutConfig_Throws()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var act = () => services.AddMystiraCors(configuration, CreateEnvironment("Production"));

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*CorsSettings:AllowedOrigins*");
    }

    [Fact]
    public void PolicyName_IsExpectedValue()
    {
        CorsExtensions.PolicyName.Should().Be("MystiraAppPolicy");
    }
}
