@echo off
REM Windows batch script to load OpenAI API key from .env file into environment variables
REM This allows appsettings.json to remain clean and secure

echo 🔐 OpenAI API Key Setup Script
echo.

REM Check if .env file exists
if not exist ".env" (
    echo ❌ Error: .env file not found!
    echo.
    echo Please create a .env file in the project root with:
    echo   OPENAI_API_KEY=sk-your-api-key-here
    echo.
    exit /b 1
)

echo 📖 Loading .env file...

REM Read .env file and set environment variables
for /f "usebackq tokens=1,* delims==" %%a in (".env") do (
    if not "%%a"=="" (
        if not "%%a"=="#" (
            set "%%a=%%b"
        )
    )
)

REM Check if API key is set
if "%OPENAI_API_KEY%"=="" (
    echo ❌ Error: OPENAI_API_KEY not found in .env file!
    echo.
    echo Please add to .env:
    echo   OPENAI_API_KEY=sk-your-api-key-here
    echo.
    exit /b 1
)

REM Set environment variable for .NET (double underscore format)
set OpenAI__ApiKey=%OPENAI_API_KEY%

echo ✅ API key loaded successfully!
echo.
echo Environment variable set: OpenAI__ApiKey
echo.

REM Verify the key is set
if not "%OpenAI__ApiKey%"=="" (
    set KEY_PREVIEW=%OpenAI__ApiKey:~0,20%...
    echo ✓ Key preview: %KEY_PREVIEW%
    echo.
    echo You can now run your .NET applications and they will use this API key.
    echo The appsettings.json files remain clean and secure.
) else (
    echo ❌ Failed to set environment variable
    exit /b 1
)
