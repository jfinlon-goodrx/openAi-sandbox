#!/usr/bin/env python3
"""
OpenAI Platform Learning Portfolio - Python API Examples
Updated with new features: Streaming, Authentication, Metrics, Database, Autonomous Agent
"""

import os
import json
import requests
from typing import Optional, Dict, Any

# Configuration
BASE_URL = os.getenv("BASE_URL", "http://localhost:5001")
API_KEY = os.getenv("API_KEY", "your-api-key-here")
JWT_TOKEN = os.getenv("JWT_TOKEN", "")

def make_request(method: str, endpoint: str, data: Optional[Dict] = None, 
                 headers: Optional[Dict] = None, stream: bool = False) -> Any:
    """Helper function to make API requests"""
    url = f"{BASE_URL}{endpoint}"
    default_headers = {
        "Content-Type": "application/json",
        "X-API-Key": API_KEY
    }
    if headers:
        default_headers.update(headers)
    
    if JWT_TOKEN:
        default_headers["Authorization"] = f"Bearer {JWT_TOKEN}"
    
    if method == "GET":
        response = requests.get(url, headers=default_headers, stream=stream)
    elif method == "POST":
        response = requests.post(url, json=data, headers=default_headers, stream=stream)
    elif method == "PUT":
        response = requests.put(url, json=data, headers=default_headers)
    elif method == "DELETE":
        response = requests.delete(url, headers=default_headers)
    else:
        raise ValueError(f"Unsupported method: {method}")
    
    response.raise_for_status()
    
    if stream:
        return response
    return response.json()

def print_response(title: str, response: Any):
    """Pretty print response"""
    print(f"\n{'='*60}")
    print(f"{title}")
    print(f"{'='*60}")
    print(json.dumps(response, indent=2))

# ============================================================================
# HEALTH CHECKS
# ============================================================================
def test_health_check():
    """Test health check endpoint"""
    response = make_request("GET", "/health")
    print_response("Health Check", response)

# ============================================================================
# AUTHENTICATION
# ============================================================================
def login():
    """Login and get JWT token"""
    data = {
        "email": "user@example.com",
        "password": "password123"
    }
    response = make_request("POST", "/api/auth/login", data=data)
    global JWT_TOKEN
    JWT_TOKEN = response.get("token", "")
    print_response("Login (JWT Token)", response)
    return JWT_TOKEN

def get_current_user():
    """Get current user info using JWT"""
    if not JWT_TOKEN:
        print("Please login first")
        return
    response = make_request("GET", "/api/auth/me")
    print_response("Current User", response)

# ============================================================================
# REQUIREMENTS ASSISTANT
# ============================================================================
def summarize_requirements():
    """Summarize requirements document"""
    data = {
        "content": "The system shall allow users to login and manage their profiles. "
                   "Users can update their email addresses and passwords."
    }
    response = make_request("POST", "/api/requirements/summarize", data=data)
    print_response("Summarize Requirements", response)

def generate_user_stories():
    """Generate user stories from requirements"""
    data = {
        "content": "Users need to login and view their dashboard with personalized content."
    }
    response = make_request("POST", "/api/requirements/generate-user-stories", data=data)
    print_response("Generate User Stories", response)

def answer_question():
    """Answer question using RAG"""
    data = {
        "question": "What are the main features?",
        "context": "The system includes user authentication, profile management, and dashboard features."
    }
    response = make_request("POST", "/api/requirements/answer-question", data=data)
    print_response("Answer Question (RAG)", response)

