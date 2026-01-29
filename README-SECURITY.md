# Security Guide: API Key Management

## Overview

This project uses **secure practices** to manage OpenAI API keys. API keys are **never stored in committed files**. Instead, we use environment variables loaded from a `.env` file.

## Quick Start

1. **Create `.env` file** in the project root:
   ```bash
   OPENAI_API_KEY=sk-your-actual-api-key-here
   ```

2. **Load environment variables** before running applications:

   **macOS/Linux:**
   ```bash
   source scripts/setup-env.sh
   ```

   **Windows PowerShell:**
   ```powershell
   .\scripts\setup-env.ps1
   ```

   **Windows Command Prompt:**
   ```cmd
   scripts\setup-env.bat
   ```

3. **Run your application** - it will automatically use the environment variable.

## How It Works

### 1. `.env` File (Not Committed)
- Contains your actual API key
- Listed in `.gitignore` - **never committed to Git**
- Loaded by setup scripts into environment variables

### 2. `appsettings.json` (Committed, But Clean)
- Contains **empty** API key: `"ApiKey": ""`
- Safe to commit because it has no sensitive data
- .NET automatically reads from environment variables when `appsettings.json` has empty values

### 3. Environment Variables
- `.NET` reads configuration in this order:
  1. Environment variables (highest priority)
  2. `appsettings.json` (fallback)
- Format: `OpenAI__ApiKey` (double underscore for nested config)

## Security Best Practices

### ✅ DO:
- ✅ Keep `.env` file in `.gitignore`
- ✅ Keep `appsettings.json` with empty API keys in repo
- ✅ Use environment variables for actual keys
- ✅ Use setup scripts to load from `.env`
- ✅ Use User Secrets for development (alternative)
- ✅ Use Azure Key Vault for production deployments

### ❌ DON'T:
- ❌ Commit `.env` file to Git
- ❌ Put real API keys in `appsettings.json`
- ❌ Share API keys in chat, email, or documentation
- ❌ Hardcode API keys in source code
- ❌ Commit `appsettings.Development.json` with real keys

## Alternative Methods

### Option 1: User Secrets (Development)
```bash
cd src/YourProject/YourProject.Api
dotnet user-secrets init
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-key-here"
```

### Option 2: Direct Environment Variable
```bash
# macOS/Linux
export OpenAI__ApiKey="sk-your-key-here"

# Windows PowerShell
$env:OpenAI__ApiKey = "sk-your-key-here"

# Windows CMD
set OpenAI__ApiKey=sk-your-key-here
```

### Option 3: Azure Key Vault (Production)
```csharp
builder.Configuration.AddAzureKeyVault(
    keyVaultUrl,
    new DefaultAzureCredential()
);
```

## Verification

Check if your API key is loaded:
```bash
# macOS/Linux
echo $OpenAI__ApiKey

# Windows PowerShell
echo $env:OpenAI__ApiKey

# Windows CMD
echo %OpenAI__ApiKey%
```

## Troubleshooting

### "API key not found" error
1. Verify `.env` file exists and contains `OPENAI_API_KEY=...`
2. Run the setup script: `source scripts/setup-env.sh`
3. Verify environment variable: `echo $OpenAI__ApiKey`

### Key not persisting between sessions
- Add to your shell profile (`~/.zshrc` or `~/.bashrc`):
  ```bash
  export OpenAI__ApiKey="$OPENAI_API_KEY"
  ```
- Or always run the setup script before starting applications

## File Structure

```
project-root/
├── .env                          # ← Your API key (NOT committed)
├── .gitignore                    # ← Ensures .env is ignored
├── appsettings.json              # ← Empty API key (committed, safe)
├── appsettings.json.example      # ← Template (committed)
├── scripts/
│   ├── setup-env.sh              # ← macOS/Linux script
│   ├── setup-env.ps1             # ← Windows PowerShell script
│   └── setup-env.bat             # ← Windows CMD script
└── src/
    └── YourProject/
        └── appsettings.json      # ← Empty API key (committed, safe)
```

## Why This Approach?

1. **Security**: API keys never in version control
2. **Convenience**: Easy to load from `.env` file
3. **Flexibility**: Works with all .NET configuration methods
4. **Team-friendly**: Each developer has their own `.env` file
5. **CI/CD Ready**: Can use GitHub Secrets or Azure Key Vault

## Questions?

See [Security Best Practices](docs/best-practices/security.md) for more details.
