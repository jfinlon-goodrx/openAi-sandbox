# Project Documentation

Detailed documentation for each project in the OpenAI Platform Learning Portfolio.

## Core Development Projects

### [Requirements Processing Assistant](requirements-assistant.md)
Document processing, summarization, and Q&A for Business Analysts.
- Document summarization
- User story generation
- Gap analysis
- Confluence integration

### [Automated Test Case Generator](test-case-generator.md)
Code analysis and test generation for Developers and Testers.
- Test case generation from code
- User story to test case conversion
- Test scenario generation

### [Sprint Retrospective Analyzer](retro-analyzer.md)
Text analysis and sentiment tracking for Product Managers.
- Comment analysis
- Action item extraction
- Sentiment tracking
- Jira integration

### [Intelligent Code Review Assistant](code-review-assistant.md)
Automated code review and suggestions for Developers.
- Code analysis
- Security scanning
- Performance suggestions
- GitHub Actions integration

### [Documentation Generator](documentation-generator.md)
Code-to-documentation automation for Developers.
- API documentation generation
- README generation
- Changelog creation

### [Meeting Transcript Analyzer](meeting-analyzer.md)
Speech-to-text and meeting analysis for Product Managers.
- Audio transcription (Whisper API)
- Meeting summaries
- Action item extraction
- Follow-up email generation

## Industry-Specific Projects

### [Publishing Assistant](publishing-assistant.md)
Book reviews, summaries, marketing blurbs, and cover image generation.
- Book review generation
- Summary/blurb creation
- Marketing copy generation
- Cover image description (DALL-E)
- File format conversion

### [Pharmacy Assistant](pharmacy-assistant.md)
Patient education, drug interactions, and prescription management.
- Patient education materials
- Drug interaction checking
- Prescription label generation
- Adherence planning
- Side effect analysis

### [Advertising Agency Assistant](advertising-agency-assistant.md)
Ad copy, campaign strategy, and creative briefs.
- Ad copy generation
- Campaign strategy development
- Target audience analysis
- Brand voice development
- Creative brief generation
- A/B test hypotheses

## Management & Integration Projects

### [SDM Assistant](sdm-assistant.md) ⭐ NEW
Software Development Manager workflows with Jira and Confluence integration.
- Daily activity summaries
- Standup preparation
- Sprint planning and velocity analysis
- Risk identification
- Status report generation
- Sprint health analysis
- Enhanced Jira and Confluence integrations

### [DevOps Assistant](devops-assistant.md) ⭐ NEW
DevOps Engineer workflows for infrastructure and CI/CD management.
- Log analysis and incident response
- CI/CD pipeline optimization
- Infrastructure as Code review (Terraform, ARM, CloudFormation)
- Deployment script generation
- Runbook generation
- Metrics analysis
- Alerting optimization
- Security scanning
- Docker and Kubernetes analysis

## Project Features Matrix

| Project | OpenAI APIs Used | Integrations | Use Cases |
|---------|-----------------|--------------|-----------|
| Requirements Assistant | GPT-4 | Confluence, Slack | BA, PM |
| Test Case Generator | GPT-4 | Slack | Developer, Tester |
| Retro Analyzer | GPT-4, Embeddings | Jira, Slack | PM, SDM |
| Code Review Assistant | GPT-4 | GitHub Actions, Slack | Developer |
| Documentation Generator | GPT-4 | Slack | Developer |
| Meeting Analyzer | Whisper, GPT-4 | Slack | PM, SDM |
| Publishing Assistant | GPT-4, DALL-E, Vision | - | Publishing |
| Pharmacy Assistant | GPT-4 | - | Pharmacy |
| Advertising Assistant | GPT-4 | - | Advertising |
| SDM Assistant | GPT-4 | Jira, Confluence, Slack | SDM |
| DevOps Assistant | GPT-4 | GitHub Actions, Azure DevOps, Slack | DevOps |

## Getting Started

1. **Choose a Project**: Select a project that matches your role or use case
2. **Read the Documentation**: Review the project-specific documentation
3. **Set Up**: Follow the setup instructions
4. **Try Examples**: Run the example code
5. **Integrate**: Connect with your tools (Jira, Confluence, GitHub)

## Common Setup Steps

All projects require:
1. OpenAI API key configuration
2. .NET 8.0 SDK
3. Project-specific dependencies (see individual docs)

Some projects require additional setup:
- **Jira Integration**: API token from Atlassian
- **Confluence Integration**: API token from Atlassian
- **GitHub Integration**: GitHub token for Actions
- **Slack Integration**: Webhook URL from Slack (see [Slack Integration Guide](../integrations/slack-integration.md)) ⭐ NEW

See [Setup Guide](../getting-started/01-setup.md) for detailed instructions.

## API Endpoints

Most projects expose REST APIs with Swagger documentation:
- Requirements Assistant: `https://localhost:5001/swagger`
- Retro Analyzer: `https://localhost:5004/swagger`
- Meeting Analyzer: `https://localhost:5005/swagger`
- Publishing Assistant: `https://localhost:7003/swagger`
- Pharmacy Assistant: `https://localhost:5001/swagger`
- Advertising Assistant: `https://localhost:5002/swagger`
- SDM Assistant: `https://localhost:7006/swagger`
- DevOps Assistant: `https://localhost:7007/swagger`

## Examples

Each project includes:
- Code examples in the documentation
- Sample usage in `samples/` directory
- REST API examples (no dev environment needed)
- Complete workflow examples

## Resources

- [Role Guides](../role-guides/) - Role-specific workflows
- [Getting Started](../getting-started/) - Setup and basics
- [Best Practices](../best-practices/) - Security, cost optimization
- [Advanced Features](../advanced-features/) - Advanced OpenAI capabilities
