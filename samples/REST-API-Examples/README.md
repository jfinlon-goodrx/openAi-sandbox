# REST API Examples - No Development Environment Required

Complete examples for testing OpenAI Platform Learning Portfolio APIs without needing a .NET development environment. Updated with all new features including streaming, authentication, metrics, database, and autonomous agent endpoints.

## Quick Start

### Option 1: curl (Bash)

```bash
# Set your API key
export API_KEY="your-api-key-here"
export BASE_URL="http://localhost:5001"

# Run examples
bash openai-platform-examples.sh
```

### Option 2: Python

```bash
# Install dependencies
pip install requests

# Set environment variables
export API_KEY="your-api-key-here"
export BASE_URL="http://localhost:5001"

# Run examples
python python-examples.py
```

### Option 3: Postman

1. Import `postman-collection.json` into Postman
2. Set collection variables:
   - `base_url`: `http://localhost:5001`
   - `api_key`: Your API key
3. Run requests from the collection

## New Features Examples

### 1. Health Checks

```bash
# Basic health check
curl http://localhost:5001/health

# Readiness check
curl http://localhost:5001/health/ready
```

### 2. Authentication

#### API Key Authentication

```bash
curl -X POST http://localhost:5001/api/requirements/summarize \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{"content": "System requirements..."}'
```

#### JWT Authentication

```bash
# Login to get JWT token
TOKEN=$(curl -s -X POST http://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "user@example.com", "password": "password123"}' \
  | jq -r '.token')

# Use token in subsequent requests
curl -X GET http://localhost:5001/api/auth/me \
  -H "Authorization: Bearer $TOKEN"
```

### 3. Streaming Responses (Server-Sent Events)

```bash
curl -N -X POST http://localhost:5001/api/streaming/summarize-stream \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -H "Accept: text/event-stream" \
  -d '{"content": "System requirements..."}'
```

**Python Example:**
```python
import requests

response = requests.post(
    "http://localhost:5001/api/streaming/summarize-stream",
    json={"content": "System requirements..."},
    headers={
        "X-API-Key": "your-api-key",
        "Accept": "text/event-stream"
    },
    stream=True
)

for line in response.iter_lines():
    if line:
        decoded = line.decode('utf-8')
        if decoded.startswith('data: '):
            data = decoded[6:]  # Remove 'data: ' prefix
            if data == '[DONE]':
                break
            print(data)
```

### 4. Metrics Endpoints

```bash
# Get all metrics
curl http://localhost:5001/api/metrics \
  -H "X-API-Key: your-api-key"

# Get metrics for specific model
curl http://localhost:5001/api/metrics/gpt-4-turbo-preview \
  -H "X-API-Key: your-api-key"

# Reset metrics
curl -X POST http://localhost:5001/api/metrics/reset \
  -H "X-API-Key: your-api-key"
```

### 5. Database Endpoints

```bash
# Get all user stories
curl http://localhost:5001/api/database/user-stories \
  -H "X-API-Key: your-api-key"

# Get user story by ID
curl http://localhost:5001/api/database/user-stories/{id} \
  -H "X-API-Key: your-api-key"

# Get requirement documents
curl http://localhost:5001/api/database/requirement-documents \
  -H "X-API-Key: your-api-key"
```

### 6. Autonomous Development Agent

```bash
# Analyze code
curl -X POST http://localhost:5001/api/autonomousagent/analyze \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "code": "public class UserService { public User GetUser(int id) { return _users.First(u => u.Id == id); } }",
    "context": "User service for authentication"
  }'

# Execute full autonomous workflow
curl -X POST http://localhost:5001/api/autonomousagent/workflow \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "code": "public class UserService { ... }",
    "context": "User service",
    "repository": "org/repo",
    "filePath": "src/Services/UserService.cs"
  }'
```

### 7. SignalR Real-Time (WebSocket)

**Note:** SignalR requires a WebSocket client. Use JavaScript or a SignalR client library.

**JavaScript Example:**
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5004/retroHub")
    .build();

await connection.start();
await connection.invoke("JoinRoom", "retro-room-123");

connection.on("ProgressUpdate", (data) => {
    console.log("Progress:", data.message);
});

connection.on("AnalysisComplete", (result) => {
    console.log("Analysis:", result);
});
```

## Complete API Reference

### Requirements Assistant

#### Summarize Requirements
```bash
curl -X POST http://localhost:5001/api/requirements/summarize \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{"content": "System requirements document text..."}'
```

#### Generate User Stories
```bash
curl -X POST http://localhost:5001/api/requirements/generate-user-stories \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{"content": "Users need to login and view dashboard..."}'
```

#### Answer Question (RAG)
```bash
curl -X POST http://localhost:5001/api/requirements/answer-question \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "question": "What are the main features?",
    "context": "System includes authentication, profiles, dashboard..."
  }'
