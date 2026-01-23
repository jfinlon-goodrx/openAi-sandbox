#!/usr/bin/env python3
"""
RAG and Vector Embeddings Examples
Comprehensive examples for creating embeddings and using RAG (Retrieval-Augmented Generation)
"""

import os
import json
import requests
import math
from typing import List, Dict, Any, Tuple

# Configuration
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY", "your-openai-api-key")
BASE_URL = os.getenv("BASE_URL", "http://localhost:5001")
API_KEY = os.getenv("API_KEY", "your-api-key")

def print_section(title: str):
    """Print a formatted section header"""
    print(f"\n{'='*70}")
    print(f"  {title}")
    print(f"{'='*70}\n")

def create_embedding(text: str) -> List[float]:
    """Create an embedding for a single text"""
    response = requests.post(
        "https://api.openai.com/v1/embeddings",
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {OPENAI_API_KEY}"
        },
        json={
            "model": "text-embedding-ada-002",
            "input": text
        }
    )
    response.raise_for_status()
    return response.json()["data"][0]["embedding"]

def create_embeddings(texts: List[str]) -> List[List[float]]:
    """Create embeddings for multiple texts"""
    response = requests.post(
        "https://api.openai.com/v1/embeddings",
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {OPENAI_API_KEY}"
        },
        json={
            "model": "text-embedding-ada-002",
            "input": texts
        }
    )
    response.raise_for_status()
    return [item["embedding"] for item in response.json()["data"]]

def cosine_similarity(vec_a: List[float], vec_b: List[float]) -> float:
    """Calculate cosine similarity between two vectors"""
    if len(vec_a) != len(vec_b):
        raise ValueError("Vectors must have the same length")
    
    dot_product = sum(a * b for a, b in zip(vec_a, vec_b))
    magnitude_a = math.sqrt(sum(a * a for a in vec_a))
    magnitude_b = math.sqrt(sum(b * b for b in vec_b))
    
    if magnitude_a == 0 or magnitude_b == 0:
        return 0.0
    
    return dot_product / (magnitude_a * magnitude_b)

def find_similar_documents(
    query: str,
    documents: List[Dict[str, str]],
    document_embeddings: List[List[float]],
    top_k: int = 3
) -> List[Tuple[Dict[str, str], float]]:
    """Find similar documents using cosine similarity"""
    # Create query embedding
    query_embedding = create_embedding(query)
    
    # Calculate similarities
    similarities = []
    for doc, doc_embedding in zip(documents, document_embeddings):
        similarity = cosine_similarity(query_embedding, doc_embedding)
        similarities.append((doc, similarity))
    
    # Sort by similarity (descending) and return top_k
    similarities.sort(key=lambda x: x[1], reverse=True)
    return similarities[:top_k]

def query_with_rag(
    question: str,
    documents: List[Dict[str, str]],
    document_embeddings: List[List[float]],
    top_k: int = 3
) -> str:
    """Perform RAG: find similar documents and generate answer"""
    # Find similar documents
    similar_docs = find_similar_documents(question, documents, document_embeddings, top_k)
    
    # Build context from similar documents
    context = "\n\n".join([
        f"[{doc['title']}]\n{doc['content']}"
        for doc, similarity in similar_docs
    ])
    
    # Generate answer using GPT with context
    response = requests.post(
        "https://api.openai.com/v1/chat/completions",
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {OPENAI_API_KEY}"
        },
        json={
            "model": "gpt-4-turbo-preview",
            "messages": [
                {
                    "role": "system",
                    "content": "You are a helpful assistant that answers questions based on provided context. If the answer is not in the context, say so."
                },
                {
                    "role": "user",
                    "content": f"Context:\n{context}\n\nQuestion: {question}"
                }
            ],
            "temperature": 0.3,
            "max_tokens": 500
        }
    )
    response.raise_for_status()
    return response.json()["choices"][0]["message"]["content"]