# ============================================================================
# STREAMING ENDPOINTS
# ============================================================================
def stream_summarization():
    """Stream requirements summarization"""
    data = {
        "content": "The system shall allow users to login and manage their profiles."
    }
    url = f"{BASE_URL}/api/streaming/summarize-stream"
    headers = {
        "Content-Type": "application/json",
        "X-API-Key": API_KEY,
        "Accept": "text/event-stream"
    }
    
    print("\n" + "="*60)
    print("Streaming Summarization (Server-Sent Events)")
    print("="*60)
    
    response = requests.post(url, json=data, headers=headers, stream=True)
    response.raise_for_status()
    
    for line in response.iter_lines():
        if line:
            decoded = line.decode('utf-8')
            if decoded.startswith('data: '):
                data_str = decoded[6:]  # Remove 'data: ' prefix
                if data_str == '[DONE]':
                    break
                try:
                    chunk = json.loads(data_str)
                    print(chunk.get('content', ''), end='', flush=True)
                except json.JSONDecodeError:
                    pass
    print("\n")

# ============================================================================
# METRICS
# ============================================================================
def get_all_metrics():
    """Get all metrics"""
    response = make_request("GET", "/api/metrics")
    print_response("All Metrics", response)

def get_model_metrics(model: str = "gpt-4-turbo-preview"):
    """Get metrics for specific model"""
    response = make_request("GET", f"/api/metrics/{model}")
    print_response(f"Metrics for {model}", response)

def reset_metrics():
    """Reset all metrics"""
    response = make_request("POST", "/api/metrics/reset")
    print_response("Reset Metrics", response)

# ============================================================================
# DATABASE ENDPOINTS
# ============================================================================
def get_user_stories():
    """Get all user stories from database"""
    response = make_request("GET", "/api/database/user-stories")
    print_response("User Stories", response)

def get_requirement_documents():
    """Get requirement documents"""
    response = make_request("GET", "/api/database/requirement-documents")
    print_response("Requirement Documents", response)

# ============================================================================
# AUTONOMOUS DEVELOPMENT AGENT
# ============================================================================
def analyze_code():
    """Analyze code for improvements"""
    data = {
        "code": """public class UserService {
    public User GetUser(int id) {
        return _users.First(u => u.Id == id);
    }
}""",
        "context": "User service for authentication"
    }
    response = make_request("POST", "/api/autonomousagent/analyze", data=data)
    print_response("Code Analysis", response)

def execute_autonomous_workflow():
    """Execute full autonomous workflow"""
    data = {
        "code": """public class UserService {
    public User GetUser(int id) {
        return _users.First(u => u.Id == id);
    }
}""",
        "context": "User service for authentication",
        "repository": "org/repo",
        "filePath": "src/Services/UserService.cs"
    }
    response = make_request("POST", "/api/autonomousagent/workflow", data=data)
    print_response("Autonomous Workflow", response)

# ============================================================================
# RETROSPECTIVE ANALYZER
# ============================================================================
def analyze_retrospective():
    """Analyze retrospective comments"""
    data = {
        "comments": [
            "We need better documentation",
            "Deployments are taking too long",
            "Great collaboration this sprint"
        ]
    }
    response = make_request("POST", "/api/retro/analyze", data=data)
    print_response("Retrospective Analysis", response)

# ============================================================================
# PHARMACY ASSISTANT
# ============================================================================
def generate_patient_education():
    """Generate patient education materials"""
    data = {
        "medication": "Metformin",
        "dosage": "500mg twice daily",
        "patientAge": 45
    }
    response = make_request("POST", "/api/pharmacy/patient-education", data=data)
    print_response("Patient Education", response)

# ============================================================================
# PUBLISHING ASSISTANT
# ============================================================================
def generate_book_review():
    """Generate book review"""
    data = {
        "bookTitle": "The Future of AI",
        "bookContent": "This book explores the potential of artificial intelligence..."
    }
    response = make_request("POST", "/api/publishing/review", data=data)
    print_response("Book Review", response)

