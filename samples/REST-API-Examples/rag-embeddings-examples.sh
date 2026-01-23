#!/bin/bash

# RAG and Vector Embeddings Examples
# Comprehensive examples for creating embeddings and using RAG (Retrieval-Augmented Generation)

# Configuration
BASE_URL="${BASE_URL:-http://localhost:5001}"
API_KEY="${API_KEY:-your-api-key-here}"
OPENAI_API_KEY="${OPENAI_API_KEY:-your-openai-api-key}"

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo -e "${BLUE}=== RAG and Vector Embeddings Examples ===${NC}\n"

# ============================================================================
# DIRECT OPENAI API: CREATE EMBEDDINGS
# ============================================================================
echo -e "${GREEN}1. Create Embeddings - Single Text${NC}"
curl -s -X POST "https://api.openai.com/v1/embeddings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${OPENAI_API_KEY}" \
  -d '{
    "model": "text-embedding-ada-002",
    "input": "Patient education: Metformin is used to treat type 2 diabetes by helping control blood sugar levels."
  }' | jq '{
    model: .model,
    embedding_length: .data[0].embedding | length,
    first_10_values: .data[0].embedding[0:10],
    usage: .usage
  }'

echo -e "\n${GREEN}2. Create Embeddings - Multiple Texts (Batch)${NC}"
curl -s -X POST "https://api.openai.com/v1/embeddings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${OPENAI_API_KEY}" \
  -d '{
    "model": "text-embedding-ada-002",
    "input": [
      "The system must allow users to login securely using multi-factor authentication.",
      "Users can update their profile information including email and password.",
      "The dashboard displays personalized content based on user preferences."
    ]
  }' | jq '{
    model: .model,
    count: .data | length,
    embedding_lengths: [.data[] | .embedding | length],
    usage: .usage
  }'

# ============================================================================
# RAG WORKFLOW: CREATE DOCUMENT EMBEDDINGS
# ============================================================================
echo -e "\n${GREEN}3. RAG Workflow - Step 1: Create Document Embeddings${NC}"
echo "Creating embeddings for a knowledge base of requirements documents..."

# Simulate creating embeddings for multiple documents
DOC1_EMBEDDING=$(curl -s -X POST "https://api.openai.com/v1/embeddings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${OPENAI_API_KEY}" \
  -d '{
    "model": "text-embedding-ada-002",
    "input": "Security Requirements: The system must implement multi-factor authentication, encrypt sensitive data at rest and in transit, and maintain audit logs for all user actions."
  }' | jq -r '.data[0].embedding | @json')

DOC2_EMBEDDING=$(curl -s -X POST "https://api.openai.com/v1/embeddings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${OPENAI_API_KEY}" \
  -d '{
    "model": "text-embedding-ada-002",
    "input": "User Management: Users can create accounts, update profiles, reset passwords, and manage notification preferences. Administrators can manage user roles and permissions."
  }' | jq -r '.data[0].embedding | @json')

DOC3_EMBEDDING=$(curl -s -X POST "https://api.openai.com/v1/embeddings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${OPENAI_API_KEY}" \
  -d '{
    "model": "text-embedding-ada-002",
    "input": "Dashboard Features: The dashboard displays personalized content, recent activity, notifications, and quick access to frequently used features. Users can customize their dashboard layout."
  }' | jq -r '.data[0].embedding | @json')

echo "✓ Created embeddings for 3 documents"
echo "  - Document 1: Security Requirements"
echo "  - Document 2: User Management"
echo "  - Document 3: Dashboard Features"

# ============================================================================
# RAG WORKFLOW: FIND SIMILAR DOCUMENTS
# ============================================================================
echo -e "\n${GREEN}4. RAG Workflow - Step 2: Find Similar Documents${NC}"
echo "Query: 'How do users authenticate?'"

# Create query embedding
QUERY_EMBEDDING=$(curl -s -X POST "https://api.openai.com/v1/embeddings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${OPENAI_API_KEY}" \
  -d '{
    "model": "text-embedding-ada-002",
    "input": "How do users authenticate?"
  }' | jq -r '.data[0].embedding | @json')