# ============================================================================
# EXAMPLE 1: CREATE EMBEDDINGS
# ============================================================================
def example_create_embeddings():
    """Example: Create embeddings for single and multiple texts"""
    print_section("Example 1: Create Embeddings")
    
    # Single text
    print("Creating embedding for single text...")
    embedding = create_embedding("Patient education: Metformin is used to treat type 2 diabetes.")
    print(f"✓ Embedding created: {len(embedding)} dimensions")
    print(f"  First 10 values: {embedding[:10]}")
    
    # Multiple texts (batch)
    print("\nCreating embeddings for multiple texts...")
    texts = [
        "The system must allow users to login securely.",
        "Users can update their profile information.",
        "The dashboard displays personalized content."
    ]
    embeddings = create_embeddings(texts)
    print(f"✓ Created {len(embeddings)} embeddings")
    print(f"  Each embedding has {len(embeddings[0])} dimensions")

# ============================================================================
# EXAMPLE 2: REQUIREMENTS DOCUMENT RAG
# ============================================================================
def example_requirements_rag():
    """Example: RAG for requirements document Q&A"""
    print_section("Example 2: Requirements Document RAG")
    
    # Create documents
    documents = [
        {
            "title": "Security Requirements",
            "content": "Security Requirements: The system must implement multi-factor authentication, encrypt sensitive data at rest and in transit, and maintain audit logs for all user actions."
        },
        {
            "title": "User Management",
            "content": "User Management: Users can create accounts, update profiles, reset passwords, and manage notification preferences. Administrators can manage user roles and permissions."
        },
        {
            "title": "Dashboard Features",
            "content": "Dashboard Features: The dashboard displays personalized content, recent activity, notifications, and quick access to frequently used features. Users can customize their dashboard layout."
        }
    ]
    
    # Create embeddings
    print("Creating document embeddings...")
    texts = [doc["content"] for doc in documents]
    document_embeddings = create_embeddings(texts)
    print(f"✓ Created embeddings for {len(documents)} documents")
    
    # Find similar documents
    print("\nFinding similar documents...")
    query = "How do users authenticate?"
    similar = find_similar_documents(query, documents, document_embeddings, top_k=2)
    print(f"Query: '{query}'")
    for doc, similarity in similar:
        print(f"  - {doc['title']}: {similarity:.3f} similarity")
    
    # Query with RAG
    print("\nQuerying with RAG...")
    question = "What are the security requirements?"
    answer = query_with_rag(question, documents, document_embeddings, top_k=2)
    print(f"Question: {question}")
    print(f"Answer: {answer}")

# ============================================================================
# EXAMPLE 3: PHARMACY DRUG INFORMATION RAG
# ============================================================================
def example_pharmacy_rag():
    """Example: RAG for pharmacy drug information"""
    print_section("Example 3: Pharmacy Drug Information RAG")
    
    # Create drug information documents
    documents = [
        {
            "title": "Metformin Information",
            "content": "Metformin: Used to treat type 2 diabetes. Common side effects include nausea, diarrhea, and stomach upset. Take with meals to reduce side effects. Do not take with alcohol. Dosage typically starts at 500mg twice daily."
        },
        {
            "title": "Lisinopril Information",
            "content": "Lisinopril: Used to treat high blood pressure and heart failure. Common side effects include dizziness, cough, and fatigue. Avoid potassium supplements unless directed by doctor. May cause dry cough in some patients."
        },
        {
            "title": "Aspirin Information",
            "content": "Aspirin: Used for pain relief, fever reduction, and cardiovascular protection. Common side effects include stomach irritation and bleeding risk. Should not be taken with certain blood thinners. Low-dose aspirin (81mg) is often used for heart protection."
        }
    ]
    
    # Create embeddings
    print("Creating embeddings for drug information...")
    texts = [doc["content"] for doc in documents]
    document_embeddings = create_embeddings(texts)
    print(f"✓ Created embeddings for {len(documents)} drug documents")
    
    # Query about drug interactions
    print("\nQuerying about drug interactions...")
    question = "Can Metformin be taken with Lisinopril?"
    answer = query_with_rag(question, documents, document_embeddings, top_k=2)
    print(f"Question: {question}")
    print(f"Answer: {answer}")

