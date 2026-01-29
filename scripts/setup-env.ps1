# PowerShell script to load OpenAI API key from .env file into environment variables
# This allows appsettings.json to remain clean and secure

Write-Host "🔐 OpenAI API Key Setup Script" -ForegroundColor Green
Write-Host ""

# Check if .env file exists
if (-not (Test-Path ".env")) {
    Write-Host "❌ Error: .env file not found!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please create a .env file in the project root with:"
    Write-Host "  OPENAI_API_KEY=sk-your-api-key-here"
    Write-Host ""
    exit 1
}

# Load .env file
Write-Host "📖 Loading .env file..." -ForegroundColor Yellow

$envContent = Get-Content ".env" | Where-Object { $_ -notmatch '^\s*#' -and $_ -match '=' }
foreach ($line in $envContent) {
    $parts = $line -split '=', 2
    if ($parts.Length -eq 2) {
        $key = $parts[0].Trim()
        $value = $parts[1].Trim()
        [Environment]::SetEnvironmentVariable($key, $value, "Process")
    }
}

# Check if API key is set
if (-not $env:OPENAI_API_KEY) {
    Write-Host "❌ Error: OPENAI_API_KEY not found in .env file!" -ForegroundColor Red
    Write-Host ""
    Write-Host "Please add to .env:"
    Write-Host "  OPENAI_API_KEY=sk-your-api-key-here"
    Write-Host ""
    exit 1
}

# Set environment variable for .NET (double underscore format)
$env:OpenAI__ApiKey = $env:OPENAI_API_KEY

Write-Host "✅ API key loaded successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Environment variable set: OpenAI__ApiKey"
Write-Host ""

# Verify the key is set
if ($env:OpenAI__ApiKey) {
    $keyPreview = $env:OpenAI__ApiKey.Substring(0, [Math]::Min(20, $env:OpenAI__ApiKey.Length)) + "..."
    Write-Host "✓ Key preview: $keyPreview" -ForegroundColor Green
    Write-Host ""
    Write-Host "You can now run your .NET applications and they will use this API key."
    Write-Host "The appsettings.json files remain clean and secure."
} else {
    Write-Host "❌ Failed to set environment variable" -ForegroundColor Red
    exit 1
}