def review_pdf_manuscript():
    """Upload PDF for senior agent review"""
    print_section("PDF Manuscript Review")
    print("Note: This requires a PDF file. Replace 'manuscript.pdf' with your actual file path.")
    
    try:
        with open("manuscript.pdf", "rb") as pdf_file:
            files = {"pdfFile": ("manuscript.pdf", pdf_file, "application/pdf")}
            params = {"genre": "Science Fiction"}
            
            url = f"{BASE_URL}/api/publishing/review-pdf"
            headers = {"X-API-Key": API_KEY}
            
            response = requests.post(
                url,
                files=files,
                params=params,
                headers=headers
            )
            response.raise_for_status()
            
            result = response.json()
            print_response("Senior Agent Review", result)
            print(f"\nDocument ID: {result.get('documentId')}")
            print(f"Chunks: {result.get('chunkCount')}")
            print(f"Estimated Tokens: {result.get('estimatedTokensUsed')}")
    except FileNotFoundError:
        print("Error: manuscript.pdf not found. Please provide a PDF file.")
    except requests.exceptions.RequestException as e:
        print(f"Error: {e}")
        if hasattr(e, 'response') and e.response is not None:
            print(f"Response: {e.response.text}")

# ============================================================================
# ADVERTISING ASSISTANT
# ============================================================================
def generate_ad_copy():
    """Generate ad copy"""
    data = {
        "product": "Smart Watch",
        "targetAudience": "Tech enthusiasts aged 25-40",
        "tone": "Modern and exciting"
    }
    response = make_request("POST", "/api/advertising/ad-copy", data=data)
    print_response("Ad Copy", response)

# ============================================================================
# TESTING FEATURES
# ============================================================================
def test_rate_limiting():
    """Test rate limiting by making multiple requests"""
    print("\n" + "="*60)
    print("Testing Rate Limiting")
    print("="*60)
    for i in range(5):
        try:
            response = requests.get(
                f"{BASE_URL}/api/metrics",
                headers={"X-API-Key": API_KEY}
            )
            rate_limit = response.headers.get("X-RateLimit-Limit", "N/A")
            remaining = response.headers.get("X-RateLimit-Remaining", "N/A")
            print(f"Request {i+1}: Status {response.status_code}, "
                  f"Remaining: {remaining}/{rate_limit}")
        except requests.exceptions.RequestException as e:
            print(f"Request {i+1}: Error - {e}")

def test_correlation_id():
    """Test correlation ID tracking"""
    import uuid
    correlation_id = str(uuid.uuid4())
    headers = {
        "X-Correlation-ID": correlation_id
    }
    response = make_request("GET", "/api/metrics", headers=headers)
    print(f"\nCorrelation ID sent: {correlation_id}")
    print("Check logs for correlation ID tracking")

# ============================================================================
# MAIN
# ============================================================================
def main():
    """Run all examples"""
    print("="*60)
    print("OpenAI Platform Learning Portfolio - Python Examples")
    print("="*60)
    print(f"Base URL: {BASE_URL}")
    print(f"API Key: {API_KEY[:20]}...")
    print("="*60)
    
    try:
        # Health checks
        test_health_check()
        
        # Authentication
        login()
        get_current_user()
        
        # Requirements Assistant
        summarize_requirements()
        generate_user_stories()
        answer_question()
        
        # Streaming
        stream_summarization()
        
        # Metrics
        get_all_metrics()
        get_model_metrics()
        
        # Database
        get_user_stories()
        get_requirement_documents()
        
        # Autonomous Agent
        analyze_code()
        
        # Retrospective
        analyze_retrospective()
        
        # Industry examples
        generate_patient_education()
        generate_book_review()
        review_pdf_manuscript()  # PDF review example
        generate_ad_copy()
        
        # Testing features
        test_rate_limiting()
        test_correlation_id()
        
        print("\n" + "="*60)
        print("All examples completed successfully!")
        print("="*60)
        
    except requests.exceptions.RequestException as e:
        print(f"\nError: {e}")
        if hasattr(e, 'response') and e.response is not None:
            print(f"Response: {e.response.text}")

if __name__ == "__main__":
    main()
