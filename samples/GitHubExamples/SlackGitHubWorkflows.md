# Slack + GitHub Actions Workflow Examples

Examples combining GitHub Actions with Slack notifications for complete DevOps automation.

## Deployment with Slack Notifications

```yaml
name: Deploy and Notify

on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Deploy to Production
        run: |
          # Your deployment steps
          echo "Deploying version ${{ github.sha }}"
          
      - name: Notify Slack - Success
        if: success()
        env:
          SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
        run: |
          curl -X POST $SLACK_WEBHOOK_URL \
            -H 'Content-Type: application/json' \
            -d '{
              "text": "✅ Deployment Successful",
              "blocks": [
                {
                  "type": "header",
                  "text": {
                    "type": "plain_text",
                    "text": "🚀 Deployment to Production"
                  }
                },
                {
                  "type": "section",
                  "fields": [
                    {
                      "type": "mrkdwn",
                      "text": "*Repository:*\n${{ github.repository }}"
                    },
                    {
                      "type": "mrkdwn",
                      "text": "*Commit:*\n${{ github.sha }}"
                    },
                    {
                      "type": "mrkdwn",
                      "text": "*Author:*\n${{ github.actor }}"
                    },
                    {
                      "type": "mrkdwn",
                      "text": "*Time:*\n$(date)"
                    }
                  ]
                },
                {
                  "type": "section",
                  "text": {
                    "type": "mrkdwn",
                    "text": "<${{ github.server_url }}/${{ github.repository }}/commit/${{ github.sha }}|View Commit>"
                  }
                }
              ],
              "attachments": [{
                "color": "good"
              }]
            }'
            
      - name: Notify Slack - Failure
        if: failure()
        env:
          SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
        run: |
          curl -X POST $SLACK_WEBHOOK_URL \
            -H 'Content-Type: application/json' \
            -d '{
              "text": "❌ Deployment Failed",
              "blocks": [
                {
                  "type": "header",
                  "text": {
                    "type": "plain_text",
                    "text": "🚨 Deployment Failed"
                  }
                },
                {
                  "type": "section",
                  "text": {
                    "type": "mrkdwn",
                    "text": "*Repository:* ${{ github.repository }}\n*Commit:* ${{ github.sha }}\n*Author:* ${{ github.actor }}"
                  }
                },
                {
                  "type": "section",
                  "text": {
                    "type": "mrkdwn",
                    "text": "<${{ github.server_url }}/${{ github.repository }}/actions/runs/${{ github.run_id }}|View Failed Run>"
                  }
                }
              ],
              "attachments": [{
                "color": "danger"
              }]
            }'
```

## PR Review with Slack Notification

```yaml
name: AI Code Review and Notify

on:
  pull_request:
    types: [opened, synchronize]

jobs:
  review:
    runs-on: ubuntu-latest
    permissions:
      contents: read
      pull-requests: write
    steps:
      - uses: actions/checkout@v3
      
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
            --pr ${{ github.event.pull_request.number }}
      
      - name: Notify Slack
        if: always()
        env:
          SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
        run: |
          curl -X POST $SLACK_WEBHOOK_URL \
            -H 'Content-Type: application/json' \
            -d '{
              "text": "🔍 Code Review Completed",
              "blocks": [
                {
                  "type": "section",
                  "text": {
                    "type": "mrkdwn",
                    "text": "*PR Review Completed*\n<${{ github.event.pull_request.html_url }}|PR #${{ github.event.pull_request.number }}: ${{ github.event.pull_request.title }}>"
                  }
                },
                {
                  "type": "section",
                  "fields": [
                    {
                      "type": "mrkdwn",
                      "text": "*Author:*\n${{ github.event.pull_request.user.login }}"
                    },
                    {
                      "type": "mrkdwn",
                      "text": "*Status:*\n${{ job.status }}"
                    }
                  ]
                }
              ]
            }'
```

## Pipeline Performance Report to Slack