```

### Retrospective Analyzer

#### Analyze Retrospective
```bash
curl -X POST http://localhost:5001/api/retro/analyze \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "comments": [
      "We need better documentation",
      "Deployments are taking too long",
      "Great collaboration this sprint"
    ]
  }'
```

#### Analyze with Real-Time Updates (SignalR)
```bash
# Start analysis (triggers SignalR events)
curl -X POST http://localhost:5001/api/retro/analyze-stream \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "comments": ["We need better documentation"],
    "roomId": "retro-room-123"
  }'

# Connect to SignalR hub at: ws://localhost:5004/retroHub
```

### Meeting Analyzer

#### Transcribe Audio
```bash
curl -X POST http://localhost:5001/api/meeting/transcribe \
  -H "X-API-Key: your-api-key" \
  -F "file=@meeting.mp3" \
  -F "language=en"
```

#### Analyze Transcript
```bash
curl -X POST http://localhost:5001/api/meeting/analyze \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{"transcript": "Meeting transcript text..."}'
```

### Pharmacy Assistant

#### Generate Patient Education
```bash
curl -X POST http://localhost:5001/api/pharmacy/patient-education \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "medication": "Metformin",
    "dosage": "500mg twice daily",
    "patientAge": 45
  }'
```

#### Check Drug Interactions
```bash
curl -X POST http://localhost:5001/api/pharmacy/drug-interactions \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "medications": ["Metformin", "Aspirin"],
    "patientAge": 45
  }'
```

### Publishing Assistant

#### Generate Book Review
```bash
curl -X POST http://localhost:5001/api/publishing/review \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "bookTitle": "The Future of AI",
    "bookContent": "Book content text..."
  }'
```

#### Generate Marketing Blurb
```bash
curl -X POST http://localhost:5001/api/publishing/marketing-blurb \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "bookTitle": "The Future of AI",
    "bookSummary": "Book summary..."
  }'
```

### Advertising Assistant

#### Generate Ad Copy
```bash
curl -X POST http://localhost:5001/api/advertising/ad-copy \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "product": "Smart Watch",
    "targetAudience": "Tech enthusiasts aged 25-40",
    "tone": "Modern and exciting"
  }'
```

#### Develop Campaign Strategy
```bash
curl -X POST http://localhost:5001/api/advertising/campaign-strategy \
  -H "Content-Type: application/json" \
  -H "X-API-Key: your-api-key" \
  -d '{
    "product": "Smart Watch",
    "budget": "$50,000",
    "timeline": "3 months"
  }'
```

## Testing New Features

### Rate Limiting

```bash
# Make multiple rapid requests to test rate limiting
for i in {1..10}; do
  curl -s -X GET http://localhost:5001/api/metrics \
    -H "X-API-Key: your-api-key" \
    -w " Status: %{http_code}, Remaining: %{header}X-RateLimit-Remaining\n"
done
```

**Check Response Headers:**
- `X-RateLimit-Limit`: Maximum requests allowed
- `X-RateLimit-Remaining`: Remaining requests in window
- `Retry-After`: Seconds until refill (when limit exceeded)

### Correlation IDs

```bash
# Send request with correlation ID
CORRELATION_ID=$(uuidgen)
curl -X GET http://localhost:5001/api/metrics \
  -H "X-API-Key: your-api-key" \
  -H "X-Correlation-ID: $CORRELATION_ID" \
  -v 2>&1 | grep -i "correlation"
```

**Response Header:**
- `X-Correlation-ID`: Correlation ID for request tracking

### CORS Testing

```bash
# Test CORS preflight
curl -X OPTIONS http://localhost:5001/api/requirements/summarize \
  -H "Origin: http://localhost:3000" \
  -H "Access-Control-Request-Method: POST" \
  -v
```

### Response Compression

```bash
# Request with compression
curl -X GET http://localhost:5001/api/metrics \
  -H "X-API-Key: your-api-key" \
  -H "Accept-Encoding: gzip" \
  --compressed
