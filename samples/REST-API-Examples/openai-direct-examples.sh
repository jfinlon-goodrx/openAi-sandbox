#!/bin/bash

# Direct OpenAI API Examples
# These examples call OpenAI APIs directly - no development environment needed
# Replace YOUR_OPENAI_API_KEY with your actual API key

OPENAI_API_KEY="YOUR_OPENAI_API_KEY"
OPENAI_BASE_URL="https://api.openai.com/v1"

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== OpenAI Direct API Examples ===${NC}\n"

# Example 1: Chat Completion - Summarize Requirements
echo -e "${GREEN}Example 1: Summarize Requirements Document${NC}"
curl -s "$OPENAI_BASE_URL/chat/completions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $OPENAI_API_KEY" \
  -d '{
    "model": "gpt-4-turbo-preview",
    "messages": [
      {
        "role": "system",
        "content": "You are an expert business analyst."
      },
      {
        "role": "user",
        "content": "Summarize this requirements document: The system must allow users to login securely using multi-factor authentication, manage their profiles, and view their order history."
      }
    ],
    "temperature": 0.3,
    "max_tokens": 500
  }' | jq '.'

echo -e "\n"

# Example 2: Generate User Stories (JSON Mode)
echo -e "${GREEN}Example 2: Generate User Stories${NC}"
curl -s "$OPENAI_BASE_URL/chat/completions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $OPENAI_API_KEY" \
  -d '{
    "model": "gpt-4-turbo-preview",
    "messages": [
      {
        "role": "user",
        "content": "Generate 3 user stories from: Users need to login securely, view their dashboard, and update their profile. Return as JSON with stories array containing title, asA, iWant, soThat fields."
      }
    ],
    "response_format": {"type": "json_object"},
    "temperature": 0.3
  }' | jq '.'

echo -e "\n"

# Example 3: Moderation Check
echo -e "${GREEN}Example 3: Content Moderation${NC}"
curl -s "$OPENAI_BASE_URL/moderations" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $OPENAI_API_KEY" \
  -d '{
    "input": "This is a test message to check for inappropriate content."
  }' | jq '.'

echo -e "\n"

# Example 4: Generate Embeddings
echo -e "${GREEN}Example 4: Generate Embeddings${NC}"
curl -s "$OPENAI_BASE_URL/embeddings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $OPENAI_API_KEY" \
  -d '{
    "model": "text-embedding-ada-002",
    "input": "Patient education: Metformin is used to treat type 2 diabetes by helping control blood sugar levels."
  }' | jq '.data[0].embedding | length'

echo -e "\n"

# Example 5: Vision API - Analyze Image (requires image URL)
echo -e "${GREEN}Example 5: Vision API - Analyze Image${NC}"
echo "Note: Replace IMAGE_URL with a valid image URL"
IMAGE_URL="https://example.com/image.jpg"
curl -s "$OPENAI_BASE_URL/chat/completions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $OPENAI_API_KEY" \
  -d "{
    \"model\": \"gpt-4-vision-preview\",
    \"messages\": [
      {
        \"role\": \"user\",
        \"content\": [
          {
            \"type\": \"text\",
            \"text\": \"Describe this image in detail.\"
          },
          {
            \"type\": \"image_url\",
            \"image_url\": {
              \"url\": \"$IMAGE_URL\"
            }
          }
        ]
      }
    ],
    \"max_tokens\": 300
  }" | jq '.'

echo -e "\n"

# Example 6: DALL-E - Generate Image
echo -e "${GREEN}Example 6: DALL-E - Generate Cover Image${NC}"
curl -s "$OPENAI_BASE_URL/images/generations" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $OPENAI_API_KEY" \
  -d '{
    "model": "dall-e-3",
    "prompt": "A futuristic book cover for a science fiction novel, dark space background with stars, title in bold modern typography, professional design",
    "size": "1024x1024",
    "quality": "standard",
    "n": 1
  }' | jq '.'

echo -e "\n${BLUE}=== Examples Complete ===${NC}"
echo "Remember to replace YOUR_OPENAI_API_KEY with your actual API key!"
