# OpenAI Platform Learning Portfolio

A comprehensive portfolio of sample projects demonstrating OpenAI Platform capabilities for a .NET development team. These projects provide practical tools and workflows for Developers, Business Analysts, Product Managers, Testers, Software Development Managers, and DevOps Engineers.

## Overview

This repository contains **eleven production-ready projects** that showcase the full range of OpenAI Platform capabilities:

### Core Development Projects

1. **Requirements Processing Assistant** - Document processing, summarization, Q&A, and user story generation with Confluence integration
2. **Automated Test Case Generator** - Code analysis, test generation from code/user stories, and edge case identification
3. **Sprint Retrospective Analyzer** - Text analysis, sentiment tracking, action item extraction, and Jira integration
4. **Intelligent Code Review Assistant** - Automated code review, security scanning, performance analysis with GitHub Actions integration
5. **Documentation Generator** - API documentation, README, and changelog generation from code
6. **Meeting Transcript Analyzer** - Speech-to-text transcription (Whisper), meeting summaries, action item extraction, and follow-up emails

### Industry-Specific Projects

7. **Publishing Assistant** - Book reviews, summaries, marketing blurbs, cover image generation (DALL-E), Vision API analysis, and file conversion
8. **Pharmacy Assistant** - Patient education, drug interactions, prescription labels, adherence planning, side effect analysis with RAG and Moderation API
9. **Advertising Agency Assistant** - Ad copy, campaign strategy, audience analysis, brand voice, creative briefs, A/B testing, and Vision API for ad creative analysis

### Management & Integration Projects

10. **SDM Assistant** - Software Development Manager workflows: daily summaries, sprint planning, velocity analysis, risk identification, status reports with enhanced Jira and Confluence integrations
11. **DevOps Assistant** - DevOps Engineer workflows: log analysis, incident response, CI/CD optimization, infrastructure review (Terraform/ARM/CloudFormation), security scanning, Docker/Kubernetes analysis with GitHub Actions integration

## Getting Started

See the [Getting Started Guide](docs/getting-started/01-setup.md) for setup instructions.

## Project Structure

```
openAi-sandbox/
├── src/                    # Source code for all projects
├── shared/                 # Shared libraries and utilities
├── docs/                   # Documentation
├── tests/                  # Test projects
└── samples/                # Example usage and prompts
```

## Prerequisites

- .NET 8.0 SDK or later
- OpenAI API key (Enterprise subscription)
- Visual Studio 2022, VS Code, or Rider

## Quick Start

### Option 1: Use REST API Examples (No Development Environment Required)

Try the APIs immediately using curl, Python, or Postman - no .NET setup needed!

- **[REST API Examples](samples/REST-API-Examples/README.md)** - curl, Python, and Postman examples
- **[Direct OpenAI API Examples](samples/REST-API-Examples/openai-direct-examples.sh)** - Call OpenAI directly

### Option 2: Build and Run Projects

1. Clone the repository
2. Set up your OpenAI API key (see [Setup Guide](docs/getting-started/01-setup.md))
3. Build the solution: `dotnet build`
4. Run a sample project

## OpenAI Platform Capabilities Demonstrated

This portfolio demonstrates the full range of OpenAI Platform features:

### Core APIs
- **GPT-4 Turbo** - Advanced reasoning, code generation, analysis, and content creation
- **Whisper API** - Speech-to-text transcription for meeting analysis
- **DALL-E** - Image generation for cover images and creative content
- **Embeddings API** - Semantic search and RAG (Retrieval-Augmented Generation)
- **Moderation API** - Content safety and compliance checking

### Advanced Features
- **Vision API** - Image analysis for cover images, prescription labels, and ad creatives
- **RAG Patterns** - Document Q&A with semantic search
- **Batch Processing** - Cost-effective high-volume processing (50% cost reduction)
- **JSON Mode** - Structured output generation
- **Function Calling** - Structured data extraction

## Integrations

All projects support comprehensive integration with your existing tools:

