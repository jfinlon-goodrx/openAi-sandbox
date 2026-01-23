#!/bin/bash

# OpenAI Platform Learning Portfolio - API Examples
# Updated with new features: Streaming, Authentication, Metrics, Database, Autonomous Agent

# Configuration
BASE_URL="${BASE_URL:-http://localhost:5001}"
API_KEY="${API_KEY:-your-api-key-here}"
JWT_TOKEN="${JWT_TOKEN:-}"

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== OpenAI Platform Learning Portfolio - API Examples ===${NC}\n"

# ============================================================================
# HEALTH CHECKS
# ============================================================================
echo -e "${GREEN}1. Health Check${NC}"
curl -X GET "${BASE_URL}/health" \
  -H "Content-Type: application/json" \
  -w "\nStatus: %{http_code}\n\n"

# ============================================================================
# AUTHENTICATION
# ============================================================================
echo -e "${GREEN}2. Login (Get JWT Token)${NC}"
JWT_TOKEN=$(curl -s -X POST "${BASE_URL}/api/auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "email": "user@example.com",
    "password": "password123"
  }' | jq -r '.token')

echo "JWT Token: ${JWT_TOKEN:0:50}..."
echo ""

# ============================================================================
# REQUIREMENTS ASSISTANT - Standard Endpoints
# ============================================================================
echo -e "${GREEN}3. Summarize Requirements Document${NC}"
curl -X POST "${BASE_URL}/api/requirements/summarize" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: ${API_KEY}" \
  -d '{
    "content": "The system shall allow users to login and manage their profiles. Users can update their email addresses and passwords."
  }' | jq '.'

echo -e "\n${GREEN}4. Generate User Stories${NC}"
curl -X POST "${BASE_URL}/api/requirements/generate-user-stories" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: ${API_KEY}" \
  -d '{
    "content": "Users need to login and view their dashboard with personalized content."
  }' | jq '.'

echo -e "\n${GREEN}5. Answer Question (RAG)${NC}"
curl -X POST "${BASE_URL}/api/requirements/answer-question" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: ${API_KEY}" \
  -d '{
    "question": "What are the main features?",
    "documentContent": "The system includes user authentication, profile management, and dashboard features."
  }' | jq '.'

echo -e "\n${YELLOW}Note: For comprehensive RAG examples with embeddings, see:${NC}"
echo "  - rag-embeddings-examples.sh (curl examples)"
echo "  - rag-embeddings-examples.py (Python examples)"

# ============================================================================
# STREAMING ENDPOINTS
# ============================================================================
echo -e "\n${GREEN}6. Stream Requirements Summarization${NC}"
echo "Streaming response (Server-Sent Events):"
curl -N -X POST "${BASE_URL}/api/streaming/summarize-stream" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: ${API_KEY}" \
  -d '{
    "content": "The system shall allow users to login and manage their profiles."
  }'

echo -e "\n\n"

# ============================================================================
# METRICS ENDPOINTS
# ============================================================================
echo -e "${GREEN}7. Get All Metrics${NC}"
curl -X GET "${BASE_URL}/api/metrics" \
  -H "X-API-Key: ${API_KEY}" \
  | jq '.'

echo -e "\n${GREEN}8. Get Metrics for Specific Model${NC}"
curl -X GET "${BASE_URL}/api/metrics/gpt-4-turbo-preview" \
  -H "X-API-Key: ${API_KEY}" \
  | jq '.'

# ============================================================================
# DATABASE ENDPOINTS
# ============================================================================
echo -e "\n${GREEN}9. Get All User Stories from Database${NC}"
curl -X GET "${BASE_URL}/api/database/user-stories" \
  -H "X-API-Key: ${API_KEY}" \
  | jq '.'

echo -e "\n${GREEN}10. Get Requirement Documents${NC}"
curl -X GET "${BASE_URL}/api/database/requirement-documents" \
  -H "X-API-Key: ${API_KEY}" \
  | jq '.'

# ============================================================================
# AUTONOMOUS DEVELOPMENT AGENT
# ============================================================================
echo -e "\n${GREEN}11. Analyze Code (Autonomous Agent)${NC}"
curl -X POST "${BASE_URL}/api/autonomousagent/analyze" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: ${API_KEY}" \
  -d '{
    "code": "public class UserService { public User GetUser(int id) { return _users.First(u => u.Id == id); } }",
    "context": "User service for authentication"
  }' | jq '.'

echo -e "\n${GREEN}12. Execute Autonomous Workflow${NC}"
curl -X POST "${BASE_URL}/api/autonomousagent/workflow" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: ${API_KEY}" \
  -d '{
    "code": "public class UserService { public User GetUser(int id) { return _users.First(u => u.Id == id); } }",
    "context": "User service for authentication",
    "repository": "org/repo",
    "filePath": "src/Services/UserService.cs"
  }' | jq '.'

