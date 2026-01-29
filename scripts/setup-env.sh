#!/bin/bash
# Setup script to load OpenAI API key from .env file into environment variables
# This allows appsettings.json to remain clean and secure

set -e

# Colors for output
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${GREEN}🔐 OpenAI API Key Setup Script${NC}"
echo ""

# Check if .env file exists
if [ ! -f ".env" ]; then
    echo -e "${RED}❌ Error: .env file not found!${NC}"
    echo ""
    echo "Please create a .env file in the project root with:"
    echo "  OPENAI_API_KEY=sk-your-api-key-here"
    echo ""
    exit 1
fi

# Load .env file
echo -e "${YELLOW}📖 Loading .env file...${NC}"
while IFS= read -r line || [ -n "$line" ]; do
    # Skip comments and empty lines
    case "$line" in
        \#*|'') continue ;;
    esac
    
    # Extract key and value using parameter expansion
    key="${line%%=*}"
    value="${line#*=}"
    
    # Trim whitespace from key
    key=$(echo "$key" | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')
    
    # Remove quotes if present from value and trim whitespace
    value=$(echo "$value" | sed "s/^[\"']//;s/[\"']$//;s/^[[:space:]]*//;s/[[:space:]]*$//")
    
    # Export the variable
    if [ -n "$key" ] && [ -n "$value" ]; then
        export "${key}=${value}"
    fi
done < .env

# Check if API key is set
if [ -z "$OPENAI_API_KEY" ]; then
    echo -e "${RED}❌ Error: OPENAI_API_KEY not found in .env file!${NC}"
    echo ""
    echo "Please add to .env:"
    echo "  OPENAI_API_KEY=sk-your-api-key-here"
    echo ""
    exit 1
fi

# Set environment variable for .NET (double underscore format)
export OpenAI__ApiKey="$OPENAI_API_KEY"

echo -e "${GREEN}✅ API key loaded successfully!${NC}"
echo ""
echo "Environment variable set: OpenAI__ApiKey"
echo ""

# Check if running in interactive shell
if [ -n "$PS1" ]; then
    echo -e "${YELLOW}💡 To make this permanent, add to your shell profile (~/.zshrc or ~/.bashrc):${NC}"
    echo ""
    echo "  export OpenAI__ApiKey=\"\$OPENAI_API_KEY\""
    echo ""
    echo "Or run this script before starting your application:"
    echo "  source scripts/setup-env.sh"
    echo ""
fi

# Verify the key is set
if [ -n "$OpenAI__ApiKey" ]; then
    KEY_PREVIEW="${OpenAI__ApiKey:0:20}..."
    echo -e "${GREEN}✓ Key preview: ${KEY_PREVIEW}${NC}"
    echo ""
    echo "You can now run your .NET applications and they will use this API key."
    echo "The appsettings.json files remain clean and secure."
else
    echo -e "${RED}❌ Failed to set environment variable${NC}"
    exit 1
fi
