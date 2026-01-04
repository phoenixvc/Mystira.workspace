using Xunit;

namespace Mystira.Admin.Api.Tests;

/// <summary>
/// Basic tests for the Admin API.
/// Integration tests requiring full app startup are disabled until DI is fully configured.
/// </summary>
public class HealthCheckTests
{
    [Fact]
    public void ProjectCompiles_ReturnsTrue()
    {
        // Basic smoke test to verify the test project compiles and runs
        Assert.True(true);
    }

    [Fact]
    public void ProgramClass_Exists()
    {
        // Verify the Program class exists and is accessible
        var programType = typeof(Program);
        Assert.NotNull(programType);
        Assert.Equal("Program", programType.Name);
    }
}