# ============================================================================
# RETROSPECTIVE ANALYZER (with SignalR note)
# ============================================================================
echo -e "\n${GREEN}13. Analyze Retrospective${NC}"
curl -X POST "${BASE_URL}/api/retro/analyze" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: ${API_KEY}" \
  -d '{
    "comments": [
      "We need better documentation",
      "Deployments are taking too long",
      "Great collaboration this sprint"
    ]
  }' | jq '.'

echo -e "\n${YELLOW}Note: For real-time updates, connect to SignalR hub at: ws://localhost:5004/retroHub${NC}"

# ============================================================================
# MEETING ANALYZER
# ============================================================================
echo -e "\n${GREEN}14. Transcribe Meeting Audio${NC}"
echo "Note: Requires audio file upload"
curl -X POST "${BASE_URL}/api/meeting/transcribe" \
  -H "X-API-Key: ${API_KEY}" \
  -F "file=@meeting.mp3" \
  -F "language=en" \
  | jq '.'

# ============================================================================
# PHARMACY ASSISTANT
# ============================================================================
echo -e "\n${GREEN}15. Generate Patient Education${NC}"
curl -X POST "${BASE_URL}/api/pharmacy/patient-education" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: ${API_KEY}" \
  -d '{
    "medication": "Metformin",
    "dosage": "500mg twice daily",
    "patientAge": 45
  }' | jq '.'

# ============================================================================
# PUBLISHING ASSISTANT
# ============================================================================
echo -e "\n${GREEN}16. Generate Book Review${NC}"
curl -X POST "${BASE_URL}/api/publishing/review" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: ${API_KEY}" \
  -d '{
    "bookTitle": "The Future of AI",
    "bookContent": "This book explores the potential of artificial intelligence..."
  }' | jq '.'

echo -e "\n${GREEN}17. Upload PDF for Senior Agent Review${NC}"
echo "Uploading PDF manuscript for comprehensive review..."
curl -X POST "${BASE_URL}/api/publishing/review-pdf?genre=Science Fiction" \
  -H "X-API-Key: ${API_KEY}" \
  -F "pdfFile=@manuscript.pdf" \
  | jq '.'

echo -e "\n${YELLOW}Note: PDF review uses intelligent chunking and RAG to minimize token usage${NC}"
echo "  - Document is chunked intelligently (preserving chapter/paragraph boundaries)"
echo "  - Chunks are stored locally for reference"
echo "  - RAG is used to analyze specific aspects"
echo "  - Only relevant chunks are sent to GPT for analysis"

# ============================================================================
# ADVERTISING ASSISTANT
# ============================================================================
echo -e "\n${GREEN}17. Generate Ad Copy${NC}"
curl -X POST "${BASE_URL}/api/advertising/ad-copy" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: ${API_KEY}" \
  -d '{
    "product": "Smart Watch",
    "targetAudience": "Tech enthusiasts aged 25-40",
    "tone": "Modern and exciting"
  }' | jq '.'

# ============================================================================
# RATE LIMITING TEST
# ============================================================================
echo -e "\n${GREEN}18. Test Rate Limiting${NC}"
echo "Making multiple rapid requests to test rate limiting..."
for i in {1..5}; do
  echo "Request $i:"
  curl -s -X GET "${BASE_URL}/api/metrics" \
    -H "X-API-Key: ${API_KEY}" \
    -w " Status: %{http_code}\n" \
    | head -1
done

echo -e "\n${YELLOW}Note: Check X-RateLimit-Limit and X-RateLimit-Remaining headers${NC}"

# ============================================================================
# CORS TEST
# ============================================================================
echo -e "\n${GREEN}19. Test CORS${NC}"
curl -X OPTIONS "${BASE_URL}/api/requirements/summarize" \
  -H "Origin: http://localhost:3000" \
  -H "Access-Control-Request-Method: POST" \
  -v 2>&1 | grep -i "access-control"

# ============================================================================
# CORRELATION ID TEST
# ============================================================================
echo -e "\n${GREEN}20. Test Correlation ID${NC}"
CORRELATION_ID=$(uuidgen)
curl -X GET "${BASE_URL}/api/metrics" \
  -H "X-API-Key: ${API_KEY}" \
  -H "X-Correlation-ID: ${CORRELATION_ID}" \
  -v 2>&1 | grep -i "correlation"

echo -e "\n${BLUE}=== Examples Complete ===${NC}"
echo "Set environment variables to customize:"
echo "  export BASE_URL=http://localhost:5001"
echo "  export API_KEY=your-api-key"
echo "  export JWT_TOKEN=your-jwt-token"
