using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace Shared.Integrations;

/// <summary>
/// Slack integration for sending notifications and messages
/// </summary>
public class SlackIntegration
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SlackIntegration> _logger;
    private readonly string _webhookUrl;

    public SlackIntegration(
        HttpClient httpClient,
        ILogger<SlackIntegration> logger,
        string webhookUrl)
    {
        _httpClient = httpClient;
        _logger = logger;
        _webhookUrl = webhookUrl;
    }

    /// <summary>
    /// Sends a simple text message to Slack
    /// </summary>
    public async Task SendMessageAsync(
        string text,
        string? channel = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var payload = new
            {
                text = text,
                channel = channel
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_webhookUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending Slack message");
            throw;
        }
    }

    /// <summary>
    /// Sends a formatted Slack message with blocks
    /// </summary>
    public async Task SendFormattedMessageAsync(
        string title,
        string text,
        string? color = null,
        List<SlackField>? fields = null,
        string? channel = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var blocks = new List<object>
            {
                new
                {
                    type = "header",
                    text = new
                    {
                        type = "plain_text",
                        text = title
                    }
                },
                new
                {
                    type = "section",
                    text = new
                    {
                        type = "mrkdwn",
                        text = text
                    }
                }
            };

            if (fields != null && fields.Any())
            {
                blocks.Add(new
                {
                    type = "section",
                    fields = fields.Select(f => new
                    {
                        type = "mrkdwn",
                        text = $"*{f.Title}*\n{f.Value}"
                    }).ToArray()
                });
            }

            var payload = new
            {
                channel = channel,
                blocks = blocks,
                attachments = new[]
                {
                    new
                    {
                        color = color ?? "good",
                        blocks = new object[0]
                    }
                }
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(_webhookUrl, content, cancellationToken);
            response.EnsureSuccessStatusCode();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending formatted Slack message");
            throw;
        }
    }

    /// <summary>
    /// Sends an incident report to Slack
    /// </summary>
    public async Task SendIncidentReportAsync(
        string title,
        string severity,
        string summary,
        List<string>? affectedSystems = null,
        List<string>? remediationSteps = null,
        string? channel = null,
        CancellationToken cancellationToken = default)
    {
        var color = severity.ToLower() switch
        {
            "critical" or "high" => "danger",
            "medium" => "warning",
            _ => "good"
        };

        var fields = new List<SlackField>
        {
            new() { Title = "Severity", Value = severity }
        };

        if (affectedSystems != null && affectedSystems.Any())
        {
            fields.Add(new SlackField
            {
                Title = "Affected Systems",
                Value = string.Join(", ", affectedSystems)
            });
        }

        if (remediationSteps != null && remediationSteps.Any())
        {
            fields.Add(new SlackField
            {
                Title = "Remediation Steps",
                Value = string.Join("\n", remediationSteps.Select((s, i) => $"{i + 1}. {s}"))
            });
        }

        await SendFormattedMessageAsync(
            title: $"🚨 Incident: {title}",
            text: summary,
            color: color,
            fields: fields,
            channel: channel,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Sends a daily summary to Slack
    /// </summary>
    public async Task SendDailySummaryAsync(
        string summary,
        Dictionary<string, string>? metrics = null,
        string? channel = null,
        CancellationToken cancellationToken = default)
    {
        var fields = metrics?.Select(m => new SlackField
        {
            Title = m.Key,
            Value = m.Value
        }).ToList();

        await SendFormattedMessageAsync(
            title: $"📊 Daily Summary - {DateTime.Now:yyyy-MM-dd}",
            text: summary,
            color: "good",
            fields: fields,
            channel: channel,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Sends standup talking points to Slack
    /// </summary>
    public async Task SendStandupPointsAsync(
        List<string> talkingPoints,
        string? channel = null,
        CancellationToken cancellationToken = default)
    {
        var pointsText = string.Join("\n", talkingPoints.Select((p, i) => $"{i + 1}. {p}"));

        await SendFormattedMessageAsync(
            title: "📝 Standup Talking Points",
            text: pointsText,
            color: "good",
            channel: channel,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Sends deployment notification to Slack
    /// </summary>
    public async Task SendDeploymentNotificationAsync(
        string applicationName,
        string environment,
        string version,
        string status,
        string? details = null,
        string? channel = null,
        CancellationToken cancellationToken = default)
    {
        var color = status.ToLower() switch
        {
            "success" => "good",
            "failure" => "danger",
            _ => "warning"
        };

        var emoji = status.ToLower() switch
        {
            "success" => "✅",
            "failure" => "❌",
            _ => "⚠️"
        };

        var fields = new List<SlackField>
        {
            new() { Title = "Application", Value = applicationName },
            new() { Title = "Environment", Value = environment },
            new() { Title = "Version", Value = version },
            new() { Title = "Status", Value = status }
        };

        await SendFormattedMessageAsync(
            title: $"{emoji} Deployment: {applicationName}",
            text: details ?? $"Deployment to {environment} completed with status: {status}",
            color: color,
            fields: fields,
            channel: channel,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Sends PR notification to Slack
    /// </summary>
    public async Task SendPrNotificationAsync(
        string repository,
        int prNumber,
        string title,
        string author,
        string status,
        string? prUrl = null,
        string? channel = null,
        CancellationToken cancellationToken = default)
    {
        var color = status.ToLower() switch
        {
            "merged" => "good",
            "closed" => "warning",
            "open" => "#36a64f",
            _ => "good"
        };

        var fields = new List<SlackField>
        {
            new() { Title = "Repository", Value = repository },
            new() { Title = "Author", Value = author },
            new() { Title = "Status", Value = status }
        };

        var text = prUrl != null 
            ? $"<{prUrl}|PR #{prNumber}: {title}>"
            : $"PR #{prNumber}: {title}";

        await SendFormattedMessageAsync(
            title: $"🔀 Pull Request: {title}",
            text: text,
            color: color,
            fields: fields,
            channel: channel,
            cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Sends pipeline status to Slack
    /// </summary>
    public async Task SendPipelineStatusAsync(
        string pipelineName,
        string status,
        string? conclusion = null,
        string? runUrl = null,
        Dictionary<string, string>? details = null,
        string? channel = null,
        CancellationToken cancellationToken = default)
    {
        var color = (conclusion ?? status).ToLower() switch
        {
            "success" => "good",
            "failure" or "cancelled" => "danger",
            "in_progress" => "warning",
            _ => "good"
        };

        var emoji = (conclusion ?? status).ToLower() switch
        {
            "success" => "✅",
            "failure" => "❌",
            "cancelled" => "🚫",
            "in_progress" => "🔄",
            _ => "ℹ️"
        };

        var fields = new List<SlackField>
        {
            new() { Title = "Pipeline", Value = pipelineName },
            new() { Title = "Status", Value = status }
        };

        if (!string.IsNullOrEmpty(conclusion))
        {
            fields.Add(new SlackField { Title = "Conclusion", Value = conclusion });
        }

        if (details != null)
        {
            foreach (var detail in details)
            {
                fields.Add(new SlackField { Title = detail.Key, Value = detail.Value });
            }
        }

        var text = runUrl != null
            ? $"<{runUrl}|View Pipeline Run>"
            : $"Pipeline {pipelineName} status: {status}";

        await SendFormattedMessageAsync(
            title: $"{emoji} Pipeline: {pipelineName}",
            text: text,
            color: color,
            fields: fields,
            channel: channel,
            cancellationToken: cancellationToken);
    }
}

public class SlackField
{
    public string Title { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
}