echo "✓ Created query embedding"
echo "Note: In production, you would calculate cosine similarity between query embedding"
echo "      and document embeddings to find the most similar documents."

# ============================================================================
# RAG WORKFLOW: QUERY WITH RAG (Complete Example)
# ============================================================================
echo -e "\n${GREEN}5. RAG Workflow - Step 3: Query with RAG${NC}"
echo "Using RAG to answer: 'What are the security requirements?'"

# This simulates the RAG process:
# 1. Find similar documents (we'll use the security doc as context)
# 2. Generate answer using GPT with context

curl -s -X POST "https://api.openai.com/v1/chat/completions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${OPENAI_API_KEY}" \
  -d '{
    "model": "gpt-4-turbo-preview",
    "messages": [
      {
        "role": "system",
        "content": "You are a helpful assistant that answers questions based on provided context. If the answer is not in the context, say so."
      },
      {
        "role": "user",
        "content": "Context:\n[Security Requirements]\nSecurity Requirements: The system must implement multi-factor authentication, encrypt sensitive data at rest and in transit, and maintain audit logs for all user actions.\n\nQuestion: What are the security requirements?"
      }
    ],
    "temperature": 0.3,
    "max_tokens": 500
  }' | jq '{
    answer: .choices[0].message.content,
    tokens_used: .usage.total_tokens
  }'

# ============================================================================
# PHARMACY EXAMPLE: DRUG INFORMATION RAG
# ============================================================================
echo -e "\n${GREEN}6. Pharmacy RAG Example - Drug Information Search${NC}"

# Create embeddings for drug information
echo "Creating embeddings for drug information documents..."

DRUG1_EMBEDDING=$(curl -s -X POST "https://api.openai.com/v1/embeddings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${OPENAI_API_KEY}" \
  -d '{
    "model": "text-embedding-ada-002",
    "input": "Metformin: Used to treat type 2 diabetes. Common side effects include nausea, diarrhea, and stomach upset. Take with meals to reduce side effects. Do not take with alcohol."
  }' | jq -r '.data[0].embedding | @json')

DRUG2_EMBEDDING=$(curl -s -X POST "https://api.openai.com/v1/embeddings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${OPENAI_API_KEY}" \
  -d '{
    "model": "text-embedding-ada-002",
    "input": "Lisinopril: Used to treat high blood pressure and heart failure. Common side effects include dizziness, cough, and fatigue. Avoid potassium supplements unless directed by doctor."
  }' | jq -r '.data[0].embedding | @json')

echo "✓ Created embeddings for drug information"

# Query about drug interactions
echo "Querying: 'Can Metformin be taken with Lisinopril?'"

curl -s -X POST "https://api.openai.com/v1/chat/completions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${OPENAI_API_KEY}" \
  -d '{
    "model": "gpt-4-turbo-preview",
    "messages": [
      {
        "role": "system",
        "content": "You are a pharmacist assistant. Answer questions about medications based on provided context. Always recommend consulting a healthcare provider."
      },
      {
        "role": "user",
        "content": "Context:\n[Metformin Info]\nMetformin: Used to treat type 2 diabetes. Common side effects include nausea, diarrhea, and stomach upset. Take with meals to reduce side effects. Do not take with alcohol.\n\n[Lisinopril Info]\nLisinopril: Used to treat high blood pressure and heart failure. Common side effects include dizziness, cough, and fatigue. Avoid potassium supplements unless directed by doctor.\n\nQuestion: Can Metformin be taken with Lisinopril?"
      }
    ],
    "temperature": 0.3,
    "max_tokens": 500
  }' | jq '{
    answer: .choices[0].message.content,
    tokens_used: .usage.total_tokens
  }'

# ============================================================================
# PUBLISHING EXAMPLE: MANUSCRIPT SEARCH WITH RAG
# ============================================================================
echo -e "\n${GREEN}7. Publishing RAG Example - Manuscript Chapter Search${NC}"

# Create embeddings for manuscript chapters
echo "Creating embeddings for manuscript chapters..."

