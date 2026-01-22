# Getting Started - Setup Guide

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 8.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))
- **Visual Studio 2022**, **VS Code**, or **Rider** (recommended IDE)
- **OpenAI API Key** (Enterprise subscription)
- **Git** (for version control)

## Step 1: Clone and Setup

1. Clone the repository:
```bash
git clone <repository-url>
cd openAi-sandbox
```

2. Restore NuGet packages:
```bash
dotnet restore
```

3. Build the solution:
```bash
dotnet build
```

## Step 2: Configure OpenAI API Key

### Option 1: Environment Variables (Recommended)

Set your OpenAI API key as an environment variable:

**Windows (PowerShell):**
```powershell
$env:OpenAI__ApiKey = "your-api-key-here"
```

**macOS/Linux:**
```bash
export OpenAI__ApiKey="your-api-key-here"
```

### Option 2: User Secrets (Development)

For local development, use .NET User Secrets:

```bash
cd src/RequirementsAssistant/RequirementsAssistant.Api
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "your-api-key-here"
```

### Option 3: appsettings.json (Not Recommended for Production)

Add your API key to `appsettings.json`:

```json
{
  "OpenAI": {
    "ApiKey": "your-api-key-here",
    "BaseUrl": "https://api.openai.com/v1",
    "DefaultModel": "gpt-4-turbo-preview"
  }
}
```

**⚠️ Warning:** Never commit API keys to version control. Add `appsettings.json` to `.gitignore` if storing keys there.

## Step 3: Verify Installation

Run a simple test to verify your setup:

```bash
cd src/RequirementsAssistant/RequirementsAssistant.Api
dotnet run
```

Navigate to `https://localhost:7001/swagger` to see the API documentation.

## Step 4: Configure Integrations (Optional)

### Confluence Integration

If you want to use Confluence integration for Requirements Assistant:

1. Get your Confluence API token from [Atlassian Account Settings](https://id.atlassian.com/manage-profile/security/api-tokens)
2. Configure in `appsettings.json`:

```json
{
  "Confluence": {
    "BaseUrl": "https://your-domain.atlassian.net/wiki",
    "Username": "your-email@example.com",
    "ApiToken": "your-api-token"
  }
}
```

### Jira Integration

For Retro Analyzer and SDM Assistant to create Jira tickets:

1. Get your Jira API token from [Atlassian Account Settings](https://id.atlassian.com/manage-profile/security/api-tokens)
2. Configure in `appsettings.json`:

```json
{
  "Jira": {
    "BaseUrl": "https://your-domain.atlassian.net",
    "Username": "your-email@example.com",
    "ApiToken": "your-api-token",
    "ProjectKey": "PROJ"
  }
}
```

**Note:** SDM Assistant requires Jira integration for daily summaries, sprint planning, and risk identification. See [SDM Guide](../role-guides/sdm-guide.md) for detailed setup.

### GitHub Actions

For automated code reviews:

1. Add `OPENAI_API_KEY` to your GitHub repository secrets
2. The workflow file is already configured at `.github/workflows/code-review.yml`

## Next Steps

- Read [API Basics](02-api-basics.md) to understand how to make your first API call
- Check out [Prompt Engineering](03-prompt-engineering.md) for best practices
- Build your [First Project](04-first-project.md)
- Explore [Role Guides](../role-guides/) for role-specific workflows:
  - [Developer Guide](../role-guides/developer-guide.md)
  - [Business Analyst Guide](../role-guides/ba-guide.md)
  - [Product Manager Guide](../role-guides/pm-guide.md)
  - [Tester Guide](../role-guides/tester-guide.md)
  - [Software Development Manager Guide](../role-guides/sdm-guide.md) ⭐ NEW
  - [DevOps Engineer Guide](../role-guides/devops-guide.md) ⭐ NEW

## Troubleshooting

### Common Issues

**Issue:** "API key not found"
- **Solution:** Ensure your API key is configured correctly using one of the methods above

**Issue:** "Rate limit exceeded"
- **Solution:** You may be hitting OpenAI rate limits. Check your usage in the OpenAI dashboard

**Issue:** "Connection timeout"
- **Solution:** Check your network connection and firewall settings. Ensure `api.openai.com` is accessible

**Issue:** Build errors
- **Solution:** Ensure you have .NET 8.0 SDK installed: `dotnet --version`

## Additional Resources

- [OpenAI Platform Documentation](https://platform.openai.com/docs)
- [.NET Documentation](https://learn.microsoft.com/dotnet/)
- [OpenAI API Reference](https://platform.openai.com/docs/api-reference)
