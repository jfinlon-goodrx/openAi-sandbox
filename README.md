# OpenAI Platform Learning Portfolio

A comprehensive portfolio of sample projects demonstrating OpenAI Platform capabilities for a .NET development team. These projects provide practical tools and workflows for Developers, Business Analysts, Product Managers, and Testers.

## Overview

This repository contains eleven sample projects that showcase different aspects of the OpenAI Platform:

1. **Requirements Processing Assistant** - Document processing, summarization, and Q&A
2. **Automated Test Case Generator** - Code analysis and test generation
3. **Sprint Retrospective Analyzer** - Text analysis and sentiment tracking
4. **Intelligent Code Review Assistant** - Automated code review and suggestions
5. **Documentation Generator** - Code-to-documentation automation
6. **Meeting Transcript Analyzer** - Speech-to-text and meeting analysis
7. **Publishing Assistant** - Book reviews, summaries, marketing blurbs, cover image generation, and file conversion
8. **Pharmacy Assistant** - Patient education, drug interactions, prescription labels, adherence planning, and side effect analysis
9. **Advertising Agency Assistant** - Ad copy, campaign strategy, audience analysis, brand voice, creative briefs, and A/B testing
10. **SDM Assistant** - Software Development Manager workflows with Jira and Confluence integration: daily summaries, sprint planning, risk identification, and status reports
11. **DevOps Assistant** - DevOps Engineer workflows: log analysis, CI/CD optimization, infrastructure review, security scanning, and container analysis

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

## Documentation

- [Getting Started](docs/getting-started/) - Setup and basics
- [Role Guides](docs/role-guides/) - Role-specific workflows (Developer, BA, PM, Tester, **SDM**, **DevOps**)
- [Project Documentation](docs/project-docs/) - Detailed project docs for all 11 projects
- [Best Practices](docs/best-practices/) - Security, cost optimization, error handling
- [Advanced Features](docs/advanced-features/) - Vision API, RAG, Moderation, Batch Processing, JSON Mode

### New Guides

**SDM Guide** - For Software Development Managers using Jira and Confluence daily:
- **[SDM Guide](docs/role-guides/sdm-guide.md)** - Complete guide for SDM workflows
- **[SDM Assistant](docs/project-docs/sdm-assistant.md)** - Project documentation
- Daily standup preparation, sprint planning, risk identification, and automated status reports

**DevOps Guide** - For DevOps Engineers managing infrastructure and CI/CD:
- **[DevOps Guide](docs/role-guides/devops-guide.md)** - Complete guide for DevOps workflows
- **[DevOps Assistant](docs/project-docs/devops-assistant.md)** - Project documentation
- Log analysis, CI/CD optimization, infrastructure review, security scanning, and container analysis

## Contributing

This is a learning portfolio. Feel free to explore, modify, and extend the projects to suit your needs.

## License

MIT License - See LICENSE file for details
