#!/usr/bin/env python3
"""
Python examples for OpenAI Platform Learning Portfolio
These examples can be run without a development environment - just install requests: pip install requests
"""

import requests
import json
import os

# Configuration
OPENAI_API_KEY = os.getenv("OPENAI_API_KEY", "YOUR_OPENAI_API_KEY")
OPENAI_BASE_URL = "https://api.openai.com/v1"
PROJECT_API_BASE_URL = "http://localhost:5000"  # Change if your API runs on different port

def print_section(title):
    """Print a formatted section header"""
    print(f"\n{'='*60}")
    print(f"  {title}")
    print(f"{'='*60}\n")

def example_chat_completion():
    """Example: Chat Completion - Summarize Requirements"""
    print_section("Example 1: Chat Completion - Summarize Requirements")
    
    response = requests.post(
        f"{OPENAI_BASE_URL}/chat/completions",
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {OPENAI_API_KEY}"
        },
        json={
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
        }
    )
    
    if response.status_code == 200:
        result = response.json()
        print(json.dumps(result, indent=2))
    else:
        print(f"Error: {response.status_code}")
        print(response.text)

def example_moderation():
    """Example: Content Moderation"""
    print_section("Example 2: Content Moderation")
    
    response = requests.post(
        f"{OPENAI_BASE_URL}/moderations",
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {OPENAI_API_KEY}"
        },
        json={
            "input": "This is a test message to check for inappropriate content."
        }
    )
    
    if response.status_code == 200:
        result = response.json()
        flagged = result["results"][0]["flagged"]
        print(f"Content flagged: {flagged}")
        print(json.dumps(result, indent=2))
    else:
        print(f"Error: {response.status_code}")
        print(response.text)

def example_embeddings():
    """Example: Generate Embeddings"""
    print_section("Example 3: Generate Embeddings")
    
    response = requests.post(
        f"{OPENAI_BASE_URL}/embeddings",
        headers={
            "Content-Type": "application/json",
            "Authorization": f"Bearer {OPENAI_API_KEY}"
        },
        json={
            "model": "text-embedding-ada-002",
            "input": "Patient education: Metformin is used to treat type 2 diabetes by helping control blood sugar levels."
        }
    )
    
    if response.status_code == 200:
        result = response.json()
        embedding_length = len(result["data"][0]["embedding"])
        print(f"Embedding vector length: {embedding_length}")
        print(f"First 10 values: {result['data'][0]['embedding'][:10]}")
    else:
        print(f"Error: {response.status_code}")
        print(response.text)

def example_publishing_api():
    """Example: Publishing Assistant API"""
    print_section("Example 4: Publishing Assistant - Generate Summary")
    
    response = requests.post(
        f"{PROJECT_API_BASE_URL}/api/publishing/summary",
        headers={"Content-Type": "application/json"},
        json={
            "content": "Chapter 1: The story begins in a small town where nothing ever happens. Our hero, a young detective, arrives to investigate a mysterious disappearance...",
            "maxLength": 250
        }
    )
    
    if response.status_code == 200:
        result = response.json()
        print(json.dumps(result, indent=2))
    else:
        print(f"Error: {response.status_code}")
        print(response.text)
        print("\nNote: Make sure the Publishing Assistant API is running!")

def example_pharmacy_api():
    """Example: Pharmacy Assistant API"""
    print_section("Example 5: Pharmacy Assistant - Patient Education")
    
    response = requests.post(
        f"{PROJECT_API_BASE_URL}/api/pharmacy/patient-education",
        headers={"Content-Type": "application/json"},
        json={
            "medicationName": "Metformin",
            "condition": "Type 2 Diabetes"
        }
    )
    
    if response.status_code == 200:
        result = response.json()
        print(json.dumps(result, indent=2))
    else:
        print(f"Error: {response.status_code}")
        print(response.text)
        print("\nNote: Make sure the Pharmacy Assistant API is running!")

if __name__ == "__main__":
    print("OpenAI Platform Learning Portfolio - Python Examples")
    print("=" * 60)
    print("\nMake sure to set OPENAI_API_KEY environment variable")
    print("or update OPENAI_API_KEY in this script.\n")
    
    # Run examples
    try:
        example_chat_completion()
        example_moderation()
        example_embeddings()
        
        # Project API examples (require running services)
        print("\n" + "="*60)
        print("  Project API Examples (require running services)")
        print("="*60)
        example_publishing_api()
        example_pharmacy_api()
        
    except requests.exceptions.RequestException as e:
        print(f"\nError making request: {e}")
        print("Make sure you have an internet connection and valid API key.")
    except Exception as e:
        print(f"\nUnexpected error: {e}")
