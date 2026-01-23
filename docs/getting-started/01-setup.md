# Getting Started - Setup Guide

**For:** Developers who want to build and run .NET applications

**Not a developer?** See [Quick Start for Non-Developers](00-non-developer-setup.md) - Use REST APIs without any development environment!

This guide will walk you through setting up a full development environment to build and run the .NET projects in this portfolio.

## What You'll Learn

- How to set up your development environment (or use REST APIs without one)
- How to configure your OpenAI API key securely
- How to set up optional integrations (Jira, Confluence, GitHub, Slack)
- How to verify everything is working correctly

## Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 8.0 SDK** or later ([Download](https://dotnet.microsoft.com/download))
  - *What is this?* A toolkit for building and running .NET applications
  - *Do I need it?* Only if you want to build and run the projects. You can use REST APIs without it.
- **Visual Studio 2022**, **VS Code**, or **Rider** (recommended IDE)
  - *What is this?* A code editor/development environment
  - *Do I need it?* Only if you're modifying code. You can use REST APIs with curl, Python, or Postman instead.
- **OpenAI API Key** (Enterprise subscription recommended)
  - *What is this?* A secret key that allows you to use OpenAI's services
  - *How do I get it?* Sign up at [platform.openai.com](https://platform.openai.com) and create an API key
- **Git** (for version control)
  - *What is this?* A tool for tracking changes to files
  - *Do I need it?* Only if you want to clone the repository. You can download it as a ZIP file instead.

**Don't have these?** No problem! See [Option 1: REST API Examples](../README.md#option-1-rest-api-no-development-environment-required) to use the APIs without any development setup.

## Step 1: Get the Code

### Option A: Clone with Git (Recommended)

1. Clone the repository:
```bash
git clone https://github.com/jfinlon-goodrx/openAi-sandbox.git
cd openAi-sandbox
```

**What this does:** Downloads all the project files to your computer.

### Option B: Download as ZIP

1. Go to [GitHub Repository](https://github.com/jfinlon-goodrx/openAi-sandbox)
2. Click "Code" → "Download ZIP"
3. Extract the ZIP file to a folder
4. Open a terminal/command prompt in that folder

**What this does:** Same as cloning, but without needing Git installed.

## Step 2: Restore Dependencies

**What are dependencies?** Pre-built code libraries that the projects need to run. Think of them as ingredients for a recipe.

```bash
dotnet restore
```

**What this does:** Downloads all required packages from the internet.

**Expected output:** You should see packages being downloaded. This may take a few minutes the first time.

## Step 3: Build the Solution

**What is building?** Converting source code into executable programs.

```bash
dotnet build
```

**What this does:** Compiles all projects and checks for errors.

**Expected output:** "Build succeeded" message. If you see errors, check the Troubleshooting section below.

## Step 4: Configure OpenAI API Key

**What is an API key?** A secret password that identifies you to OpenAI's servers and allows you to use their services. Keep it secure and never share it publicly.

**Why do I need it?** All the projects in this portfolio use OpenAI's AI models, which require authentication.

### Option 1: Environment Variables (Recommended for Production)

**What are environment variables?** Settings stored in your computer's environment that applications can read. More secure than storing in files.

**Why recommended?** Keeps your API key out of code files, reducing the risk of accidentally sharing it.

**Windows (PowerShell):**
```powershell
$env:OpenAI__ApiKey = "sk-your-actual-api-key-here"
```

**Windows (Command Prompt):**
```cmd
set OpenAI__ApiKey=sk-your-actual-api-key-here
```

**macOS/Linux:**
```bash
export OpenAI__ApiKey="sk-your-actual-api-key-here"
```

**To make it permanent** (so you don't have to set it every time):

**macOS/Linux:** Add to `~/.bashrc` or `~/.zshrc`:
```bash
echo 'export OpenAI__ApiKey="sk-your-actual-api-key-here"' >> ~/.zshrc
source ~/.zshrc
```

**Windows:** Use System Properties → Environment Variables to add it permanently.

### Option 2: User Secrets (Recommended for Development)

**What are User Secrets?** A secure way to store sensitive data (like API keys) during development. They're stored outside your project folder and never committed to version control.

**Why use this?** Safer than storing in `appsettings.json` and easier than environment variables for development.

**Setup:**
```bash
cd src/RequirementsAssistant/RequirementsAssistant.Api
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-actual-api-key-here"
```

**What this does:**
- `init`: Creates a secrets file for this project
- `set`: Stores your API key securely

**Verify it's set:**
```bash
dotnet user-secrets list
```

You should see your API key listed (it will be partially hidden for security).

### Option 3: appsettings.json (Not Recommended - Use Only for Quick Testing)

**What is appsettings.json?** A configuration file that stores application settings.

**Why not recommended?** If you accidentally commit this file to Git, your API key could be exposed publicly.

**When to use:** Only for quick testing when you're certain the file won't be committed.

**Setup:**
1. Navigate to a project folder, e.g., `src/RequirementsAssistant/RequirementsAssistant.Api/`
2. Open or create `appsettings.json`
3. Add your configuration:

```json
{
  "OpenAI": {
    "ApiKey": "sk-your-actual-api-key-here",
    "BaseUrl": "https://api.openai.com/v1",
    "DefaultModel": "gpt-4-turbo-preview",
    "MaxRetries": 3,
    "TimeoutSeconds": 60,
    "EnableLogging": true
  }
}
```

**⚠️ Critical Security Warning:**
- **Never commit this file** if it contains your API key
- The `.gitignore` file should already exclude `appsettings.json`, but double-check
- If you've already committed it with a key, rotate your API key immediately in the OpenAI dashboard
- Prefer User Secrets (Option 2) or Environment Variables (Option 1) instead

## Step 5: Verify Installation

**What does verification do?** Confirms that everything is set up correctly and the application can start.

**Steps:**

1. Navigate to a project directory:
```bash
cd src/RequirementsAssistant/RequirementsAssistant.Api
```

2. Run the application:
```bash
dotnet run
```

**What this does:** Starts the web server and makes the API available.

**Expected output:**
- You should see messages like "Now listening on: https://localhost:7001"
- If you see errors about missing API keys, go back to Step 3

3. Open your web browser and navigate to:
```
https://localhost:7001/swagger
```

**What is Swagger?** An interactive API documentation tool that lets you see all available endpoints and test them directly in your browser.

**What you should see:**
- A web page showing all available API endpoints
- You can expand endpoints to see their details
- You can click "Try it out" to test endpoints directly

**If Swagger doesn't load:**
- Check that the server is running (look for "Now listening" messages)
- Try `http://localhost:5001/swagger` (HTTP instead of HTTPS)
- Check for firewall or security software blocking the connection

## Step 6: Configure Integrations (Optional)

**What are integrations?** Connections to other tools you already use (like Jira, Confluence, Slack, GitHub). These are optional - you can use the projects without them, but they add powerful automation capabilities.

**When to set these up:**
- **Jira**: If you want to automatically create tickets from action items or retrospectives
- **Confluence**: If you want to pull requirements documents or create documentation pages
- **GitHub**: If you want automated code reviews or CI/CD workflows
- **Slack**: If you want notifications and updates sent to your team channels

**Note:** Each integration requires an API token or webhook URL from the respective service. See individual setup instructions below.

### Confluence Integration

**What is Confluence?** Atlassian's documentation and collaboration platform. Many teams use it to store requirements, meeting notes, and project documentation.

**What does this integration do?** Allows the Requirements Assistant to automatically pull documents from Confluence and process them with AI.

**Setup Steps:**

1. **Get your Confluence API token:**
   - Go to [Atlassian Account Settings](https://id.atlassian.com/manage-profile/security/api-tokens)
   - Click "Create API token"
   - Give it a name (e.g., "OpenAI Portfolio")
   - Copy the token (you'll only see it once!)

2. **Find your Confluence base URL:**
   - Usually: `https://your-domain.atlassian.net/wiki`
   - Or: `https://your-domain.atlassian.net/confluence`

3. **Configure using User Secrets** (recommended):
```bash
cd src/RequirementsAssistant/RequirementsAssistant.Api
dotnet user-secrets set "Confluence:BaseUrl" "https://your-domain.atlassian.net/wiki"
dotnet user-secrets set "Confluence:Username" "your-email@example.com"
dotnet user-secrets set "Confluence:ApiToken" "your-api-token-here"
dotnet user-secrets set "Confluence:DefaultSpace" "ENG"  # Optional: default space key
```

**Or configure in `appsettings.json`:**

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

**What is Jira?** Atlassian's project management and issue tracking tool. Used for managing tasks, bugs, user stories, and sprints.

**What does this integration do?** 
- Retro Analyzer: Automatically creates Jira tickets from action items identified in retrospectives
- SDM Assistant: Pulls sprint data, creates tickets, and analyzes team velocity

**Setup Steps:**

1. **Get your Jira API token:**
   - Go to [Atlassian Account Settings](https://id.atlassian.com/manage-profile/security/api-tokens)
   - Click "Create API token"
   - Give it a name (e.g., "OpenAI Portfolio")
   - Copy the token

2. **Find your Jira base URL:**
   - Usually: `https://your-domain.atlassian.net`

3. **Find your project key:**
   - In Jira, go to your project
   - The project key is usually visible in the URL or project settings (e.g., "PROJ", "ENG")

4. **Configure using User Secrets** (recommended):
```bash
cd src/RetroAnalyzer/RetroAnalyzer.Api  # Or SDMAssistant.Api
dotnet user-secrets set "Jira:BaseUrl" "https://your-domain.atlassian.net"
dotnet user-secrets set "Jira:Username" "your-email@example.com"
dotnet user-secrets set "Jira:ApiToken" "your-api-token-here"
dotnet user-secrets set "Jira:ProjectKey" "PROJ"  # Your project key
```

**Or configure in `appsettings.json`:**

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

### GitHub Integration

**What is GitHub?** A platform for hosting code repositories and collaborating on software projects.

**What does this integration do?**
- **GitHub Actions**: Automatically reviews code when pull requests are created
- **DevOps Assistant**: Analyzes CI/CD pipelines, workflow runs, and deployment logs
- **Autonomous Development Agent**: Can create pull requests with code improvements

**Setup Steps:**

#### For GitHub Actions (Automated Code Reviews):

1. **Add OpenAI API key to GitHub Secrets:**
   - Go to your GitHub repository
   - Click "Settings" → "Secrets and variables" → "Actions"
   - Click "New repository secret"
   - Name: `OPENAI_API_KEY`
   - Value: Your OpenAI API key (starts with `sk-`)
   - Click "Add secret"

2. **The workflow is already configured:**
   - File: `.github/workflows/code-review.yml`
   - It will automatically run on pull requests

#### For GitHub API Access (DevOps Assistant, Autonomous Agent):

1. **Create a GitHub Personal Access Token:**
   - Go to [GitHub Settings → Developer settings → Personal access tokens](https://github.com/settings/tokens)
   - Click "Generate new token (classic)"
   - Give it a name (e.g., "OpenAI Portfolio")
   - Select scopes:
     - `repo` (full repository access)
     - `workflow` (for GitHub Actions access)
   - Click "Generate token"
   - Copy the token (you'll only see it once!)

2. **Configure using User Secrets:**
```bash
cd src/DevOpsAssistant/DevOpsAssistant.Api  # Or AutonomousDevelopmentAgent.Api
dotnet user-secrets set "GitHub:Token" "ghp_your-token-here"
dotnet user-secrets set "GitHub:Owner" "your-username"  # Or organization name
dotnet user-secrets set "GitHub:Repo" "your-repo-name"
```

### Slack Integration

**What is Slack?** A team communication platform used for messaging, notifications, and team collaboration.

**What does this integration do?** Sends automated notifications to Slack channels for:
- Daily summaries and status updates
- Incident reports and alerts
- Deployment notifications
- Pull request updates
- Pipeline status changes

**Setup Steps:**

1. **Create a Slack Incoming Webhook:**
   - Go to [Slack Apps → Incoming Webhooks](https://api.slack.com/messaging/webhooks)
   - Click "Add to Slack"
   - Choose your workspace
   - Select the channel where notifications should appear (e.g., `#devops`, `#notifications`)
   - Click "Add Incoming Webhooks integration"
   - Copy the webhook URL (looks like: `https://hooks.slack.com/services/YOUR/WEBHOOK/URL`)

2. **Configure using User Secrets** (recommended):
```bash
cd src/[ProjectName]/[ProjectName].Api  # Any project that uses Slack
dotnet user-secrets set "Slack:WebhookUrl" "https://hooks.slack.com/services/YOUR/WEBHOOK/URL"
dotnet user-secrets set "Slack:DefaultChannel" "#devops"  # Optional: default channel
```

**Or configure in `appsettings.json`:**
```json
{
  "Slack": {
    "WebhookUrl": "https://hooks.slack.com/services/YOUR/WEBHOOK/URL",
    "DefaultChannel": "#devops"
  }
}
```

**Or set environment variable:**
```bash
export Slack__WebhookUrl="https://hooks.slack.com/services/YOUR/WEBHOOK/URL"
```

**Test the integration:**
- See [Slack Integration Guide](../integrations/slack-integration.md) for examples
- Most projects will send a test notification when they start if Slack is configured

## Next Steps

Now that you're set up, here's what to do next:

### For Non-Developers (No Code Required)

1. **Try the REST APIs** - Use curl, Python, or Postman to interact with the APIs:
   - [REST API Examples](../../samples/REST-API-Examples/README.md) - Complete guide with examples
   - [Postman Collection](../../samples/REST-API-Examples/postman-collection.json) - Import and start testing

2. **Explore Role-Specific Guides** - Find workflows for your role:
   - [Business Analyst Guide](../role-guides/ba-guide.md) - Requirements processing
   - [Product Manager Guide](../role-guides/pm-guide.md) - Retrospectives and meetings
   - [Project Manager Guide](../role-guides/project-manager-guide.md) - Status reports and planning
   - [SDM Guide](../role-guides/sdm-guide.md) - Team management and sprint planning
   - [DevOps Guide](../role-guides/devops-guide.md) - CI/CD and infrastructure

3. **Read Project Documentation** - Understand what each project does:
   - [Project Documentation Index](../project-docs/README.md) - Overview of all projects

### For Developers

1. **Learn the Basics:**
   - [API Basics](02-api-basics.md) - Understanding models, tokens, and making API calls
   - [Prompt Engineering](03-prompt-engineering.md) - Writing effective prompts
   - [First Project](04-first-project.md) - Build your first AI-powered app

2. **Explore Advanced Features:**
   - [Advanced Features Index](../advanced-features/README.md) - Vision API, RAG, Batch Processing
   - [Production Improvements](../improvements/README.md) - Testing, authentication, streaming, etc.

3. **Check Role Guides:**
   - [Developer Guide](../role-guides/developer-guide.md) - Code review, testing, documentation
   - [Tester Guide](../role-guides/tester-guide.md) - Test case generation

### Quick Reference

- **Don't want to code?** → [REST API Examples](../../samples/REST-API-Examples/README.md)
- **Want to understand AI concepts?** → [Glossary](../GLOSSARY.md)
- **Looking for a specific feature?** → [Documentation Index](../README.md)
- **Having setup issues?** → See Troubleshooting section below

## Troubleshooting

### Common Issues and Solutions

#### "API key not found" or "Invalid API key"

**Symptoms:** Error messages about missing or invalid API keys when running the application.

**Solutions:**
1. **Verify your API key is set:**
   - If using environment variables: Check with `echo $OpenAI__ApiKey` (macOS/Linux) or `echo %OpenAI__ApiKey%` (Windows)
   - If using User Secrets: Run `dotnet user-secrets list` to see configured secrets
   - If using appsettings.json: Check the file exists and contains your key

2. **Verify the key format:**
   - OpenAI API keys start with `sk-`
   - Make sure there are no extra spaces or quotes
   - Copy the entire key from the OpenAI dashboard

3. **Check the key is active:**
   - Go to [OpenAI API Keys](https://platform.openai.com/api-keys)
   - Verify your key is active and not revoked
   - Check your account has available credits

4. **Restart your terminal/IDE:**
   - Environment variables only apply to new terminal sessions
   - Close and reopen your terminal/IDE after setting environment variables

#### "Rate limit exceeded" or "429 Too Many Requests"

**Symptoms:** API calls fail with rate limit errors.

**Solutions:**
1. **Check your usage:**
   - Go to [OpenAI Usage Dashboard](https://platform.openai.com/usage)
   - Review your current usage and limits

2. **Wait and retry:**
   - Rate limits reset after a time period
   - Wait a few minutes and try again

3. **Upgrade your plan:**
   - Free tier has lower limits
   - Enterprise subscriptions have higher rate limits

4. **Use batch processing:**
   - For high-volume operations, use [Batch Processing](../advanced-features/batch-processing.md) (50% cost reduction)

#### "Connection timeout" or "Unable to connect"

**Symptoms:** Requests fail with timeout or connection errors.

**Solutions:**
1. **Check internet connection:**
   - Ensure you're connected to the internet
   - Try accessing `https://api.openai.com` in your browser

2. **Check firewall/proxy:**
   - Corporate firewalls may block API access
   - Contact your IT department if needed
   - Try from a different network (e.g., home vs. office)

3. **Verify the API endpoint:**
   - Default: `https://api.openai.com/v1`
   - Check `appsettings.json` if you've customized it

#### Build errors or "Project not found"

**Symptoms:** `dotnet build` fails with errors about missing projects or packages.

**Solutions:**
1. **Restore packages first:**
   ```bash
   dotnet restore
   dotnet build
   ```

2. **Check .NET version:**
   ```bash
   dotnet --version
   ```
   - Should be 8.0 or later
   - If not, [download .NET 8.0 SDK](https://dotnet.microsoft.com/download)

3. **Clean and rebuild:**
   ```bash
   dotnet clean
   dotnet restore
   dotnet build
   ```

4. **Check you're in the right directory:**
   - Run `dotnet build` from the repository root (`openAi-sandbox/`)
   - Or navigate to a specific project folder

#### "Swagger page not loading" or "This site can't be reached"

**Symptoms:** Can't access the Swagger documentation page.

**Solutions:**
1. **Verify the server is running:**
   - Look for "Now listening on: https://localhost:XXXX" in the terminal
   - The server must be running to access Swagger

2. **Try HTTP instead of HTTPS:**
   - Some projects use HTTP: `http://localhost:5001/swagger`
   - Check the terminal output for the correct URL

3. **Check the port number:**
   - Different projects use different ports
   - See [Project Documentation](../project-docs/README.md#api-endpoints) for port numbers

4. **Accept SSL certificate:**
   - For HTTPS, you may need to accept a self-signed certificate
   - Click "Advanced" → "Proceed to localhost" in your browser

#### Integration-specific Issues

**Jira/Confluence: "Unauthorized" or "403 Forbidden"**
- Verify your API token is correct
- Check your username is your email address
- Ensure the token has the right permissions
- Verify the base URL is correct (include `/wiki` for Confluence)

**Slack: "Webhook URL invalid"**
- Verify the webhook URL is complete and correct
- Check the webhook hasn't been revoked in Slack
- Ensure the channel still exists

**GitHub: "Bad credentials"**
- Verify your personal access token is correct
- Check the token hasn't expired
- Ensure the token has the required scopes (`repo`, `workflow`)

### Getting More Help

1. **Check the specific guide:**
   - [API Basics](02-api-basics.md) for API-related issues
   - [Role Guides](../role-guides/) for workflow-specific questions
   - [Integration Guides](../integrations/) for integration setup

2. **Review examples:**
   - [REST API Examples](../../samples/REST-API-Examples/README.md) - Working examples you can test
   - [Code Examples](../../samples/) - See how things are implemented

3. **Check the glossary:**
   - [Glossary](../GLOSSARY.md) - Definitions of technical terms

4. **Review error messages:**
   - Error messages often contain helpful information
   - Look for specific error codes or messages
   - Search the error message online for solutions

## Additional Resources

### Official Documentation

- **[OpenAI Platform Documentation](https://platform.openai.com/docs)** - Complete OpenAI API documentation
- **[OpenAI API Reference](https://platform.openai.com/docs/api-reference)** - Detailed API endpoint documentation
- **[.NET Documentation](https://learn.microsoft.com/dotnet/)** - Microsoft's .NET framework documentation

### Project-Specific Resources

- **[Documentation Index](../README.md)** - Complete index of all documentation
- **[Glossary](../GLOSSARY.md)** - Definitions of technical terms
- **[REST API Examples](../../samples/REST-API-Examples/README.md)** - Try APIs without coding
- **[Role Guides](../role-guides/)** - Role-specific workflows and examples

### Learning Resources

- **[API Basics](02-api-basics.md)** - Understanding models, tokens, and costs
- **[Prompt Engineering](03-prompt-engineering.md)** - Writing effective prompts
- **[First Project](04-first-project.md)** - Hands-on tutorial
- **[Best Practices](../best-practices/)** - Security, cost optimization, error handling

---

**Next:** Ready to make your first API call? Continue to [API Basics](02-api-basics.md) →
