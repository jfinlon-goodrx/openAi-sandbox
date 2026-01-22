using System.Net;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Shared.Common;

/// <summary>
/// API Key authentication handler
/// </summary>
public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private const string ApiKeyHeaderName = "X-API-Key";
    private readonly string _validApiKey;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IConfiguration configuration)
        : base(options, logger, encoder, clock)
    {
        _validApiKey = configuration["ApiKeys:Default"] ?? throw new InvalidOperationException("ApiKeys:Default not configured");
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue(ApiKeyHeaderName, out var apiKeyHeaderValues))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var providedApiKey = apiKeyHeaderValues.ToString();

        if (string.IsNullOrWhiteSpace(providedApiKey))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        // In production, validate against database or key vault
        if (providedApiKey != _validApiKey)
        {
            return Task.FromResult(AuthenticateResult.Fail("Invalid API key"));
        }

        var claims = new[] { new Claim(ClaimTypes.Name, "api-user") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        Response.Headers["WWW-Authenticate"] = $"ApiKey realm=\"{Options.Challenge}\"";
        return Task.CompletedTask;
    }
}

/// <summary>
/// Extension methods for API key authentication
/// </summary>
public static class ApiKeyAuthenticationExtensions
{
    /// <summary>
    /// Adds API key authentication
    /// </summary>
    public static AuthenticationBuilder AddApiKeyAuthentication(
        this AuthenticationBuilder builder,
        IConfiguration configuration)
    {
        return builder.AddScheme<AuthenticationSchemeOptions, ApiKeyAuthenticationHandler>(
            "ApiKey",
            options => { },
            configuration);
    }
}
