# OpenAI Platform Learning Portfolio

A comprehensive portfolio of sample projects demonstrating OpenAI Platform capabilities for a .NET development team. These projects provide practical tools and workflows for Developers, Business Analysts, Product Managers, Testers, Software Development Managers, and DevOps Engineers.

## Overview

This repository contains **twelve production-ready projects** that showcase the full range of OpenAI Platform capabilities:

### Software Development Projects

**Core Development Tools:**
1. **Requirements Processing Assistant** - Document processing, summarization, Q&A, and user story generation with Confluence integration
2. **Automated Test Case Generator** - Code analysis, test generation from code/user stories, and edge case identification
3. **Intelligent Code Review Assistant** - Automated code review, security scanning, performance analysis with GitHub Actions integration
4. **Documentation Generator** - API documentation, README, and changelog generation from code

**Team Collaboration & Management:**
5. **Sprint Retrospective Analyzer** ⭐ - Text analysis, sentiment tracking, action item extraction, Jira integration, and SignalR real-time updates
6. **Meeting Transcript Analyzer** - Speech-to-text transcription (Whisper), meeting summaries, action item extraction, and follow-up emails
7. **SDM Assistant** - Software Development Manager workflows: daily summaries, sprint planning, velocity analysis, risk identification, status reports with enhanced Jira and Confluence integrations
8. **DevOps Assistant** - DevOps Engineer workflows: log analysis, incident response, CI/CD optimization, infrastructure review (Terraform/ARM/CloudFormation), security scanning, Docker/Kubernetes analysis with GitHub Actions integration
9. **Autonomous Development Agent** ⭐ NEW - AI-powered autonomous code analysis, improvement generation, and automated PR creation

### Other Industry Examples

These projects demonstrate how OpenAI Platform can be applied to different industries:

10. **Publishing Assistant** - Book reviews, summaries, marketing blurbs, cover image generation (DALL-E), Vision API analysis, and file conversion
11. **Pharmacy Assistant** - Patient education, drug interactions, prescription labels, adherence planning, side effect analysis with RAG and Moderation API
12. **Advertising Agency Assistant** - Ad copy, campaign strategy, audience analysis, brand voice, creative briefs, A/B testing, and Vision API for ad creative analysis

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
- [All Projects](docs/project-docs/) - Detailed documentation for all 12 projects
- [Advanced Features](docs/advanced-features/) - Vision API, RAG, Moderation, Batch Processing, JSON Mode
- [AI Agents & Services](docs/concepts/ai-agents-and-services.md) ⭐ NEW - Conceptual guide to autonomous AI agents
- [Best Practices](docs/best-practices/) - Security, cost optimization, error handling
- [Integration Guides](docs/integrations/) - Slack, GitHub Actions, Jira, Confluence setup and examples

### Improvements & Deployment
- [Improvements Overview](docs/improvements/README.md) - All production-ready improvements
- [Middleware Guide](docs/improvements/middleware-guide.md) - Using common middleware
- [Streaming Examples](docs/improvements/streaming-examples.md) - Real-time response streaming
- [JWT Authentication](docs/improvements/jwt-authentication-guide.md) - JWT setup and usage
- [Serilog Guide](docs/improvements/serilog-guide.md) - Structured logging
- [Metrics Guide](docs/improvements/metrics-guide.md) - API usage tracking
- [Autonomous Agent Guide](docs/improvements/autonomous-agent-guide.md) - AI-powered development
- [SignalR Guide](docs/improvements/signalr-guide.md) - Real-time communication
- [Docker Guide](docs/deployment/docker-guide.md) - Containerization and deployment
- [Database Setup](docs/deployment/database-setup.md) - Entity Framework Core configuration

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

### Software Development Projects