- **Slack** ⭐ - Notifications, daily summaries, incident reports, deployment updates, PR notifications, pipeline status
- **GitHub Actions** - Automated code reviews, workflow analysis, pipeline optimization, PR deployment checks
- **Jira** - Ticket creation, sprint planning, action item tracking, velocity analysis
- **Confluence** - Requirements processing, documentation management, page creation/updates

See [Integration Guides](docs/integrations/) for setup and complete examples.

## Documentation

### Getting Started
- [Setup Guide](docs/getting-started/01-setup.md) - Environment setup, API keys, integrations
- [API Basics](docs/getting-started/02-api-basics.md) - Understanding models, tokens, costs
- [Prompt Engineering](docs/getting-started/03-prompt-engineering.md) - Writing effective prompts
- [First Project](docs/getting-started/04-first-project.md) - Build your first AI-powered app

### Role-Specific Guides
- [Developer Guide](docs/role-guides/developer-guide.md) - Code review, testing, documentation
- [Business Analyst Guide](docs/role-guides/ba-guide.md) - Requirements processing, user stories
- [Product Manager Guide](docs/role-guides/pm-guide.md) - Retrospectives, meetings, planning
- [Project Manager Guide](docs/role-guides/project-manager-guide.md) ⭐ NEW - Status reports, risk analysis, stakeholder communication, project planning, budget tracking
- [Tester Guide](docs/role-guides/tester-guide.md) - Test case generation, edge cases
- [SDM Guide](docs/role-guides/sdm-guide.md) ⭐ - Daily summaries, sprint planning, team management
- [DevOps Guide](docs/role-guides/devops-guide.md) ⭐ - Log analysis, CI/CD optimization, infrastructure

### Project Documentation
- [All Projects](docs/project-docs/) - Detailed documentation for all 11 projects
- [Advanced Features](docs/advanced-features/) - Vision API, RAG, Moderation, Batch Processing, JSON Mode
- [Best Practices](docs/best-practices/) - Security, cost optimization, error handling
- [Integration Guides](docs/integrations/) - Slack, GitHub Actions, Jira, Confluence setup and examples

## Quick Start Examples

### Option 1: REST API (No Development Environment Required)

Try the APIs immediately using curl, Python, or Postman:

- **[REST API Examples](samples/REST-API-Examples/README.md)** - Complete curl, Python, and Postman examples
- **[Direct OpenAI API Examples](samples/REST-API-Examples/openai-direct-examples.sh)** - Call OpenAI directly
- **[Postman Collection](samples/REST-API-Examples/postman-collection.json)** - Import ready-to-use collection

### Option 2: Build and Run Projects

1. Clone the repository: `git clone https://github.com/jfinlon-goodrx/openAi-sandbox.git`
2. Set up your OpenAI API key (see [Setup Guide](docs/getting-started/01-setup.md))
3. Build the solution: `dotnet build`
4. Run a sample project: `dotnet run --project src/[ProjectName]/[ProjectName].Api`

### Option 3: GitHub Actions Workflows

Use pre-built workflows for automated code reviews, deployments, and notifications:

- **[GitHub Actions Workflows](samples/GitHubExamples/GitHubActionsWorkflows.md)** - Complete workflow templates
- **[Slack + GitHub Actions](samples/GitHubExamples/SlackGitHubWorkflows.md)** - Combined automation examples

## Features Matrix

| Project | OpenAI APIs | Integrations | Key Features |
|---------|-------------|--------------|--------------|
| Requirements Assistant | GPT-4 | Confluence, Slack | Document processing, user stories, Q&A |
| Test Case Generator | GPT-4 | Slack | Code analysis, test generation, edge cases |
| Retro Analyzer | GPT-4, Embeddings | Jira, Slack | Sentiment analysis, action items, themes |
| Code Review Assistant | GPT-4 | GitHub Actions, Slack | Security scanning, performance, style |
| Documentation Generator | GPT-4 | Slack | API docs, README, changelogs |
| Meeting Analyzer | Whisper, GPT-4 | Slack | Transcription, summaries, action items |
| Publishing Assistant | GPT-4, DALL-E, Vision | - | Reviews, summaries, cover images, file conversion |
| Pharmacy Assistant | GPT-4, RAG, Moderation | - | Patient education, interactions, prescriptions |
| Advertising Assistant | GPT-4, Vision | - | Ad copy, campaigns, creative analysis |
| SDM Assistant | GPT-4 | Jira, Confluence, Slack | Daily summaries, sprint planning, velocity |
| DevOps Assistant | GPT-4 | GitHub Actions, Slack | Log analysis, CI/CD optimization, security |

