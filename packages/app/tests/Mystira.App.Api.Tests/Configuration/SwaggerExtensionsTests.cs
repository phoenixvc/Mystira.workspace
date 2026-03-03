using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Mystira.App.Api.Configuration;

namespace Mystira.App.Api.Tests.Configuration;

public class SwaggerExtensionsTests
{
    [Fact]
    public void AddMystiraSwagger_RegistersSwaggerServices()
    {
        var services = new ServiceCollection();

        services.AddMystiraSwagger();

        // SwaggerGen registers ISwaggerProvider
        services.Should().Contain(d => d.ServiceType.FullName != null &&
            d.ServiceType.FullName.Contains("Swagger"));
    }

    [Fact]
    public void AddMystiraSwagger_RegistersApiExplorer()
    {
        var services = new ServiceCollection();

        services.AddMystiraSwagger();

        services.Should().Contain(d => d.ServiceType.FullName != null &&
            d.ServiceType.FullName.Contains("ApiExplorer"));
    }

    [Fact]
    public void AddMystiraSwagger_ReturnsServiceCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddMystiraSwagger();

        result.Should().BeSameAs(services);
    }
}