| Project | OpenAI APIs | Integrations | Key Features |
|---------|-------------|--------------|--------------|
| Requirements Assistant | GPT-4 | Confluence, Slack | Document processing, user stories, Q&A |
| Test Case Generator | GPT-4 | Slack | Code analysis, test generation, edge cases |
| Code Review Assistant | GPT-4 | GitHub Actions, Slack | Security scanning, performance, style |
| Documentation Generator | GPT-4 | Slack | API docs, README, changelogs |
| Retro Analyzer | GPT-4, Embeddings | Jira, Slack, SignalR | Sentiment analysis, action items, themes, real-time updates |
| Meeting Analyzer | Whisper, GPT-4 | Slack | Transcription, summaries, action items |
| SDM Assistant | GPT-4 | Jira, Confluence, Slack | Daily summaries, sprint planning, velocity |
| DevOps Assistant | GPT-4 | GitHub Actions, Slack | Log analysis, CI/CD optimization, security |
| Autonomous Dev Agent | GPT-4 | GitHub, Slack | Code analysis, improvements, automated PRs |

### Other Industry Examples

| Project | OpenAI APIs | Integrations | Key Features |
|---------|-------------|--------------|--------------|
| Publishing Assistant | GPT-4, DALL-E, Vision | - | Reviews, summaries, cover images, file conversion |
| Pharmacy Assistant | GPT-4, RAG, Moderation | - | Patient education, interactions, prescriptions |
| Advertising Assistant | GPT-4, Vision | - | Ad copy, campaigns, creative analysis |

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
├── src/                          # Source code for all 12 projects
│   ├── RequirementsAssistant/   # Requirements processing
│   ├── TestCaseGenerator/       # Test case generation
│   ├── CodeReviewAssistant/    # Code review automation
│   ├── DocumentationGenerator/  # Documentation generation
│   ├── RetroAnalyzer/           # Retrospective analysis (with SignalR)
│   ├── MeetingAnalyzer/         # Meeting transcription & analysis
│   ├── SDMAssistant/            # SDM workflows
│   ├── DevOpsAssistant/         # DevOps workflows
│   ├── AutonomousDevelopmentAgent/ # Autonomous code analysis & PR creation
│   ├── PublishingAssistant/     # Publishing workflows (industry example)
│   ├── PharmacyAssistant/       # Pharmacy operations (industry example)
│   └── AdvertisingAgency/       # Advertising workflows (industry example)
├── shared/                       # Shared libraries
│   ├── OpenAIShared/            # OpenAI client wrapper
│   ├── Integrations/            # Slack, GitHub integrations
│   ├── Common/                  # Common utilities & middleware
│   ├── Models/                  # Shared data models
│   └── Data/                    # Entity Framework Core database context
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
- **Docker** (optional, for containerized deployment)
- **SQL Server** (optional, for database features)
- **Optional Integrations:**
  - Slack webhook URL (for notifications)
  - GitHub token (for Actions, API access, and Autonomous Agent)
  - Jira API token (for ticket management)
  - Confluence API token (for documentation)

## Recent Improvements ⭐ NEW

The project now includes comprehensive production-ready improvements:

### Phase 1: Foundation
- **Testing Infrastructure**: Unit tests with mocking helpers (`OpenAIMockHelper`)
- **Integration Tests**: End-to-end API testing with `WebApplicationFactory`
- **Authentication**: API key and JWT authentication middleware
- **Error Handling**: Circuit breaker pattern with Polly
- **Health Checks**: Health check endpoints for all APIs

### Phase 2: Production Readiness
- **Streaming Responses**: Server-Sent Events (SSE) for real-time responses
- **Caching Service**: Response caching with IMemoryCache to reduce API costs
- **Rate Limiting**: Token bucket algorithm for request throttling
- **Structured Logging**: Serilog with JSON formatting and file sinks
- **Metrics Service**: Track API usage, costs, and performance metrics

### Phase 3: Advanced Features
- **Autonomous Development Agent**: AI-powered code analysis, improvement generation, and automated PR creation
- **Database Integration**: Entity Framework Core with SQL Server and In-Memory support
- **Docker Deployment**: Multi-stage Dockerfiles and Docker Compose setup
- **SignalR Real-Time**: Live updates for retrospective analysis and collaborative features