# ============================================================================
# EXAMPLE 4: PUBLISHING MANUSCRIPT SEARCH
# ============================================================================
def example_publishing_rag():
    """Example: RAG for manuscript chapter search"""
    print_section("Example 4: Publishing Manuscript Search")
    
    # Create manuscript chapters
    documents = [
        {
            "title": "Chapter 1: Arrival",
            "content": "Chapter 1: The hero arrives in a small town where nothing ever happens. He is a detective investigating a mysterious disappearance. The townspeople are wary of outsiders and reluctant to share information."
        },
        {
            "title": "Chapter 5: Discovery",
            "content": "Chapter 5: The detective discovers clues pointing to a conspiracy. The character development shows his growing determination and attention to detail. He begins to trust his instincts and forms an unlikely alliance."
        },
        {
            "title": "Chapter 10: Resolution",
            "content": "Chapter 10: The mystery is solved through careful investigation and character growth. The hero's journey demonstrates resilience and the importance of trusting others. The resolution ties together all plot threads."
        }
    ]
    
    # Create embeddings
    print("Creating embeddings for manuscript chapters...")
    texts = [doc["content"] for doc in documents]
    document_embeddings = create_embeddings(texts)
    print(f"✓ Created embeddings for {len(documents)} chapters")
    
    # Find chapters about character development
    print("\nFinding chapters about character development...")
    query = "character development and growth"
    similar = find_similar_documents(query, documents, document_embeddings, top_k=2)
    print(f"Query: '{query}'")
    for doc, similarity in similar:
        print(f"  - {doc['title']}: {similarity:.3f} similarity")

# ============================================================================
# EXAMPLE 5: COMPLETE RAG WORKFLOW
# ============================================================================
def example_complete_rag_workflow():
    """Example: Complete RAG workflow demonstration"""
    print_section("Example 5: Complete RAG Workflow")
    
    print("""
Complete RAG Workflow Steps:

1. Prepare Documents
   - Collect documents you want to search
   - Chunk large documents appropriately (typically 500-1000 tokens)

2. Create Embeddings
   - Generate embeddings for all documents
   - Store embeddings with document metadata

3. Store in Vector Database (Production)
   - Use vector database like Pinecone, Weaviate, or Qdrant
   - Index embeddings for fast similarity search

4. Query Process
   - Create embedding for user query
   - Find similar documents using cosine similarity
   - Retrieve top K most similar documents

5. Generate Answer
   - Use GPT with retrieved documents as context
   - Generate answer based on relevant context
   - Cite sources if needed

Best Practices:
- Chunk documents appropriately (not too small, not too large)
- Store embeddings for reuse (they don't change)
- Use appropriate top_k values (3-5 typically works well)
- Include metadata for filtering (dates, categories, etc.)
- Monitor similarity scores (very low scores may indicate no relevant docs)
""")

# ============================================================================
# MAIN
# ============================================================================
def main():
    """Run all RAG and embedding examples"""
    print("="*70)
    print("RAG and Vector Embeddings Examples")
    print("="*70)
    print(f"OpenAI API Key: {OPENAI_API_KEY[:20]}...")
    print("="*70)
    
    try:
        # Example 1: Create embeddings
        example_create_embeddings()
        
        # Example 2: Requirements RAG
        example_requirements_rag()
        
        # Example 3: Pharmacy RAG
        example_pharmacy_rag()
        
        # Example 4: Publishing RAG
        example_publishing_rag()
        
        # Example 5: Complete workflow
        example_complete_rag_workflow()
        
        print("\n" + "="*70)
        print("All examples completed successfully!")
        print("="*70)
        
    except requests.exceptions.RequestException as e:
        print(f"\nError: {e}")
        if hasattr(e, 'response') and e.response is not None:
            print(f"Response: {e.response.text}")
    except Exception as e:
        print(f"\nUnexpected error: {e}")

if __name__ == "__main__":
    main()
