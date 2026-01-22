using System.Text.Json;
using Microsoft.Extensions.Logging;
using OpenAIShared;

namespace DevOpsAssistant.Core;

/// <summary>
/// Service for DevOps workflows: log analysis, CI/CD optimization, infrastructure review, and more
/// </summary>
public class DevOpsService
{
    private readonly OpenAIClient _openAIClient;
    private readonly GitHubIntegration? _githubIntegration;
    private readonly ILogger<DevOpsService> _logger;
    private readonly string _model;

    public DevOpsService(
        OpenAIClient openAIClient,
        ILogger<DevOpsService> logger,
        GitHubIntegration? githubIntegration = null,
        string model = "gpt-4-turbo-preview")
    {
        _openAIClient = openAIClient;
        _logger = logger;
        _githubIntegration = githubIntegration;
        _model = model;
    }

    /// <summary>
    /// Analyzes application logs to identify issues and patterns
    /// </summary>
    public async Task<LogAnalysis> AnalyzeLogsAsync(
        string logs,
        string logType = "application",
        TimeSpan? timeRange = null,
        CancellationToken cancellationToken = default)
    {
        var timeRangeText = timeRange.HasValue 
            ? $"Time range: {timeRange.Value.TotalHours} hours"
            : "";

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert DevOps engineer specializing in log analysis and incident response.")
            .WithInstruction($"Analyze the following {logType} logs. {timeRangeText}\n\n" +
                           "Identify: error patterns, root causes, affected services, severity levels, " +
                           "and recommended remediation steps. Provide a structured analysis.")
            .WithInput(logs)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "system", Content = "You are an expert DevOps engineer." },
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var analysis = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new LogAnalysis
        {
            LogType = logType,
            Analysis = analysis,
            AnalyzedAt = DateTime.UtcNow,
            TimeRange = timeRange
        };
    }

    /// <summary>
    /// Generates incident report from log analysis
    /// </summary>
    public async Task<IncidentReport> GenerateIncidentReportAsync(
        LogAnalysis logAnalysis,
        string severity = "Medium",
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are generating a professional incident report.")
            .WithInstruction($"Generate an incident report based on the following log analysis. " +
                           $"Severity: {severity}\n\n" +
                           "Include: incident title, summary, root cause, impact assessment, " +
                           "timeline, affected systems, and remediation steps.")
            .WithInput(logAnalysis.Analysis)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var report = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new IncidentReport
        {
            Severity = severity,
            Report = report,
            GeneratedAt = DateTime.UtcNow,
            BasedOnAnalysis = logAnalysis
        };
    }

    /// <summary>
    /// Analyzes CI/CD pipeline performance
    /// </summary>
    public async Task<PipelineAnalysis> AnalyzePipelineAsync(
        string pipelineLogs,
        string pipelineType = "GitHub Actions",
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at optimizing CI/CD pipelines.")
            .WithInstruction($"Analyze the following {pipelineType} pipeline logs. " +
                           "Identify: execution times, bottlenecks, failed steps, " +
                           "resource usage, and optimization opportunities.")
            .WithInput(pipelineLogs)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var analysis = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new PipelineAnalysis
        {
            PipelineType = pipelineType,
            Analysis = analysis,
            AnalyzedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Optimizes CI/CD pipeline based on analysis
    /// </summary>
    public async Task<PipelineOptimization> OptimizePipelineAsync(
        PipelineAnalysis analysis,
        string[]? targetMetrics = null,
        CancellationToken cancellationToken = default)
    {
        var metricsText = targetMetrics != null && targetMetrics.Any()
            ? $"Target metrics: {string.Join(", ", targetMetrics)}"
            : "";

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at optimizing CI/CD pipelines.")
            .WithInstruction($"Based on the following pipeline analysis, provide optimization recommendations. {metricsText}\n\n" +
                           "Include: specific changes, expected improvements, implementation steps, " +
                           "and potential risks.")
            .WithInput(analysis.Analysis)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var optimization = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new PipelineOptimization
        {
            Recommendations = optimization,
            TargetMetrics = targetMetrics?.ToList() ?? new List<string>(),
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Reviews Infrastructure as Code for security and best practices
    /// </summary>
    public async Task<InfrastructureReview> ReviewInfrastructureCodeAsync(
        string code,
        string infrastructureType = "Terraform",
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at reviewing Infrastructure as Code.")
            .WithInstruction($"Review the following {infrastructureType} configuration. " +
                           "Check for: security vulnerabilities, best practices, cost optimization opportunities, " +
                           "compliance issues, and improvement suggestions.")
            .WithInput(code)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var review = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new InfrastructureReview
        {
            InfrastructureType = infrastructureType,
            Review = review,
            ReviewedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generates deployment script
    /// </summary>
    public async Task<DeploymentScript> GenerateDeploymentScriptAsync(
        string applicationType,
        string targetEnvironment,
        string deploymentMethod = "CI/CD",
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at creating deployment scripts and automation.")
            .WithInstruction($"Generate a deployment script for:\n" +
                           $"- Application Type: {applicationType}\n" +
                           $"- Target Environment: {targetEnvironment}\n" +
                           $"- Deployment Method: {deploymentMethod}\n\n" +
                           "Include: pre-deployment checks, deployment steps, post-deployment validation, " +
                           "rollback procedures, and error handling.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var script = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new DeploymentScript
        {
            ApplicationType = applicationType,
            TargetEnvironment = targetEnvironment,
            DeploymentMethod = deploymentMethod,
            Script = script,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generates runbook for operations
    /// </summary>
    public async Task<Runbook> GenerateRunbookAsync(
        string applicationName,
        List<string>? deploymentSteps = null,
        CancellationToken cancellationToken = default)
    {
        var stepsText = deploymentSteps != null && deploymentSteps.Any()
            ? $"Deployment Steps:\n{string.Join("\n", deploymentSteps.Select((s, i) => $"{i + 1}. {s}"))}"
            : "";

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are creating an operations runbook.")
            .WithInstruction($"Generate a comprehensive runbook for: {applicationName}\n\n{stepsText}\n\n" +
                           "Include: overview, prerequisites, deployment procedures, rollback procedures, " +
                           "troubleshooting guide, monitoring, and emergency contacts.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 3000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var runbook = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new Runbook
        {
            ApplicationName = applicationName,
            Content = runbook,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Analyzes metrics and performance data
    /// </summary>
    public async Task<MetricsAnalysis> AnalyzeMetricsAsync(
        string metrics,
        TimeSpan? timeRange = null,
        CancellationToken cancellationToken = default)
    {
        var timeRangeText = timeRange.HasValue
            ? $"Time range: {timeRange.Value.TotalDays} days"
            : "";

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at analyzing system metrics and performance data.")
            .WithInstruction($"Analyze the following metrics. {timeRangeText}\n\n" +
                           "Identify: trends, anomalies, performance bottlenecks, resource constraints, " +
                           "and optimization opportunities.")
            .WithInput(metrics)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var analysis = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new MetricsAnalysis
        {
            Analysis = analysis,
            TimeRange = timeRange,
            AnalyzedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Optimizes alerting rules based on metrics analysis
    /// </summary>
    public async Task<AlertingOptimization> OptimizeAlertingRulesAsync(
        string currentAlerts,
        MetricsAnalysis? metricsAnalysis = null,
        CancellationToken cancellationToken = default)
    {
        var metricsContext = metricsAnalysis != null
            ? $"Metrics Analysis:\n{metricsAnalysis.Analysis}\n\n"
            : "";

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at optimizing alerting and monitoring.")
            .WithInstruction($"{metricsContext}Analyze the following alerting rules and provide optimization recommendations. " +
                           "Consider: reducing false positives, improving signal-to-noise ratio, " +
                           "setting appropriate thresholds, and grouping related alerts.")
            .WithInput(currentAlerts)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var optimization = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new AlertingOptimization
        {
            Recommendations = optimization,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Scans infrastructure and applications for security issues
    /// </summary>
    public async Task<SecurityScan> ScanSecurityAsync(
        string scanTarget,
        List<string>? configFiles = null,
        CancellationToken cancellationToken = default)
    {
        var filesText = configFiles != null && configFiles.Any()
            ? $"Configuration Files:\n{string.Join("\n", configFiles)}"
            : "";

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are a security expert scanning infrastructure and applications.")
            .WithInstruction($"Perform a security scan on: {scanTarget}\n\n{filesText}\n\n" +
                           "Identify: security vulnerabilities, misconfigurations, compliance issues, " +
                           "and provide remediation recommendations.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var scanResults = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new SecurityScan
        {
            ScanTarget = scanTarget,
            Results = scanResults,
            ScannedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generates security report
    /// </summary>
    public async Task<SecurityReport> GenerateSecurityReportAsync(
        SecurityScan securityScan,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are generating a professional security report.")
            .WithInstruction("Generate a comprehensive security report based on the following scan results. " +
                           "Include: executive summary, detailed findings, risk assessment, " +
                           "remediation priorities, and compliance status.")
            .WithInput(securityScan.Results)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 3000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var report = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new SecurityReport
        {
            Report = report,
            BasedOnScan = securityScan,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Analyzes Dockerfile for best practices and security
    /// </summary>
    public async Task<DockerfileAnalysis> AnalyzeDockerfileAsync(
        string dockerfileContent,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at Docker and container security.")
            .WithInstruction("Analyze the following Dockerfile. Check for: security best practices, " +
                           "optimization opportunities, layer caching, multi-stage builds, " +
                           "and vulnerability reduction.")
            .WithInput(dockerfileContent)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var analysis = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new DockerfileAnalysis
        {
            Analysis = analysis,
            AnalyzedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Analyzes Kubernetes manifests
    /// </summary>
    public async Task<KubernetesAnalysis> AnalyzeKubernetesManifestsAsync(
        string manifestYaml,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at Kubernetes and container orchestration.")
            .WithInstruction("Analyze the following Kubernetes manifests. Check for: security configurations, " +
                           "resource limits, health checks, scaling configurations, " +
                           "and best practices.")
            .WithInput(manifestYaml)
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var analysis = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new KubernetesAnalysis
        {
            Analysis = analysis,
            AnalyzedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Analyzes GitHub Actions workflow and provides optimization recommendations
    /// </summary>
    public async Task<GitHubWorkflowAnalysis> AnalyzeGitHubWorkflowAsync(
        string owner,
        string repo,
        string workflowPath,
        CancellationToken cancellationToken = default)
    {
        if (_githubIntegration == null)
        {
            throw new InvalidOperationException("GitHub integration not configured");
        }

        // Get workflow file
        var workflowContent = await _githubIntegration.GetWorkflowFileAsync(owner, repo, workflowPath, cancellationToken);
        
        // Get recent workflow runs
        var workflowRuns = await _githubIntegration.GetWorkflowRunsAsync(owner, repo, limit: 10, cancellationToken);

        var runsSummary = string.Join("\n", workflowRuns.Select(r => 
            $"Run #{r.RunNumber}: {r.Status} - {r.Conclusion}, Duration: {r.UpdatedAt - r.CreatedAt}"));

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at analyzing and optimizing GitHub Actions workflows.")
            .WithInstruction($"Analyze the following GitHub Actions workflow file and recent execution data.\n\n" +
                           $"Workflow File:\n{workflowContent}\n\n" +
                           $"Recent Runs:\n{runsSummary}\n\n" +
                           "Provide: performance analysis, optimization opportunities, caching strategies, " +
                           "parallelization suggestions, and best practice recommendations.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var analysis = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new GitHubWorkflowAnalysis
        {
            Owner = owner,
            Repo = repo,
            WorkflowPath = workflowPath,
            Analysis = analysis,
            WorkflowRunsAnalyzed = workflowRuns.Count,
            AnalyzedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generates optimized GitHub Actions workflow file
    /// </summary>
    public async Task<OptimizedWorkflow> GenerateOptimizedWorkflowAsync(
        string currentWorkflow,
        PipelineOptimization optimization,
        CancellationToken cancellationToken = default)
    {
        var prompt = new PromptBuilder()
            .WithSystemMessage("You are an expert at creating optimized GitHub Actions workflows.")
            .WithInstruction($"Based on the following optimization recommendations, generate an improved GitHub Actions workflow file.\n\n" +
                           $"Current Workflow:\n{currentWorkflow}\n\n" +
                           $"Optimization Recommendations:\n{optimization.Recommendations}\n\n" +
                           "Generate a complete, optimized workflow YAML file with all improvements applied.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 3000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var optimizedWorkflow = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        // Extract YAML from markdown code blocks if present
        if (optimizedWorkflow.Contains("```yaml"))
        {
            var start = optimizedWorkflow.IndexOf("```yaml") + 7;
            var end = optimizedWorkflow.IndexOf("```", start);
            if (end > start)
            {
                optimizedWorkflow = optimizedWorkflow.Substring(start, end - start).Trim();
            }
        }
        else if (optimizedWorkflow.Contains("```"))
        {
            var start = optimizedWorkflow.IndexOf("```") + 3;
            var end = optimizedWorkflow.IndexOf("```", start);
            if (end > start)
            {
                optimizedWorkflow = optimizedWorkflow.Substring(start, end - start).Trim();
            }
        }

        return new OptimizedWorkflow
        {
            OriginalWorkflow = currentWorkflow,
            OptimizedWorkflow = optimizedWorkflow,
            OptimizationSummary = optimization.Summary,
            GeneratedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Analyzes pull request for deployment readiness
    /// </summary>
    public async Task<PrDeploymentAnalysis> AnalyzePrForDeploymentAsync(
        string owner,
        string repo,
        int prNumber,
        CancellationToken cancellationToken = default)
    {
        if (_githubIntegration == null)
        {
            throw new InvalidOperationException("GitHub integration not configured");
        }

        var pr = await _githubIntegration.GetPullRequestAsync(owner, repo, prNumber, cancellationToken);
        var files = await _githubIntegration.GetPullRequestFilesAsync(owner, repo, prNumber, cancellationToken);

        var filesSummary = string.Join("\n", files.Select(f => 
            $"{f.Filename}: {f.Status} (+{f.Additions}/-{f.Deletions})"));

        var prompt = new PromptBuilder()
            .WithSystemMessage("You are analyzing a pull request for deployment readiness.")
            .WithInstruction($"Analyze this pull request for deployment readiness:\n\n" +
                           $"Title: {pr.Title}\n" +
                           $"Description: {pr.Body ?? "No description"}\n" +
                           $"State: {pr.State}\n" +
                           $"Files Changed ({files.Count}):\n{filesSummary}\n\n" +
                           "Assess: deployment risk, breaking changes, migration needs, " +
                           "rollback considerations, and deployment checklist.")
            .Build();

        var request = new ChatCompletionRequest
        {
            Model = _model,
            Messages = new List<ChatMessage>
            {
                new() { Role = "user", Content = prompt }
            },
            Temperature = 0.3,
            MaxTokens = 2000
        };

        var response = await _openAIClient.GetChatCompletionAsync(request, cancellationToken);
        var analysis = response.Choices.FirstOrDefault()?.Message?.Content ?? "";

        return new PrDeploymentAnalysis
        {
            PrNumber = prNumber,
            PrTitle = pr.Title,
            Analysis = analysis,
            FilesChanged = files.Count,
            AnalyzedAt = DateTime.UtcNow
        };
    }
}

// Additional data models
public class GitHubWorkflowAnalysis
{
    public string Owner { get; set; } = string.Empty;
    public string Repo { get; set; } = string.Empty;
    public string WorkflowPath { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public int WorkflowRunsAnalyzed { get; set; }
    public DateTime AnalyzedAt { get; set; }
}

public class OptimizedWorkflow
{
    public string OriginalWorkflow { get; set; } = string.Empty;
    public string OptimizedWorkflow { get; set; } = string.Empty;
    public string OptimizationSummary { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

public class PrDeploymentAnalysis
{
    public int PrNumber { get; set; }
    public string PrTitle { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public int FilesChanged { get; set; }
    public DateTime AnalyzedAt { get; set; }
}

// Data models
public class LogAnalysis
{
    public string LogType { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }
    public TimeSpan? TimeRange { get; set; }
}

public class IncidentReport
{
    public string Severity { get; set; } = "Medium";
    public string Report { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public LogAnalysis? BasedOnAnalysis { get; set; }
    public string Title => Report.Split('\n').FirstOrDefault() ?? "Incident Report";
    public string Details => Report;
}

public class PipelineAnalysis
{
    public string PipelineType { get; set; } = string.Empty;
    public string Analysis { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }
}

public class PipelineOptimization
{
    public string Recommendations { get; set; } = string.Empty;
    public List<string> TargetMetrics { get; set; } = new();
    public DateTime GeneratedAt { get; set; }
    public string Summary => Recommendations.Split('\n').Take(5).Aggregate((a, b) => $"{a}\n{b}");
}

public class InfrastructureReview
{
    public string InfrastructureType { get; set; } = string.Empty;
    public string Review { get; set; } = string.Empty;
    public DateTime ReviewedAt { get; set; }
    public List<string> SecurityIssues => ParseIssues(Review, "Security");
    public List<string> CostOptimizations => ParseIssues(Review, "Cost");

    private List<string> ParseIssues(string review, string category)
    {
        // Simplified parsing - in production, use structured output
        return review.Split('\n')
            .Where(line => line.Contains(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
}

public class DeploymentScript
{
    public string ApplicationType { get; set; } = string.Empty;
    public string TargetEnvironment { get; set; } = string.Empty;
    public string DeploymentMethod { get; set; } = string.Empty;
    public string Script { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
    public List<string> Steps => Script.Split('\n')
        .Where(line => line.Trim().StartsWith("-") || char.IsDigit(line.Trim()[0]))
        .Select(line => line.Trim().TrimStart('-', '1', '2', '3', '4', '5', '6', '7', '8', '9', '0', '.', ' '))
        .Where(line => !string.IsNullOrWhiteSpace(line))
        .ToList();
}

public class Runbook
{
    public string ApplicationName { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

public class MetricsAnalysis
{
    public string Analysis { get; set; } = string.Empty;
    public TimeSpan? TimeRange { get; set; }
    public DateTime AnalyzedAt { get; set; }
}

public class AlertingOptimization
{
    public string Recommendations { get; set; } = string.Empty;
    public DateTime GeneratedAt { get; set; }
}

public class SecurityScan
{
    public string ScanTarget { get; set; } = string.Empty;
    public string Results { get; set; } = string.Empty;
    public DateTime ScannedAt { get; set; }
}

public class SecurityReport
{
    public string Report { get; set; } = string.Empty;
    public SecurityScan? BasedOnScan { get; set; }
    public DateTime GeneratedAt { get; set; }
}

public class DockerfileAnalysis
{
    public string Analysis { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }
}

public class KubernetesAnalysis
{
    public string Analysis { get; set; } = string.Empty;
    public DateTime AnalyzedAt { get; set; }
}
