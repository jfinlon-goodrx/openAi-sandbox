# DevOps Assistant

**For:** DevOps Engineers who want to automate infrastructure management, optimize CI/CD, and improve incident response.

**What you'll learn:** How to use AI for log analysis, incident response, CI/CD optimization, infrastructure as code review, deployment automation, security scanning, Docker/Kubernetes analysis, and monitoring optimization.

## Overview

The DevOps Assistant helps DevOps Engineers automate infrastructure management, optimize CI/CD pipelines, analyze logs, improve security, and streamline deployment processes using AI.

**Why use AI for DevOps workflows?**
- **Faster incident response:** Quickly analyze logs and identify root causes
- **Infrastructure optimization:** AI can suggest improvements to IaC and pipelines
- **Security:** Identify security issues in infrastructure code and deployments
- **Automation:** Generate runbooks, deployment scripts, and documentation automatically
- **Cost optimization:** Analyze usage patterns and suggest cost-saving measures
- **Integration:** Seamless GitHub Actions integration for CI/CD workflows

## Features

- **Log Analysis**: Analyze application and infrastructure logs to identify issues
- **Incident Response**: Generate incident reports from log analysis
- **CI/CD Optimization**: Analyze and optimize pipeline performance
- **Infrastructure Review**: Review Infrastructure as Code (Terraform, ARM, CloudFormation)
- **Deployment Automation**: Generate deployment scripts and runbooks
- **Metrics Analysis**: Analyze system metrics and performance data
- **Alerting Optimization**: Optimize monitoring and alerting rules
- **Security Scanning**: Scan infrastructure and applications for security issues
- **Container Analysis**: Analyze Dockerfiles and Kubernetes manifests

## Architecture

```
┌─────────────────┐
│   Web API       │
└────────┬────────┘
         │
┌────────▼────────┐
│  DevOps Service  │
└────────┬────────┘
         │
┌────────▼────────┐
│  OpenAI Client  │
│  (GPT-4)        │
└─────────────────┘
```

## Setup

1. Configure OpenAI API key (see [Setup Guide](../getting-started/01-setup.md))

2. Run the API:
```bash
cd src/DevOpsAssistant/DevOpsAssistant.Api
dotnet run
```

3. Navigate to `https://localhost:7007/swagger` for API documentation

## API Endpoints

### POST /api/devops/analyze-logs

Analyzes application logs to identify issues and patterns.

**Request:**
```json
{
  "logs": "Error logs content...",
  "logType": "application",
  "timeRangeHours": 1
}
```

**Response:**
```json
{
  "logType": "application",
  "analysis": "Analysis of logs...",
  "analyzedAt": "2024-01-15T10:00:00Z"
}
```

### POST /api/devops/incident-report

Generates incident report from log analysis.

**Request:**
```json
{
  "logAnalysis": { /* LogAnalysis object */ },
  "severity": "High"
}
```

### POST /api/devops/analyze-pipeline

Analyzes CI/CD pipeline performance.

**Request:**
```json
{
  "pipelineLogs": "Pipeline execution logs...",
  "pipelineType": "GitHub Actions"
}
```

### POST /api/devops/optimize-pipeline

Optimizes CI/CD pipeline based on analysis.

**Request:**
```json
{
  "pipelineAnalysis": { /* PipelineAnalysis object */ },
  "targetMetrics": ["build_time", "deployment_time"]
}
```

### POST /api/devops/review-infrastructure

Reviews Infrastructure as Code.

**Request:**
```json
{
  "code": "terraform { ... }",
  "infrastructureType": "Terraform"
}
```

### POST /api/devops/deployment-script

Generates deployment script.

**Request:**
```json
{
  "applicationType": "ASP.NET Core",
  "targetEnvironment": "Azure App Service",
  "deploymentMethod": "CI/CD"
}
```

### POST /api/devops/runbook

Generates operations runbook.

**Request:**
```json
{
  "applicationName": "MyApp",
  "deploymentSteps": ["Step 1", "Step 2"]
}
```

### POST /api/devops/analyze-metrics

Analyzes system metrics.

**Request:**
```json
{
  "metrics": "CPU: 80%, Memory: 60%...",
  "timeRangeDays": 7
}
```

### POST /api/devops/optimize-alerts

Optimizes alerting rules.

**Request:**
```json
{
  "currentAlerts": "Alert rules configuration...",
  "metricsAnalysis": { /* MetricsAnalysis object */ }
}
```

### POST /api/devops/security-scan

Scans for security issues.

**Request:**
```json
{
  "scanTarget": "infrastructure",
  "configFiles": ["terraform.tf", "docker-compose.yml"]
}
```

### POST /api/devops/analyze-dockerfile

Analyzes Dockerfile.

**Request:**
```json
{
  "dockerfileContent": "FROM node:18..."
}
```

### POST /api/devops/analyze-kubernetes

Analyzes Kubernetes manifests.

**Request:**
```json
{
  "manifestYaml": "apiVersion: v1..."
}
```

## Example Usage

### Log Analysis and Incident Response

