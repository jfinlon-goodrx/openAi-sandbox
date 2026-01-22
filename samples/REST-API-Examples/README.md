# REST API Examples

These examples can be used **without a development environment**. They demonstrate how to interact with the APIs using standard HTTP tools like `curl`, Postman, or any HTTP client.

## Prerequisites

- An HTTP client (curl, Postman, Insomnia, or browser)
- OpenAI API key (if calling OpenAI directly)
- Or a running instance of the API (if using the project APIs)

## Table of Contents

1. [Direct OpenAI API Examples](#direct-openai-api-examples) - Call OpenAI directly
2. [Project API Examples](#project-api-examples) - Use the project APIs (requires running services)
3. [Postman Collection](#postman-collection) - Import into Postman

---

## Direct OpenAI API Examples

These examples call OpenAI APIs directly, no development environment needed.

### Chat Completions (GPT-4)

```bash
curl https://api.openai.com/v1/chat/completions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_OPENAI_API_KEY" \
  -d '{
    "model": "gpt-4-turbo-preview",
    "messages": [
      {
        "role": "system",
        "content": "You are a helpful assistant."
      },
      {
        "role": "user",
        "content": "Summarize this requirements document: The system must allow users to login securely and manage their profiles."
      }
    ],
    "temperature": 0.3,
    "max_tokens": 500
  }'
```

### Generate User Stories

```bash
curl https://api.openai.com/v1/chat/completions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_OPENAI_API_KEY" \
  -d '{
    "model": "gpt-4-turbo-preview",
    "messages": [
      {
        "role": "user",
        "content": "Generate user stories from: Users need to login, view their dashboard, and update their profile. Format as JSON array with title, asA, iWant, soThat fields."
      }
    ],
    "response_format": {"type": "json_object"},
    "temperature": 0.3
  }'
```

### Vision API - Analyze Image

```bash
curl https://api.openai.com/v1/chat/completions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_OPENAI_API_KEY" \
  -d '{
    "model": "gpt-4-vision-preview",
    "messages": [
      {
        "role": "user",
        "content": [
          {
            "type": "text",
            "text": "Analyze this book cover. Evaluate visual appeal, typography, and marketability."
          },
          {
            "type": "image_url",
            "image_url": {
              "url": "https://example.com/book-cover.jpg"
            }
          }
        ]
      }
    ],
    "max_tokens": 300
  }'
```

### Moderation API

```bash
curl https://api.openai.com/v1/moderations \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_OPENAI_API_KEY" \
  -d '{
    "input": "This is a test message to check for inappropriate content."
  }'
```

### Embeddings

```bash
curl https://api.openai.com/v1/embeddings \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_OPENAI_API_KEY" \
  -d '{
    "model": "text-embedding-ada-002",
    "input": "Patient education: Metformin is used to treat type 2 diabetes."
  }'
```

### Whisper API - Transcribe Audio

```bash
curl https://api.openai.com/v1/audio/transcriptions \
  -H "Authorization: Bearer YOUR_OPENAI_API_KEY" \
  -F file="@meeting.mp3" \
  -F model="whisper-1" \
  -F language="en"
```

### DALL-E - Generate Image

```bash
curl https://api.openai.com/v1/images/generations \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer YOUR_OPENAI_API_KEY" \
  -d '{
    "model": "dall-e-3",
    "prompt": "A futuristic book cover for a science fiction novel, dark space background with stars, title in bold modern typography",
    "size": "1024x1024",
    "quality": "standard",
    "n": 1
  }'
```

---

## Project API Examples

These examples use the project APIs. **First, start the API:**

```bash
cd src/PublishingAssistant/PublishingAssistant.Api
dotnet run
```

### Publishing Assistant

#### Generate Book Review

```bash
curl -X POST http://localhost:5000/api/publishing/review \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Chapter 1: The story begins in a small town...",
    "genre": "Science Fiction"
  }'
```

#### Generate Summary

```bash
curl -X POST http://localhost:5000/api/publishing/summary \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Full manuscript content...",
    "maxLength": 250
  }'
```

#### Generate Marketing Blurb

```bash
curl -X POST http://localhost:5000/api/publishing/marketing-blurb \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Book content...",
    "targetAudience": "Young adults aged 18-25"
  }'
```

#### Analyze Cover Image

```bash
curl -X POST http://localhost:5000/api/publishing/analyze-cover-image \
  -H "Content-Type: application/json" \
  -d '{
    "imageUrl": "https://example.com/cover.jpg",
    "genre": "Mystery"
  }'
```

### Pharmacy Assistant

#### Generate Patient Education

```bash
curl -X POST http://localhost:5001/api/pharmacy/patient-education \
  -H "Content-Type: application/json" \
  -d '{
    "medicationName": "Metformin",
    "condition": "Type 2 Diabetes"
  }'
```

#### Check Drug Interactions

```bash
curl -X POST http://localhost:5001/api/pharmacy/check-interactions \
  -H "Content-Type: application/json" \
  -d '{
    "medications": ["Metformin", "Lisinopril"],
    "supplements": ["Vitamin D"],
    "conditions": ["Type 2 Diabetes", "Hypertension"]
  }'
```

#### Generate Prescription Label

```bash
curl -X POST http://localhost:5001/api/pharmacy/prescription-label \
  -H "Content-Type: application/json" \
  -d '{
    "medicationName": "Metformin",
    "dosage": "500mg",
    "frequency": "Twice daily",
    "quantity": 60
  }'
```

### Advertising Agency Assistant

#### Generate Ad Copy

```bash
curl -X POST http://localhost:5002/api/advertising/ad-copy \
  -H "Content-Type: application/json" \
  -d '{
    "productName": "EcoClean Laundry Detergent",
    "productDescription": "Eco-friendly laundry detergent made from natural ingredients",
    "targetAudience": "Environmentally conscious consumers aged 25-45",
    "channel": "Social Media",
    "tone": "Friendly and informative"
  }'
```

#### Develop Campaign Strategy

```bash
curl -X POST http://localhost:5002/api/advertising/campaign-strategy \
  -H "Content-Type: application/json" \
  -d '{
    "brandName": "EcoClean",
    "productDescription": "Eco-friendly laundry detergent",
    "targetAudience": "Environmentally conscious consumers",
    "campaignObjective": "Increase brand awareness",
    "budget": 50000
  }'
```

### Requirements Assistant

#### Summarize Document

```bash
curl -X POST http://localhost:5003/api/requirements/summarize \
  -H "Content-Type: application/json" \
  -d '{
    "content": "The system must allow users to login securely using multi-factor authentication..."
  }'
```

#### Generate User Stories

```bash
curl -X POST http://localhost:5003/api/requirements/generate-user-stories \
  -H "Content-Type: application/json" \
  -d '{
    "content": "Users need to login, view dashboard, update profile..."
  }'
```

### SDM Assistant ⭐ NEW

#### Get Daily Activity Summary

```bash
curl -X POST http://localhost:7006/api/sdm/daily-summary \
  -H "Content-Type: application/json" \
  -d '{
    "projectKey": "PROJ",
    "date": "2024-01-15"
  }'
```

#### Generate Standup Talking Points

```bash
curl -X POST http://localhost:7006/api/sdm/standup-talking-points \
  -H "Content-Type: application/json" \
  -d '{
    "date": "2024-01-15T00:00:00Z",
    "summary": "Daily activity summary...",
    "ticketsAnalyzed": 12
  }'
```

#### Analyze Team Velocity

```bash
curl -X POST http://localhost:7006/api/sdm/analyze-velocity \
  -H "Content-Type: application/json" \
  -d '{
    "projectKey": "PROJ",
    "sprintCount": 5
  }'
```

#### Generate Sprint Plan

```bash
curl -X POST http://localhost:7006/api/sdm/sprint-plan \
  -H "Content-Type: application/json" \
  -d '{
    "projectKey": "PROJ",
    "sprintGoal": "Complete payment integration",
    "teamCapacity": 40
  }'
```

#### Identify Risks

```bash
curl -X POST http://localhost:7006/api/sdm/identify-risks \
  -H "Content-Type: application/json" \
  -d '{
    "projectKey": "PROJ",
    "sprintId": "123"
  }'
```

#### Generate Status Report

```bash
curl -X POST http://localhost:7006/api/sdm/status-report \
  -H "Content-Type: application/json" \
  -d '{
    "projectKey": "PROJ",
    "startDate": "2024-01-08T00:00:00Z",
    "endDate": "2024-01-15T00:00:00Z",
    "includeMetrics": true
  }'
```

### DevOps Assistant ⭐ NEW

#### Analyze Logs

```bash
curl -X POST http://localhost:7007/api/devops/analyze-logs \
  -H "Content-Type: application/json" \
  -d '{
    "logs": "Error logs content...",
    "logType": "application",
    "timeRangeHours": 1
  }'
```

#### Generate Incident Report

```bash
curl -X POST http://localhost:7007/api/devops/incident-report \
  -H "Content-Type: application/json" \
  -d '{
    "logAnalysis": { /* LogAnalysis object */ },
    "severity": "High"
  }'
```

#### Analyze CI/CD Pipeline

```bash
curl -X POST http://localhost:7007/api/devops/analyze-pipeline \
  -H "Content-Type: application/json" \
  -d '{
    "pipelineLogs": "Pipeline execution logs...",
    "pipelineType": "GitHub Actions"
  }'
```

#### Optimize Pipeline

```bash
curl -X POST http://localhost:7007/api/devops/optimize-pipeline \
  -H "Content-Type: application/json" \
  -d '{
    "pipelineAnalysis": { /* PipelineAnalysis object */ },
    "targetMetrics": ["build_time", "deployment_time"]
  }'
```

#### Review Infrastructure Code

```bash
curl -X POST http://localhost:7007/api/devops/review-infrastructure \
  -H "Content-Type: application/json" \
  -d '{
    "code": "terraform { ... }",
    "infrastructureType": "Terraform"
  }'
```

#### Generate Deployment Script

```bash
curl -X POST http://localhost:7007/api/devops/deployment-script \
  -H "Content-Type: application/json" \
  -d '{
    "applicationType": "ASP.NET Core",
    "targetEnvironment": "Azure App Service",
    "deploymentMethod": "CI/CD"
  }'
```

#### Security Scan

```bash
curl -X POST http://localhost:7007/api/devops/security-scan \
  -H "Content-Type: application/json" \
  -d '{
    "scanTarget": "infrastructure",
    "configFiles": ["terraform.tf", "docker-compose.yml"]
  }'
```

### Retrospective Analyzer

#### Extract Action Items

```bash
curl -X POST http://localhost:5004/api/retro/extract-action-items \
  -H "Content-Type: application/json" \
  -d '{
    "comments": [
      "We need better documentation",
      "Code reviews take too long",
      "Deployment process is confusing"
    ]
  }'
```

#### Analyze Sentiment

```bash
curl -X POST http://localhost:5004/api/retro/analyze-sentiment \
  -H "Content-Type: application/json" \
  -d '{
    "comments": [
      "Great sprint! We accomplished a lot.",
      "Frustrated with the slow CI pipeline.",
      "Love the new team collaboration tools."
    ]
  }'
```

### Meeting Analyzer

#### Transcribe Audio

```bash
curl -X POST http://localhost:5005/api/meeting/transcribe \
  -F "audioFile=@meeting.mp3" \
  -F "language=en"
```

#### Summarize Meeting

```bash
curl -X POST http://localhost:5005/api/meeting/summarize \
  -H "Content-Type: application/json" \
  -d '{
    "transcript": "John: We discussed the new feature requirements. Sarah: We agreed on a timeline..."
  }'
```

---

## Using with Postman

1. Import the [Postman Collection](#postman-collection) below
2. Set environment variables:
   - `base_url`: `http://localhost:5000` (or your API URL)
   - `openai_api_key`: Your OpenAI API key (for direct examples)

---

## Using with JavaScript/Fetch

```javascript
// Generate book review
const response = await fetch('http://localhost:5000/api/publishing/review', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
  },
  body: JSON.stringify({
    content: 'Chapter 1: The story begins...',
    genre: 'Science Fiction'
  })
});

const review = await response.json();
console.log(review);
```

---

## Using with Python/Requests

```python
import requests

# Generate patient education
response = requests.post(
    'http://localhost:5001/api/pharmacy/patient-education',
    json={
        'medicationName': 'Metformin',
        'condition': 'Type 2 Diabetes'
    }
)

education = response.json()
print(education)
```

---

## Notes

- Replace `YOUR_OPENAI_API_KEY` with your actual OpenAI API key
- Replace `localhost:5000` with your actual API URL if different
- For production, use HTTPS and proper authentication
- Check Swagger UI at `http://localhost:5000/swagger` for interactive API documentation