### Quick Wins
- **Correlation IDs**: Request tracking across services
- **Request/Response Logging**: Comprehensive logging middleware
- **CORS Configuration**: Cross-origin resource sharing setup
- **Response Compression**: GZip/Brotli compression middleware

See [Improvements Documentation](docs/improvements/) for details, examples, and guides.

## Documentation

### Getting Started
- [Setup Guide](docs/getting-started/01-setup.md) - Environment setup and API key configuration
- [API Basics](docs/getting-started/02-api-basics.md) - First API call, models, tokens, costs
- [Prompt Engineering](docs/getting-started/03-prompt-engineering.md) - Writing effective prompts
- [First Project](docs/getting-started/04-first-project.md) - Building your first AI-powered app

### Role-Specific Guides
- [Developer Guide](docs/role-guides/developer-guide.md) - Code review, testing, documentation
- [Business Analyst Guide](docs/role-guides/ba-guide.md) - Requirements processing, user stories
- [Project Manager Guide](docs/role-guides/project-manager-guide.md) - Status reports, risk analysis
- [Tester Guide](docs/role-guides/tester-guide.md) - Test case generation, test planning
- [SDM Guide](docs/role-guides/sdm-guide.md) - Team management, sprint planning, metrics
- [DevOps Guide](docs/role-guides/devops-guide.md) - CI/CD, infrastructure, monitoring

### Advanced Features
- [Vision API](docs/advanced-features/vision-api.md) - Image analysis and understanding
- [RAG Patterns](docs/advanced-features/rag-patterns.md) - Retrieval-Augmented Generation
- [Batch Processing](docs/advanced-features/batch-processing.md) - Cost-effective bulk operations
- [Moderation API](docs/advanced-features/moderation-api.md) - Content safety and filtering
- [JSON Mode](docs/advanced-features/json-mode.md) - Structured output generation

### Improvements & Best Practices
- [Improvements Overview](docs/improvements/README.md) - All production-ready improvements
- [Middleware Guide](docs/improvements/middleware-guide.md) - Using common middleware
- [Streaming Examples](docs/improvements/streaming-examples.md) - Real-time response streaming
- [JWT Authentication](docs/improvements/jwt-authentication-guide.md) - JWT setup and usage
- [Serilog Guide](docs/improvements/serilog-guide.md) - Structured logging
- [Metrics Guide](docs/improvements/metrics-guide.md) - API usage tracking
- [Autonomous Agent Guide](docs/improvements/autonomous-agent-guide.md) - AI-powered development
- [SignalR Guide](docs/improvements/signalr-guide.md) - Real-time communication
- [Docker Guide](docs/deployment/docker-guide.md) - Containerization and deployment
- [Database Setup](docs/deployment/database-setup.md) - Entity Framework Core configuration

### Concepts
- [AI Agents and Services](docs/concepts/ai-agents-and-services.md) - Conceptual overview
- [Autonomous Incident Response](docs/concepts/autonomous-incident-response-agent.md) - Practical example

### Integrations
- [Slack Integration](docs/integrations/slack-integration.md) - Slack notifications and workflows
- [GitHub Examples](samples/GitHubExamples/) - GitHub Actions and API examples
- [REST API Examples](samples/REST-API-Examples/) - Direct API calls without .NET

## Contributing

This is a learning portfolio. Feel free to explore, modify, and extend the projects to suit your needs.

### Code Standards

This project follows comprehensive coding standards defined in [`.cursorrules`](.cursorrules). Key principles:
- **Shared libraries first** - Never duplicate code
- **Service registration pattern** for dependency injection
- **Thin controllers** - Delegate to services
- **Comprehensive testing** - Unit and integration tests
- **Structured logging** with Serilog
- **Security best practices** - Authentication, authorization, input validation

### Code Standards

This project follows comprehensive coding standards defined in [`.cursorrules`](.cursorrules). Key principles:
- Shared libraries first - never duplicate code
- Service registration pattern for DI
- Thin controllers - delegate to services
- Comprehensive testing
- Structured logging
- Security best practices

## License

MIT License - See LICENSE file for details
