# GitHub Actions Workflow Examples

Examples demonstrating AI-powered GitHub Actions workflows for DevOps automation.

## Automated Code Review Workflow

```yaml
name: AI Code Review

on:
  pull_request:
    types: [opened, synchronize, reopened]

jobs:
  code-review:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      pull-requests: write
    steps:
      - uses: actions/checkout@v3
        with:
          fetch-depth: 0

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Run AI Code Review
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          dotnet run --project CodeReviewAssistant.GitHub -- \
            --repo ${{ github.repository }} \
            --pr ${{ github.event.pull_request.number }} \
            --token ${{ secrets.GITHUB_TOKEN }}
```

## Automated Pipeline Optimization

```yaml
name: Analyze and Optimize Pipeline

on:
  workflow_run:
    workflows: ["CI"]
    types:
      - completed

jobs:
  optimize:
    runs-on: ubuntu-latest
    if: github.event.workflow_run.conclusion == 'success'
    steps:
      - uses: actions/checkout@v3

      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'

      - name: Analyze Pipeline Performance
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: |
          # Call DevOps Assistant API to analyze workflow
          curl -X POST http://localhost:7007/api/devops/analyze-github-workflow \
            -H "Content-Type: application/json" \
            -H "Authorization: Bearer $GITHUB_TOKEN" \
            -d '{
              "owner": "${{ github.repository_owner }}",
              "repo": "${{ github.event.repository.name }}",
              "workflowPath": ".github/workflows/ci.yml"
            }'

      - name: Create Optimization PR
        if: success()
        run: |
          # Generate optimized workflow and create PR
          # (Implementation would call DevOps Assistant API)
```

## PR Deployment Readiness Check

```yaml
name: Deployment Readiness Check

on:
  pull_request:
    types: [opened, synchronize]

jobs:
  deployment-check:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Analyze PR for Deployment
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: |
          curl -X POST http://localhost:7007/api/devops/analyze-pr-deployment \
            -H "Content-Type: application/json" \
            -H "Authorization: Bearer $GITHUB_TOKEN" \
            -d '{
              "owner": "${{ github.repository_owner }}",
              "repo": "${{ github.event.repository.name }}",
              "prNumber": ${{ github.event.pull_request.number }}
            }'

      - name: Comment on PR
        uses: actions/github-script@v6
        with:
          script: |
            // Post analysis results as PR comment
            github.rest.issues.createComment({
              issue_number: context.issue.number,
              owner: context.repo.owner,
              repo: context.repo.repo,
              body: 'Deployment readiness analysis completed'
            })
```

## Automated Incident Response

```yaml
name: Incident Response

on:
  workflow_run:
    workflows: ["Deploy to Production"]
    types:
      - failure

jobs:
  incident-response:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Collect Logs
        run: |
          # Collect deployment logs
          gh run view ${{ github.event.workflow_run.id }} --log > deployment-logs.txt

      - name: Analyze Incident
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          curl -X POST http://localhost:7007/api/devops/analyze-logs \
            -H "Content-Type: application/json" \
            -d '{
              "logs": "$(cat deployment-logs.txt)",
              "logType": "deployment",
              "timeRangeHours": 1
            }'

      - name: Generate Incident Report
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          # Generate and post incident report
          # (Implementation would call DevOps Assistant API)
```

## Weekly Pipeline Performance Report

```yaml
name: Weekly Pipeline Performance Report

on:
  schedule:
    - cron: '0 9 * * 1' # Every Monday at 9 AM

jobs:
  performance-report:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Collect Pipeline Data
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: |
          # Get last week's workflow runs
          gh api repos/${{ github.repository }}/actions/runs \
            --jq '.workflow_runs[] | select(.created_at > (now - 604800))' \
            > workflow-runs.json

      - name: Generate Performance Report
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          curl -X POST http://localhost:7007/api/devops/analyze-pipeline \
            -H "Content-Type: application/json" \
            -d @workflow-runs.json

      - name: Create Issue with Report
        uses: actions/github-script@v6
        with:
          script: |
            github.rest.issues.create({
              owner: context.repo.owner,
              repo: context.repo.repo,
              title: `Weekly Pipeline Performance Report - ${new Date().toISOString().split('T')[0]}`,
              body: 'Performance analysis report...',
              labels: ['performance', 'automated']
            })
```

## Infrastructure Code Review

```yaml
name: Infrastructure Code Review

on:
  pull_request:
    paths:
      - '**/*.tf'
      - '**/*.yml'
      - '**/Dockerfile*'
      - '**/kubernetes/**'

jobs:
  infrastructure-review:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Review Terraform
        if: contains(github.event.pull_request.changed_files, '.tf')
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          # Review changed Terraform files
          for file in $(git diff --name-only origin/${{ github.base_ref }}...HEAD | grep '\.tf$'); do
            curl -X POST http://localhost:7007/api/devops/review-infrastructure \
              -H "Content-Type: application/json" \
              -d "{
                \"code\": \"$(cat $file)\",
                \"infrastructureType\": \"Terraform\"
              }"
          done

      - name: Review Dockerfile
        if: contains(github.event.pull_request.changed_files, 'Dockerfile')
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          curl -X POST http://localhost:7007/api/devops/analyze-dockerfile \
            -H "Content-Type: application/json" \
            -d "{
              \"dockerfileContent\": \"$(cat Dockerfile)\"
            }"
```

## Security Scan on PR

```yaml
name: Security Scan

on:
  pull_request:
    types: [opened, synchronize]

jobs:
  security-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3

      - name: Security Scan
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          curl -X POST http://localhost:7007/api/devops/security-scan \
            -H "Content-Type: application/json" \
            -d '{
              "scanTarget": "pull_request",
              "configFiles": ["$(git diff --name-only origin/${{ github.base_ref }}...HEAD | tr '\n' ',' | sed 's/,$//')"]
            }'

      - name: Comment Security Findings
        uses: actions/github-script@v6
        if: success()
        with:
          script: |
            // Post security scan results
```

## Using the DevOps Assistant API Directly

### Analyze GitHub Workflow

```bash
curl -X POST http://localhost:7007/api/devops/analyze-github-workflow \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_GITHUB_TOKEN" \
  -d '{
    "owner": "your-org",
    "repo": "your-repo",
    "workflowPath": ".github/workflows/ci.yml"
  }'
```

### Analyze PR for Deployment

```bash
curl -X POST http://localhost:7007/api/devops/analyze-pr-deployment \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_GITHUB_TOKEN" \
  -d '{
    "owner": "your-org",
    "repo": "your-repo",
    "prNumber": 123
  }'
```

## Slack Integration

See [Slack + GitHub Actions Workflows](SlackGitHubWorkflows.md) for examples combining GitHub Actions with Slack notifications.

## Best Practices

1. **Use Secrets**: Always store API keys in GitHub Secrets
2. **Rate Limiting**: Be mindful of GitHub API rate limits
3. **Caching**: Cache workflow runs data when possible
4. **Error Handling**: Handle API failures gracefully
5. **Notifications**: Post results as PR comments, issues, or Slack channels
6. **Slack Integration**: Send notifications to Slack for better visibility