```csharp
var devOpsService = new DevOpsService(openAIClient, logger);

// Analyze logs
var logAnalysis = await devOpsService.AnalyzeLogsAsync(
    logs: errorLogs,
    logType: "application",
    timeRange: TimeSpan.FromHours(1)
);

// Generate incident report
var incidentReport = await devOpsService.GenerateIncidentReportAsync(
    logAnalysis,
    severity: "High"
);

Console.WriteLine($"Incident: {incidentReport.Title}");
Console.WriteLine(incidentReport.Details);
```

### CI/CD Pipeline Optimization

```csharp
// Analyze pipeline
var pipelineAnalysis = await devOpsService.AnalyzePipelineAsync(
    pipelineLogs: githubActionsLogs,
    pipelineType: "GitHub Actions"
);

// Optimize
var optimization = await devOpsService.OptimizePipelineAsync(
    pipelineAnalysis,
    targetMetrics: new[] { "build_time", "deployment_time" }
);

Console.WriteLine(optimization.Recommendations);
```

### Infrastructure Review

```csharp
// Review Terraform
var review = await devOpsService.ReviewInfrastructureCodeAsync(
    code: terraformConfig,
    infrastructureType: "Terraform"
);

Console.WriteLine($"Security Issues: {review.SecurityIssues.Count}");
Console.WriteLine($"Cost Optimizations: {review.CostOptimizations.Count}");
```

### Security Scanning

```csharp
// Security scan
var scan = await devOpsService.ScanSecurityAsync(
    scanTarget: "infrastructure",
    configFiles: new[] { "terraform.tf", "docker-compose.yml" }
);

// Generate report
var report = await devOpsService.GenerateSecurityReportAsync(scan);
Console.WriteLine(report.Report);
```

## Integration Examples

### GitHub Actions Integration

```csharp
// Setup GitHub integration
var githubIntegration = new GitHubIntegration(httpClient, logger, githubToken);
var devOpsService = new DevOpsService(openAIClient, logger, githubIntegration);

// Analyze GitHub Actions workflow
var workflowAnalysis = await devOpsService.AnalyzeGitHubWorkflowAsync(
    owner: "your-org",
    repo: "your-repo",
    workflowPath: ".github/workflows/ci.yml"
);

// Get workflow runs
var runs = await githubIntegration.GetWorkflowRunsAsync("your-org", "your-repo", limit: 10);

// Analyze pipeline performance
var pipelineAnalysis = await devOpsService.AnalyzePipelineAsync(
    JsonSerializer.Serialize(runs),
    "GitHub Actions"
);

// Optimize and generate improved workflow
var optimization = await devOpsService.OptimizePipelineAsync(pipelineAnalysis);
var optimized = await devOpsService.GenerateOptimizedWorkflowAsync(
    currentWorkflow: await githubIntegration.GetWorkflowFileAsync("your-org", "your-repo", ".github/workflows/ci.yml"),
    optimization: optimization
);

// Create PR with optimizations
await githubIntegration.CreatePullRequestAsync(
    owner: "your-org",
    repo: "your-repo",
    title: "Pipeline Optimizations",
    body: optimization.Summary,
    head: "pipeline-optimization",
    @base: "main"
);
```

### PR Deployment Readiness

```csharp
// Analyze PR before deployment
var prAnalysis = await devOpsService.AnalyzePrForDeploymentAsync(
    owner: "your-org",
    repo: "your-repo",
    prNumber: 123
);

// Check deployment readiness
Console.WriteLine($"Files Changed: {prAnalysis.FilesChanged}");
Console.WriteLine($"Analysis: {prAnalysis.Analysis}");
```

See [GitHub Actions Workflow Examples](../../samples/GitHubExamples/GitHubActionsWorkflows.md) for complete workflow templates.

### Azure DevOps Integration

```csharp
// Analyze Azure DevOps pipeline
var pipelineAnalysis = await devOpsService.AnalyzePipelineAsync(
    pipelineLogs: azureDevOpsLogs,
    pipelineType: "Azure DevOps"
);
```

## Best Practices

1. **Regular Log Analysis**: Set up automated log analysis for proactive issue detection
2. **Pipeline Optimization**: Regularly review and optimize CI/CD pipelines
3. **Security First**: Regular security scans of infrastructure and applications
4. **Documentation**: Auto-generate runbooks and deployment documentation
5. **Metrics Monitoring**: Continuous metrics analysis and alerting optimization

## Troubleshooting

**Issue:** Log analysis too slow for large volumes
- **Solution:** Use sampling, batch processing, or time-range filtering

**Issue:** Pipeline optimization suggestions not applicable
- **Solution:** Provide more context about infrastructure constraints and requirements

**Issue:** Security scan false positives
- **Solution:** Fine-tune prompts with your specific security policies and standards

## Resources

- [DevOps Guide](../role-guides/devops-guide.md) - Comprehensive DevOps workflows
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Azure DevOps Documentation](https://docs.microsoft.com/azure/devops/)
- [Terraform Documentation](https://www.terraform.io/docs)
