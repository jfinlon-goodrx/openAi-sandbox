using FluentAssertions;
using OpenAIShared;
using Xunit;

namespace OpenAIShared.Tests;

public class CostCalculatorTests
{
    [Fact]
    public void CalculateCost_GPT4Turbo_ReturnsCorrectCost()
    {
        // Arrange
        var model = "gpt-4-turbo-preview";
        var promptTokens = 1000;
        var completionTokens = 500;

        // Act
        var cost = CostCalculator.CalculateCost(model, promptTokens, completionTokens);

        // Assert
        cost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateCost_GPT35Turbo_ReturnsCorrectCost()
    {
        // Arrange
        var model = "gpt-3.5-turbo";
        var promptTokens = 1000;
        var completionTokens = 500;

        // Act
        var cost = CostCalculator.CalculateCost(model, promptTokens, completionTokens);

        // Assert
        cost.Should().BeGreaterThan(0);
    }

    [Fact]
    public void CalculateCost_WithZeroTokens_ReturnsZero()
    {
        // Arrange
        var model = "gpt-4-turbo-preview";
        var promptTokens = 0;
        var completionTokens = 0;

        // Act
        var cost = CostCalculator.CalculateCost(model, promptTokens, completionTokens);

        // Assert
        cost.Should().Be(0);
    }

    [Fact]
    public void FormatCost_WithValidCost_ReturnsFormattedString()
    {
        // Arrange
        var cost = 0.001234m;

        // Act
        var formatted = CostCalculator.FormatCost(cost);

        // Assert
        formatted.Should().Contain("$");
        formatted.Should().Contain("0.001");
    }

    [Fact]
    public void CalculateCost_UnknownModel_ReturnsZero()
    {
        // Arrange
        var model = "unknown-model";
        var promptTokens = 1000;
        var completionTokens = 500;

        // Act
        var cost = CostCalculator.CalculateCost(model, promptTokens, completionTokens);

        // Assert
        cost.Should().Be(0);
    }
}