CHAPTER1_EMBEDDING=$(curl -s -X POST "https://api.openai.com/v1/embeddings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${OPENAI_API_KEY}" \
  -d '{
    "model": "text-embedding-ada-002",
    "input": "Chapter 1: The hero arrives in a small town where nothing ever happens. He is a detective investigating a mysterious disappearance. The townspeople are wary of outsiders."
  }' | jq -r '.data[0].embedding | @json')

CHAPTER2_EMBEDDING=$(curl -s -X POST "https://api.openai.com/v1/embeddings" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${OPENAI_API_KEY}" \
  -d '{
    "model": "text-embedding-ada-002",
    "input": "Chapter 5: The detective discovers clues pointing to a conspiracy. The character development shows his growing determination and attention to detail. He begins to trust his instincts."
  }' | jq -r '.data[0].embedding | @json')

echo "✓ Created embeddings for manuscript chapters"

# Find chapters about character development
echo "Querying: 'Find chapters about character development'"

curl -s -X POST "https://api.openai.com/v1/chat/completions" \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer ${OPENAI_API_KEY}" \
  -d '{
    "model": "gpt-4-turbo-preview",
    "messages": [
      {
        "role": "system",
        "content": "You are a literary assistant. Help find relevant chapters based on provided context."
      },
      {
        "role": "user",
        "content": "Context:\n[Chapter 5]\nChapter 5: The detective discovers clues pointing to a conspiracy. The character development shows his growing determination and attention to detail. He begins to trust his instincts.\n\nQuestion: Find chapters about character development"
      }
    ],
    "temperature": 0.3,
    "max_tokens": 300
  }' | jq '{
    answer: .choices[0].message.content,
    tokens_used: .usage.total_tokens
  }'

# ============================================================================
# REQUIREMENTS ASSISTANT: RAG Q&A
# ============================================================================
echo -e "\n${GREEN}8. Requirements Assistant - RAG Q&A${NC}"
curl -X POST "${BASE_URL}/api/requirements/answer-question" \
  -H "Content-Type: application/json" \
  -H "X-API-Key: ${API_KEY}" \
  -d '{
    "question": "What are the main features?",
    "documentContent": "The system includes user authentication with multi-factor authentication, profile management where users can update their information, and a personalized dashboard that displays content based on user preferences."
  }' | jq '.'

# ============================================================================
# COMPLETE RAG WORKFLOW EXAMPLE
# ============================================================================
echo -e "\n${GREEN}9. Complete RAG Workflow Example${NC}"
echo "This demonstrates the full RAG workflow:"
echo "  1. Create embeddings for multiple documents"
echo "  2. Store embeddings (in production, use a vector database)"
echo "  3. Create query embedding"
echo "  4. Find similar documents using cosine similarity"
echo "  5. Generate answer using GPT with relevant context"

cat << 'EOF'

# Complete RAG Workflow (Python-like pseudocode):

# Step 1: Create document embeddings
documents = [
    {"title": "Doc 1", "content": "..."},
    {"title": "Doc 2", "content": "..."},
    {"title": "Doc 3", "content": "..."}
]
embeddings = create_embeddings(documents)

# Step 2: Store in vector database (e.g., Pinecone, Weaviate, Qdrant)
vector_db.store(embeddings)

# Step 3: Query
query = "What are the security requirements?"
query_embedding = create_embedding(query)

# Step 4: Find similar documents
similar_docs = vector_db.similarity_search(query_embedding, top_k=3)

# Step 5: Generate answer with context
context = "\n\n".join([doc["content"] for doc in similar_docs])
answer = gpt.generate(query, context=context)

EOF

echo -e "\n${YELLOW}Note: In production, use a vector database like:${NC}"
echo "  - Pinecone (managed vector database)"
echo "  - Weaviate (open-source vector database)"
echo "  - Qdrant (open-source vector search engine)"
echo "  - Azure Cognitive Search (with vector support)"
echo "  - PostgreSQL with pgvector extension"

echo -e "\n${BLUE}=== Examples Complete ===${NC}"
echo "Set environment variables to customize:"
echo "  export BASE_URL=http://localhost:5001"
echo "  export API_KEY=your-api-key"
echo "  export OPENAI_API_KEY=your-openai-api-key"
