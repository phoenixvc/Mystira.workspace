using FluentAssertions;
using Mystira.Core.Services;

namespace Mystira.App.Application.Tests.Services;

public class PercentileCalculatorTests
{
    [Fact]
    public void CalculatePercentile_WithEmptyList_ReturnsZero()
    {
        var result = PercentileCalculator.CalculatePercentile(new List<double>(), 50);
        result.Should().Be(0);
    }

    [Fact]
    public void CalculatePercentile_WithSingleElement_ReturnsThatElement()
    {
        var result = PercentileCalculator.CalculatePercentile(new List<double> { 42.0 }, 50);
        result.Should().Be(42.0);
    }

    [Fact]
    public void CalculatePercentile_0th_ReturnsMinimum()
    {
        var sorted = new List<double> { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var result = PercentileCalculator.CalculatePercentile(sorted, 0);
        result.Should().Be(1.0);
    }

    [Fact]
    public void CalculatePercentile_100th_ReturnsMaximum()
    {
        var sorted = new List<double> { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var result = PercentileCalculator.CalculatePercentile(sorted, 100);
        result.Should().Be(5.0);
    }

    [Fact]
    public void CalculatePercentile_50th_ReturnsMedian()
    {
        var sorted = new List<double> { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var result = PercentileCalculator.CalculatePercentile(sorted, 50);
        result.Should().Be(3.0);
    }

    [Fact]
    public void CalculatePercentile_25th_InterpolatesCorrectly()
    {
        var sorted = new List<double> { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var result = PercentileCalculator.CalculatePercentile(sorted, 25);
        result.Should().Be(2.0);
    }

    [Fact]
    public void CalculatePercentile_75th_InterpolatesCorrectly()
    {
        var sorted = new List<double> { 1.0, 2.0, 3.0, 4.0, 5.0 };
        var result = PercentileCalculator.CalculatePercentile(sorted, 75);
        result.Should().Be(4.0);
    }

    [Fact]
    public void CalculatePercentiles_ReturnsAllRequestedPercentiles()
    {
        var scores = new List<double> { 5.0, 1.0, 3.0, 2.0, 4.0 };
        var percentiles = new List<double> { 0, 25, 50, 75, 100 };

        var result = PercentileCalculator.CalculatePercentiles(scores, percentiles);

        result.Should().HaveCount(5);
        result[0].Should().Be(1.0);
        result[50].Should().Be(3.0);
        result[100].Should().Be(5.0);
    }

    [Fact]
    public void CalculatePercentiles_WithEmptyScores_ReturnsEmptyDictionary()
    {
        var result = PercentileCalculator.CalculatePercentiles(
            new List<double>(), new List<double> { 50 });

        result.Should().BeEmpty();
    }

    [Fact]
    public void CalculatePercentile_WithTwoElements_Interpolates()
    {
        var sorted = new List<double> { 0.0, 10.0 };

        PercentileCalculator.CalculatePercentile(sorted, 0).Should().Be(0.0);
        PercentileCalculator.CalculatePercentile(sorted, 50).Should().Be(5.0);
        PercentileCalculator.CalculatePercentile(sorted, 100).Should().Be(10.0);
    }
}