## Sample Code & Examples

### Complete Workflows
- **[Pharmacy Workflow](samples/CompleteWorkflows/PharmacyWorkflow.cs)** - End-to-end prescription processing with RAG and Moderation
- **[Advertising Workflow](samples/CompleteWorkflows/AdvertisingWorkflow.cs)** - Complete campaign development with Vision API

### Advanced Examples
- **[Vision API Examples](samples/AdvancedExamples/VisionAPI.md)** - Image analysis and understanding
- **[RAG Examples](samples/AdvancedExamples/RAGExamples.md)** - Retrieval-Augmented Generation patterns
- **[Moderation Examples](samples/AdvancedExamples/ModerationExamples.md)** - Content safety checking
- **[Batch Processing Examples](samples/AdvancedExamples/BatchProcessingExamples.md)** - High-volume processing
- **[JSON Mode Examples](samples/AdvancedExamples/JSONModeExamples.md)** - Structured output generation

### Integration Examples
- **[Slack Examples](samples/SlackExamples/)** - Notification workflows and templates
- **[GitHub Actions Examples](samples/GitHubExamples/)** - CI/CD automation workflows

## Project Structure

```
openAi-sandbox/
├── src/                          # Source code for all 11 projects
│   ├── RequirementsAssistant/   # Requirements processing
│   ├── TestCaseGenerator/       # Test case generation
│   ├── RetroAnalyzer/           # Retrospective analysis
│   ├── CodeReviewAssistant/    # Code review automation
│   ├── DocumentationGenerator/  # Documentation generation
│   ├── MeetingAnalyzer/         # Meeting transcription & analysis
│   ├── PublishingAssistant/     # Publishing workflows
│   ├── PharmacyAssistant/       # Pharmacy operations
│   ├── AdvertisingAgency/       # Advertising workflows
│   ├── SDMAssistant/            # SDM workflows
│   └── DevOpsAssistant/         # DevOps workflows
├── shared/                       # Shared libraries
│   ├── OpenAIShared/            # OpenAI client wrapper
│   ├── Integrations/            # Slack, GitHub integrations
│   ├── Common/                  # Common utilities
│   └── Models/                  # Shared data models
├── docs/                         # Comprehensive documentation
│   ├── getting-started/         # Setup and basics
│   ├── role-guides/             # Role-specific workflows
│   ├── project-docs/            # Project documentation
│   ├── advanced-features/       # Advanced OpenAI features
│   ├── integrations/           # Integration guides
│   └── best-practices/          # Security, cost optimization
├── samples/                      # Examples and templates
│   ├── REST-API-Examples/       # curl, Python, Postman examples
│   ├── AdvancedExamples/        # Advanced feature examples
│   ├── CompleteWorkflows/       # End-to-end workflows
│   ├── SlackExamples/           # Slack integration examples
│   └── GitHubExamples/          # GitHub Actions workflows
└── tests/                        # Test projects
```

## Prerequisites

- **.NET 8.0 SDK** or later
- **OpenAI API key** (Enterprise subscription recommended)
- **Optional Integrations:**
  - Slack webhook URL (for notifications)
  - GitHub token (for Actions and API access)
  - Jira API token (for ticket management)
  - Confluence API token (for documentation)

## Contributing

This is a learning portfolio. Feel free to explore, modify, and extend the projects to suit your needs.

## License

MIT License - See LICENSE file for details