```yaml
name: Weekly Pipeline Performance Report

on:
  schedule:
    - cron: '0 9 * * 1' # Every Monday at 9 AM
  workflow_dispatch:

jobs:
  performance-report:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Collect Pipeline Data
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: |
          gh api repos/${{ github.repository }}/actions/runs \
            --jq '.workflow_runs[] | select(.created_at > (now - 604800)) | {name: .name, status: .status, conclusion: .conclusion, created_at: .created_at}' \
            > workflow-runs.json
      
      - name: Analyze and Send Report
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
          SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
        run: |
          # Call DevOps Assistant API to analyze
          ANALYSIS=$(curl -s -X POST http://localhost:7007/api/devops/analyze-pipeline \
            -H "Content-Type: application/json" \
            -d @workflow-runs.json)
          
          # Send to Slack
          curl -X POST $SLACK_WEBHOOK_URL \
            -H 'Content-Type: application/json' \
            -d "{
              \"text\": \"📊 Weekly Pipeline Performance Report\",
              \"blocks\": [
                {
                  \"type\": \"header\",
                  \"text\": {
                    \"type\": \"plain_text\",
                    \"text\": \"📊 Weekly Pipeline Performance Report\"
                  }
                },
                {
                  \"type\": \"section\",
                  \"text\": {
                    \"type\": \"mrkdwn\",
                    \"text\": \"$ANALYSIS\"
                  }
                }
              ]
            }"
```

## Incident Response Workflow

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
        env:
          GITHUB_TOKEN: ${{ secrets.GITHUB_TOKEN }}
        run: |
          gh run view ${{ github.event.workflow_run.id }} --log > deployment-logs.txt
      
      - name: Analyze Incident
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          # Call DevOps Assistant API
          LOGS=$(cat deployment-logs.txt)
          ANALYSIS=$(curl -s -X POST http://localhost:7007/api/devops/analyze-logs \
            -H "Content-Type: application/json" \
            -d "{\"logs\": \"$LOGS\", \"logType\": \"deployment\"}")
          
          echo "INCIDENT_ANALYSIS=$ANALYSIS" >> $GITHUB_ENV
      
      - name: Generate Incident Report
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          REPORT=$(curl -s -X POST http://localhost:7007/api/devops/incident-report \
            -H "Content-Type: application/json" \
            -d "{\"logAnalysis\": {\"analysis\": \"$INCIDENT_ANALYSIS\"}, \"severity\": \"High\"}")
          
          echo "INCIDENT_REPORT=$REPORT" >> $GITHUB_ENV
      
      - name: Notify Slack
        env:
          SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
        run: |
          curl -X POST $SLACK_WEBHOOK_URL \
            -H 'Content-Type: application/json' \
            -d "{
              \"text\": \"🚨 Production Deployment Failed\",
              \"blocks\": [
                {
                  \"type\": \"header\",
                  \"text\": {
                    \"type\": \"plain_text\",
                    \"text\": \"🚨 Incident: Production Deployment Failure\"
                  }
                },
                {
                  \"type\": \"section\",
                  \"text\": {
                    \"type\": \"mrkdwn\",
                    \"text\": \"$INCIDENT_REPORT\"
                  }
                },
                {
                  \"type\": \"section\",
                  \"text\": {
                    \"type\": \"mrkdwn\",
                    \"text\": \"<${{ github.server_url }}/${{ github.repository }}/actions/runs/${{ github.event.workflow_run.id }}|View Failed Run>\"
                  }
                }
              ],
              \"attachments\": [{
                \"color\": \"danger\"
              }]
            }"
      
      - name: Create Jira Ticket
        if: success()
        env:
          JIRA_TOKEN: ${{ secrets.JIRA_TOKEN }}
          JIRA_BASE_URL: ${{ secrets.JIRA_BASE_URL }}
        run: |
          # Create incident ticket in Jira
          # (Implementation would call Jira API)
```

## Daily Summary Workflow

```yaml
name: Daily Engineering Summary

on:
  schedule:
    - cron: '0 8 * * 1-5' # Every weekday at 8 AM