```

## Environment Variables

Set these environment variables before running examples:

```bash
export BASE_URL="http://localhost:5001"
export API_KEY="your-api-key-here"
export JWT_TOKEN=""  # Will be set after login
```

## Response Headers Reference

### Standard Headers
- `X-Correlation-ID`: Request correlation ID
- `X-RateLimit-Limit`: Rate limit maximum
- `X-RateLimit-Remaining`: Remaining requests
- `Retry-After`: Seconds until refill (when rate limited)
- `Content-Encoding`: Compression type (gzip, br)

### Health Check Headers
- `Content-Type`: `application/json` or `text/plain`

### Streaming Headers
- `Content-Type`: `text/event-stream`
- `Cache-Control`: `no-cache`
- `Connection`: `keep-alive`

## Error Responses

### 400 Bad Request
```json
{
  "error": "Invalid input",
  "details": "..."
}
```

### 401 Unauthorized
```json
{
  "error": "Invalid API key"
}
```

### 429 Too Many Requests
```json
{
  "error": "Rate limit exceeded"
}
```

**Headers:**
- `Retry-After`: Seconds to wait before retrying

### 500 Internal Server Error
```json
{
  "error": "Internal server error",
  "correlationId": "abc-123"
}
```

## Postman Collection

The Postman collection includes:

1. **Health Checks** - Basic and readiness checks
2. **Authentication** - Login and JWT endpoints
3. **Requirements Assistant** - All requirements endpoints
4. **Streaming** - Server-Sent Events examples
5. **Metrics** - Metrics tracking endpoints
6. **Database** - Database query endpoints
7. **Autonomous Development Agent** - Code analysis and workflow
8. **Retrospective Analyzer** - Analysis endpoints
9. **Meeting Analyzer** - Transcription and analysis
10. **Pharmacy Assistant** - Patient education and interactions
11. **Publishing Assistant** - Reviews and marketing
12. **Advertising Assistant** - Ad copy and campaigns
13. **Testing & Monitoring** - Rate limiting, CORS, correlation IDs

### Postman Setup

1. Import `postman-collection.json`
2. Set collection variables:
   - `base_url`: Your API base URL
   - `api_key`: Your API key
3. For JWT authentication:
   - Run "Login" request first
   - Token is automatically saved to `jwt_token` variable
   - Subsequent requests use Bearer token authentication

## Python Examples

The `python-examples.py` script includes:

- All API endpoints
- Streaming support
- Error handling
- Rate limiting tests
- Correlation ID tests
- Pretty-printed responses

### Running Python Examples

```bash
# Install dependencies
pip install requests

# Run all examples
python python-examples.py

# Or import and use individual functions
python -c "from python_examples import *; summarize_requirements()"
```

## curl Script

The `openai-platform-examples.sh` script includes:

- All endpoints organized by category
- Health checks
- Authentication examples
- Streaming examples
- Metrics and database endpoints
- Autonomous agent examples
- Rate limiting and CORS tests

### Running curl Script

```bash
# Make executable
chmod +x openai-platform-examples.sh

# Run all examples
./openai-platform-examples.sh

# Or set variables and run
export BASE_URL="http://localhost:5001"
export API_KEY="your-api-key"
./openai-platform-examples.sh
```

## Direct OpenAI API Examples

For testing OpenAI APIs directly (without the .NET wrapper), see:

- **[Direct OpenAI Examples](openai-direct-examples.sh)** - curl examples for OpenAI API
- **[Python Direct Examples](python-examples.py)** - Python examples for OpenAI API

## Troubleshooting

### Connection Refused
- Ensure the API is running: `dotnet run --project src/RequirementsAssistant/RequirementsAssistant.Api`
- Check the port matches your `BASE_URL`

### 401 Unauthorized
- Verify your API key is correct
- Check `X-API-Key` header is set
- For JWT endpoints, ensure token is valid

### 429 Too Many Requests
- Rate limiting is working! Wait for the window to reset
- Check `Retry-After` header for wait time
- Adjust rate limit configuration if needed

### Streaming Not Working
- Ensure `Accept: text/event-stream` header is set
- Use `-N` flag with curl for no buffering
- Check connection is not being closed prematurely

### CORS Errors
- Verify CORS configuration in `appsettings.json`
- Check `Origin` header matches allowed origins
- Ensure preflight (OPTIONS) requests are handled

## Next Steps

- Explore the [API Documentation](http://localhost:5001/swagger) when running locally
- Check [Improvements Documentation](../../docs/improvements/) for feature details
- Review [Middleware Guide](../../docs/improvements/middleware-guide.md) for advanced features
- See [Streaming Examples](../../docs/improvements/streaming-examples.md) for real-time features

## Resources

- [OpenAI Platform Documentation](https://platform.openai.com/docs)
- [Server-Sent Events (SSE)](https://developer.mozilla.org/en-US/docs/Web/API/Server-sent_events)
- [SignalR Documentation](https://learn.microsoft.com/en-us/aspnet/core/signalr/introduction)
- [Postman Documentation](https://learning.postman.com/docs/)
