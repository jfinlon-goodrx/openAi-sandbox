# Quick Start for Non-Developers

**For:** Technical professionals who want to use OpenAI Platform capabilities without writing code or setting up a development environment.

This guide shows you the simplest way to get started and run examples using REST APIs. No coding knowledge or development tools required!

## What You'll Need

- **A computer** (Windows, Mac, or Linux)
- **An OpenAI API key** (get one at [platform.openai.com](https://platform.openai.com))
- **A terminal/command prompt** (built into your computer)
- **Optional:** Python (for Python examples) or Postman (for GUI-based testing)

**That's it!** No .NET SDK, no Visual Studio, no complex setup.

## Step 1: Get Your OpenAI API Key

### For GoodRx Employees

**⚠️ IMPORTANT:** If you're a GoodRx employee, follow this process to avoid billing issues:

1. **Submit a ServiceNow request** to get OpenAI Platform access
2. **Wait for the tile to be added to your Okta panel** - Do NOT create an OpenAI account before this step
3. **Access OpenAI through Okta** once the tile is available
4. **Navigate to API Keys** in the OpenAI Platform dashboard
5. **Create your API key** from within the organization's account

**Why this matters:** Creating an OpenAI account before the Okta tile is assigned can confuse the billing entity assignment and may require a re-invite process.

**Reference:** See [AI Approved Systems - OpenAI Request](https://goodrx-dev.atlassian.net/wiki/spaces/GRXHUB/pages/5507252322/AI+Approved+Systems) in Confluence for complete instructions.

### For Other Organizations / Personal Use

1. Go to [platform.openai.com](https://platform.openai.com)
2. Sign up or log in
3. Navigate to [API Keys](https://platform.openai.com/api-keys)
4. Click "Create new secret key"
5. Give it a name (e.g., "Learning Portfolio")
6. **Copy the key immediately** - you'll only see it once!
   - It starts with `sk-` followed by a long string of characters
   - Save it somewhere secure (password manager, secure note, etc.)

**⚠️ Important:** Never share your API key publicly. Treat it like a password.

## Step 2: Choose Your Method

You have three options, from simplest to most advanced:

### Option 1: curl (Simplest - Built into Mac/Linux, Available for Windows)

**What is curl?** A command-line tool for making web requests. Already installed on Mac and Linux. For Windows, see setup below.

**Best for:** Quick testing, learning how APIs work, simple automation.

**Windows Setup:**
- Windows 10/11: curl is built-in! Just open Command Prompt or PowerShell
- Older Windows: Download from [curl.se](https://curl.se/download.html) or use PowerShell

**Mac/Linux Setup:**
- Already installed! Just open Terminal.

### Option 2: Python (Easy - Simple Scripts)

**What is Python?** A programming language that's easy to learn. Great for automating tasks.

**Best for:** Running examples, simple automation, learning programming basics.

**Setup:**
1. Check if Python is installed:
   ```bash
   python --version
   # or
   python3 --version
   ```
2. If not installed:
   - **Mac:** Install from [python.org](https://www.python.org/downloads/) or use Homebrew: `brew install python3`
   - **Windows:** Download from [python.org](https://www.python.org/downloads/)
   - **Linux:** Usually pre-installed, or use: `sudo apt install python3 python3-pip`

3. Install the `requests` library (needed for API calls):
   ```bash
   pip install requests
   # or
   pip3 install requests
   ```

### Option 3: Postman (Easiest GUI - Visual Interface)

**What is Postman?** A graphical tool for testing APIs. No command line needed!

**Best for:** Visual learners, testing APIs without typing commands, sharing API collections with teams.

**Setup:**
1. Download Postman from [postman.com/downloads](https://www.postman.com/downloads/)
2. Install and open Postman
3. Create a free account (optional but recommended)

## Step 3: Set Up Your API Key

### For curl (Mac/Linux/Windows)

**Option A: Environment Variable (Recommended)**

**Mac/Linux:**
```bash
export OPENAI_API_KEY="sk-your-actual-api-key-here"
```

**Windows (Command Prompt):**
```cmd
set OPENAI_API_KEY=sk-your-actual-api-key-here
```

**Windows (PowerShell):**
```powershell
$env:OPENAI_API_KEY="sk-your-actual-api-key-here"
```

**To make it permanent:**
- **Mac/Linux:** Add to `~/.bashrc` or `~/.zshrc`:
  ```bash
  echo 'export OPENAI_API_KEY="sk-your-actual-api-key-here"' >> ~/.zshrc
  source ~/.zshrc
  ```
- **Windows:** Use System Properties → Environment Variables

**Option B: Include in Each Command**

You can include the API key directly in curl commands (less secure but simpler for testing):
```bash
curl https://api.openai.com/v1/models \
  -H "Authorization: Bearer sk-your-actual-api-key-here"
```

### For Python

Create a file called `.env` in your project folder (or use environment variables):

**Using environment variable:**
```bash
export OPENAI_API_KEY="sk-your-actual-api-key-here"
```

**Or in Python code:**
```python
import os
os.environ['OPENAI_API_KEY'] = 'sk-your-actual-api-key-here'
```

### For Postman

1. Open Postman
2. Click "Environments" → "Create Environment"
3. Name it "OpenAI"
4. Add variable:
   - Variable: `OPENAI_API_KEY`
   - Initial Value: `sk-your-actual-api-key-here`
   - Current Value: `sk-your-actual-api-key-here`
5. Click "Save"
6. Select your environment from the dropdown (top right)

## Step 4: Test Your Setup

### Test with curl

```bash
curl https://api.openai.com/v1/models \
  -H "Authorization: Bearer $OPENAI_API_KEY"
```

**Expected result:** You should see a list of available models in JSON format.

**If you get an error:**
- "401 Unauthorized" → Check your API key is correct
- "command not found" → curl not installed (see Step 2)
- "Network error" → Check your internet connection

### Test with Python

Create a file `test_api.py`:

```python
import os
import requests

api_key = os.environ.get('OPENAI_API_KEY', 'sk-your-actual-api-key-here')

headers = {
    'Authorization': f'Bearer {api_key}',
    'Content-Type': 'application/json'
}

response = requests.get('https://api.openai.com/v1/models', headers=headers)
print(response.json())
```

Run it:
```bash
python test_api.py
# or
python3 test_api.py
```

**Expected result:** You should see a list of available models printed to the console.

### Test with Postman

1. Click "New" → "HTTP Request"
2. Set method to `GET`
3. Enter URL: `https://api.openai.com/v1/models`
4. Go to "Authorization" tab
5. Select "Bearer Token"
6. Enter token: `{{OPENAI_API_KEY}}` (or your actual key)
7. Click "Send"

**Expected result:** You should see a list of available models in the response.

## Step 5: Run Your First Example

Now you're ready to try the examples! Here are quick starts for each method:

### curl Example: Simple Chat Completion

```bash
curl https://api.openai.com/v1/chat/completions \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $OPENAI_API_KEY" \
  -d '{
    "model": "gpt-3.5-turbo",
    "messages": [
      {"role": "user", "content": "Say hello in one sentence"}
    ],
    "max_tokens": 50
  }'
```

**What this does:** Sends a message to OpenAI and gets a response.

### Python Example: Simple Chat Completion

Create `hello_ai.py`:

```python
import os
import requests
import json

api_key = os.environ.get('OPENAI_API_KEY')

headers = {
    'Authorization': f'Bearer {api_key}',
    'Content-Type': 'application/json'
}

data = {
    'model': 'gpt-3.5-turbo',
    'messages': [
        {'role': 'user', 'content': 'Say hello in one sentence'}
    ],
    'max_tokens': 50
}

response = requests.post(
    'https://api.openai.com/v1/chat/completions',
    headers=headers,
    json=data
)

result = response.json()
print(result['choices'][0]['message']['content'])
```

Run it:
```bash
python hello_ai.py
```

### Postman Example: Simple Chat Completion

1. Create new POST request
2. URL: `https://api.openai.com/v1/chat/completions`
3. Authorization: Bearer Token → `{{OPENAI_API_KEY}}`
4. Body → raw → JSON:
```json
{
  "model": "gpt-3.5-turbo",
  "messages": [
    {"role": "user", "content": "Say hello in one sentence"}
  ],
  "max_tokens": 50
}
```
5. Click "Send"

## Next Steps

Now that you're set up, try the complete examples:

### Complete Example Collections

1. **[REST API Examples](../../samples/REST-API-Examples/README.md)** - Comprehensive examples for all endpoints:
   - curl examples for all API endpoints
   - Python examples with complete functions
   - Postman collection (import ready)
   - Health checks, authentication, streaming, metrics, database, and more

2. **[RAG & Embeddings Examples](../../samples/REST-API-Examples/rag-embeddings-examples.sh)** - Advanced examples:
   - Creating embeddings
   - Semantic search
   - RAG workflows

3. **[Direct OpenAI API Examples](../../samples/REST-API-Examples/openai-direct-examples.sh)** - Call OpenAI directly:
   - Model listing
   - Chat completions
   - Embeddings

### Learning Resources

- **[API Basics](02-api-basics.md)** - Understand models, tokens, and costs
- **[Prompt Engineering](03-prompt-engineering.md)** - Learn to write effective prompts
- **[Glossary](../../GLOSSARY.md)** - Definitions of technical terms
- **[Role Guides](../../role-guides/)** - Workflows for your specific role

## Common Questions

### "Do I need to install .NET or Visual Studio?"
**No!** This guide is specifically for using REST APIs without any development environment. You only need curl, Python, or Postman.

### "Can I use these examples in my work?"
**Yes!** These examples demonstrate real OpenAI Platform capabilities. You can adapt them for your own use cases.

### "What if I want to build a full application later?"
See the [Full Setup Guide](01-setup.md) for developers who want to build .NET applications.

### "How much does this cost?"
- API calls are charged per token (see [API Basics](02-api-basics.md) for details)
- GPT-3.5 Turbo: ~$0.0005 per 1K input tokens (very cheap)
- GPT-4 Turbo: ~$0.01 per 1K input tokens (more expensive but better quality)
- Check [OpenAI Pricing](https://openai.com/pricing) for current rates
- **GoodRx employees:** Billing is handled through the organization's account. Check with your manager or IT for usage policies.

### "Is my API key secure?"
- **Never commit API keys to version control** (Git repositories)
- **Use environment variables** when possible
- **Rotate keys** if you suspect they've been compromised
- **Use different keys** for development vs. production
- **GoodRx employees:** Follow your organization's security policies. Never share API keys outside of approved channels.

### "What if I get rate limit errors?"
- You're making requests too quickly
- Wait a few minutes and try again
- Consider upgrading your OpenAI plan for higher limits
- See [Troubleshooting](01-setup.md#troubleshooting) for more help

## Troubleshooting

### curl: "command not found"
- **Mac/Linux:** Should be pre-installed. Try `which curl` to find it
- **Windows:** Use PowerShell (has curl built-in) or download from [curl.se](https://curl.se/download.html)

### Python: "No module named 'requests'"
```bash
pip install requests
# or
pip3 install requests
```

### "401 Unauthorized" error
- Check your API key is correct (starts with `sk-`)
- Verify environment variable is set: `echo $OPENAI_API_KEY` (Mac/Linux) or `echo %OPENAI_API_KEY%` (Windows)
- Make sure there are no extra spaces in your API key

### "Network error" or "Connection timeout"
- Check your internet connection
- Verify `api.openai.com` is accessible
- Check firewall/proxy settings if on corporate network

### "Rate limit exceeded" (429 error)
- You're making too many requests too quickly
- Wait a few minutes and try again
- Check your usage at [OpenAI Usage Dashboard](https://platform.openai.com/usage)

## Getting Help

1. **Check the examples:**
   - [REST API Examples](../../samples/REST-API-Examples/README.md) - Complete working examples
   - [Troubleshooting Guide](01-setup.md#troubleshooting) - Common issues and solutions

2. **Review the documentation:**
   - [API Basics](02-api-basics.md) - Understanding how APIs work
   - [Glossary](../../GLOSSARY.md) - Definitions of terms

3. **Check OpenAI's documentation:**
   - [OpenAI Platform Docs](https://platform.openai.com/docs)
   - [OpenAI API Reference](https://platform.openai.com/docs/api-reference)

## What You've Learned

After completing this setup, you now have:
- ✅ An OpenAI API key configured securely
- ✅ A working tool (curl, Python, or Postman) for making API calls
- ✅ The ability to test your setup and verify it works
- ✅ Knowledge of how to run your first example

**No coding experience required!** Just follow the examples and adapt them for your needs.

## Summary

You now have everything you need to:
- ✅ Make API calls to OpenAI
- ✅ Run example scripts
- ✅ Test different models and capabilities
- ✅ Learn how OpenAI Platform works

**Ready to explore?** Start with [REST API Examples](../../samples/REST-API-Examples/README.md) →

---

**Next:** Learn about [API Basics](02-api-basics.md) to understand models, tokens, and costs.