jobs:
  daily-summary:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '8.0.x'
      
      - name: Generate Daily Summary
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
          JIRA_TOKEN: ${{ secrets.JIRA_TOKEN }}
          JIRA_BASE_URL: ${{ secrets.JIRA_BASE_URL }}
        run: |
          # Call SDM Assistant API
          SUMMARY=$(curl -s -X POST http://localhost:7006/api/sdm/daily-summary \
            -H "Content-Type: application/json" \
            -d "{
              \"projectKey\": \"PROJ\",
              \"date\": \"$(date -u -d '1 day ago' +%Y-%m-%d)\"
            }")
          
          echo "DAILY_SUMMARY=$SUMMARY" >> $GITHUB_ENV
      
      - name: Generate Standup Points
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          POINTS=$(curl -s -X POST http://localhost:7006/api/sdm/standup-talking-points \
            -H "Content-Type: application/json" \
            -d "$DAILY_SUMMARY")
          
          echo "STANDUP_POINTS=$POINTS" >> $GITHUB_ENV
      
      - name: Send to Slack
        env:
          SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
        run: |
          curl -X POST $SLACK_WEBHOOK_URL \
            -H 'Content-Type: application/json' \
            -d "{
              \"text\": \"📊 Daily Engineering Summary\",
              \"blocks\": [
                {
                  \"type\": \"header\",
                  \"text\": {
                    \"type\": \"plain_text\",
                    \"text\": \"📊 Daily Summary - $(date +%Y-%m-%d)\"
                  }
                },
                {
                  \"type\": \"section\",
                  \"text\": {
                    \"type\": \"mrkdwn\",
                    \"text\": \"$DAILY_SUMMARY\"
                  }
                },
                {
                  \"type\": \"divider\"
                },
                {
                  \"type\": \"section\",
                  \"text\": {
                    \"type\": \"mrkdwn\",
                    \"text\": \"*Standup Talking Points:*\n$STANDUP_POINTS\"
                  }
                }
              ]
            }"
```

## Security Scan Notification

```yaml
name: Security Scan and Notify

on:
  pull_request:
    paths:
      - '**/*.tf'
      - '**/Dockerfile*'
      - '**/kubernetes/**'

jobs:
  security-scan:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      
      - name: Security Scan
        env:
          OPENAI_API_KEY: ${{ secrets.OPENAI_API_KEY }}
        run: |
          # Scan changed files
          SCAN_RESULT=$(curl -s -X POST http://localhost:7007/api/devops/security-scan \
            -H "Content-Type: application/json" \
            -d "{
              \"scanTarget\": \"pull_request\",
              \"configFiles\": [\"$(git diff --name-only origin/${{ github.base_ref }}...HEAD | tr '\n' ',' | sed 's/,$//')\"]
            }")
          
          echo "SCAN_RESULT=$SCAN_RESULT" >> $GITHUB_ENV
      
      - name: Notify Slack if Issues Found
        if: env.SCAN_RESULT != ''
        env:
          SLACK_WEBHOOK_URL: ${{ secrets.SLACK_WEBHOOK_URL }}
        run: |
          curl -X POST $SLACK_WEBHOOK_URL \
            -H 'Content-Type: application/json' \
            -d "{
              \"text\": \"🔒 Security Scan Results\",
              \"blocks\": [
                {
                  \"type\": \"header\",
                  \"text\": {
                    \"type\": \"plain_text\",
                    \"text\": \"🔒 Security Scan - PR #${{ github.event.pull_request.number }}\"
                  }
                },
                {
                  \"type\": \"section\",
                  \"text\": {
                    \"type\": \"mrkdwn\",
                    \"text\": \"$SCAN_RESULT\"
                  }
                },
                {
                  \"type\": \"section\",
                  \"text\": {
                    \"type\": \"mrkdwn\",
                    \"text\": \"<${{ github.event.pull_request.html_url }}|View PR>\"
                  }
                }
              ],
              \"attachments\": [{
                \"color\": \"warning\"
              }]
            }"
```

## Best Practices

1. **Use Secrets**: Store webhook URLs and tokens in GitHub Secrets
2. **Conditional Notifications**: Use `if: success()` or `if: failure()` appropriately
3. **Rich Formatting**: Use Slack blocks for better readability
4. **Include Links**: Always include links to PRs, runs, or commits
5. **Rate Limiting**: Be mindful of Slack rate limits

## Setup

1. Add secrets to your GitHub repository:
   - `SLACK_WEBHOOK_URL`: Your Slack webhook URL
   - `OPENAI_API_KEY`: Your OpenAI API key
   - `JIRA_TOKEN`: (Optional) For Jira integration
   - `GITHUB_TOKEN`: Automatically provided

2. Copy workflow files to `.github/workflows/` in your repository

3. Customize channels and messages as needed

See [Slack Integration Guide](../../docs/integrations/slack-integration.md) for more details.
