# Add Tests

Generate xUnit tests for a given source file or feature, following Mystira's testing conventions.

## Arguments

- `$ARGUMENTS` - One of:
  - A file path (e.g., `src/Mystira.App.Application/CQRS/GameSessions/Commands/CreateGameSessionCommandHandler.cs`)
  - A feature name (e.g., `GameSession`, `UserProfile`, `Badges`)
  - A test type flag: `--unit`, `--integration`, or `--e2e`

## Instructions

### 1. Identify What to Test

- If given a file path: read the file and generate tests for all public methods
- If given a feature name: find all related files across layers and generate tests for the most critical ones
- Prioritize by the testing pyramid: 70% unit, 20% integration, 10% E2E

### 2. Determine Test Project

Map source to test project:
| Source Project | Test Project |
|---|---|
| `Mystira.App.Domain` | `tests/Mystira.App.Domain.Tests` |
| `Mystira.App.Application` | `tests/Mystira.App.Application.Tests` |
| `Mystira.App.Api` | `tests/Mystira.App.Api.Tests` |
| `Mystira.App.Infrastructure.Data` | `tests/Mystira.App.Infrastructure.Data.Tests` |
| `Mystira.App.Infrastructure.Discord` | `tests/Mystira.App.Infrastructure.Discord.Tests` |
| `Mystira.App.Infrastructure.Payments` | `tests/Mystira.App.Infrastructure.Payments.Tests` |
| `Mystira.App.Infrastructure.Teams` | `tests/Mystira.App.Infrastructure.Teams.Tests` |
| `Mystira.App.Infrastructure.WhatsApp` | `tests/Mystira.App.Infrastructure.WhatsApp.Tests` |
| `Mystira.App.PWA` | `tests/Mystira.App.PWA.Tests` |

If the test project doesn't exist yet, create it with:
```bash
dotnet new xunit -n {TestProjectName} -o tests/{TestProjectName}
dotnet sln add tests/{TestProjectName}
```

### 3. Test Naming Convention

```
{MethodName}_{Scenario}_{ExpectedResult}
```

Examples:
- `Handle_ValidInput_ReturnsCreatedGameSession`
- `Handle_NullRequest_ThrowsArgumentNullException`
- `Handle_NonExistentEntity_ReturnsNull`

### 4. Test Structure

```csharp
using FluentAssertions;
using Moq;
using Xunit;

namespace {TestProjectNamespace};

public class {ClassUnderTest}Tests
{
    private readonly Mock<IDependency> _mockDependency;
    private readonly ClassUnderTest _sut; // System Under Test

    public {ClassUnderTest}Tests()
    {
        _mockDependency = new Mock<IDependency>();
        _sut = new ClassUnderTest(_mockDependency.Object);
    }

    [Fact]
    public async Task MethodName_Scenario_ExpectedResult()
    {
        // Arrange
        var input = new InputType { /* ... */ };
        _mockDependency
            .Setup(x => x.Method(It.IsAny<Type>()))
            .ReturnsAsync(expectedValue);

        // Act
        var result = await _sut.Method(input);

        // Assert
        result.Should().NotBeNull();
        result.Property.Should().Be(expectedValue);
        _mockDependency.Verify(x => x.Method(It.IsAny<Type>()), Times.Once);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public async Task MethodName_InvalidInput_ThrowsException(string? input)
    {
        // Arrange & Act
        var act = () => _sut.Method(input);

        // Assert
        await act.Should().ThrowAsync<ArgumentException>();
    }
}
```

### 5. What to Test

**For Command/Query Handlers:**
- Happy path (valid input -> expected output)
- Null/missing required fields
- Entity not found scenarios
- Duplicate detection (for Create operations)
- Authorization/ownership checks
- Verify repository methods called with correct arguments

**For Domain Entities:**
- Factory methods / constructors with valid input
- Business rule violations (invariant enforcement)
- State transitions
- Value object equality

**For Controllers (Integration):**
- HTTP status codes (200, 201, 204, 400, 401, 404)
- Request validation (DataAnnotations)
- Response shape matches DTO

### 6. After Generation

- Run the tests: `dotnet test tests/{TestProject}/ --verbosity normal`
- Report pass/fail results
- If any tests fail, fix them before finishing
- Report the number of tests added
