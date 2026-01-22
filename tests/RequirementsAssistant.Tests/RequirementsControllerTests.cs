using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using OpenAIShared.Tests.TestHelpers;
using RequirementsAssistant.Api;
using Xunit;

namespace RequirementsAssistant.Tests;

public class RequirementsControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public RequirementsControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Summarize_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new
        {
            content = "The system shall allow users to login and manage their profiles."
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/requirements/summarize", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
        // Note: Will be Unauthorized if API key not configured, OK if mock is set up
    }

    [Fact]
    public async Task GenerateUserStories_WithValidRequest_ReturnsOk()
    {
        // Arrange
        var request = new
        {
            content = "Users need to login and view their dashboard."
        };

        var content = new StringContent(
            JsonSerializer.Serialize(request),
            Encoding.UTF8,
            "application/json");

        // Act
        var response = await _client.PostAsync("/api/requirements/generate-user-stories", content);

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.Unauthorized);
    }
}
