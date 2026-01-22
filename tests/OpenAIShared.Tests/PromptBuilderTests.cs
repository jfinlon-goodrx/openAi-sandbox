using FluentAssertions;
using OpenAIShared;
using Xunit;

namespace OpenAIShared.Tests;

public class PromptBuilderTests
{
    [Fact]
    public void Build_WithSystemMessage_IncludesSystemMessage()
    {
        // Arrange
        var builder = new PromptBuilder()
            .WithSystemMessage("You are a helpful assistant.");

        // Act
        var prompt = builder.Build();

        // Assert
        prompt.Should().Contain("You are a helpful assistant.");
    }

    [Fact]
    public void Build_WithInstruction_IncludesInstruction()
    {
        // Arrange
        var builder = new PromptBuilder()
            .WithInstruction("Summarize the following text.");

        // Act
        var prompt = builder.Build();

        // Assert
        prompt.Should().Contain("Summarize the following text.");
    }

    [Fact]
    public void Build_WithInput_IncludesInput()
    {
        // Arrange
        var builder = new PromptBuilder()
            .WithInput("Test input text");

        // Act
        var prompt = builder.Build();

        // Assert
        prompt.Should().Contain("Test input text");
    }

    [Fact]
    public void Build_WithAllComponents_IncludesAll()
    {
        // Arrange
        var builder = new PromptBuilder()
            .WithSystemMessage("You are a helpful assistant.")
            .WithInstruction("Summarize the text.")
            .WithInput("Long text to summarize...");

        // Act
        var prompt = builder.Build();

        // Assert
        prompt.Should().Contain("You are a helpful assistant.");
        prompt.Should().Contain("Summarize the text.");
        prompt.Should().Contain("Long text to summarize...");
    }

    [Fact]
    public void Build_WithExample_IncludesExample()
    {
        // Arrange
        var builder = new PromptBuilder()
            .WithExample("Input: Hello", "Output: Hi there!");

        // Act
        var prompt = builder.Build();

        // Assert
        prompt.Should().Contain("Hello");
        prompt.Should().Contain("Hi there!");
    }

    [Fact]
    public void Build_WithContext_IncludesContext()
    {
        // Arrange
        var builder = new PromptBuilder()
            .WithContext("Additional context information");

        // Act
        var prompt = builder.Build();

        // Assert
        prompt.Should().Contain("Additional context information");
    }
}
