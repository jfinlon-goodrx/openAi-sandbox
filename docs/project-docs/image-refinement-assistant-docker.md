# Image Refinement Assistant - Docker Setup Guide

**For:** Developers who want to run the Image Refinement Assistant in Docker containers with debugging support.

## Overview

This guide covers running the Image Refinement Assistant using Docker Compose, which provides:
- **Easy setup** - No need to install .NET SDK locally
- **Hot reload** - Code changes automatically restart services
- **Debugging support** - Attach VS Code debugger to running containers
- **Isolated environment** - Consistent across different machines
- **Network isolation** - Services communicate via Docker network

## Prerequisites

- **Docker Desktop** installed and running
- **`.env` file** in project root with `OPENAI_API_KEY`
- **VS Code** (optional, for debugging)

## Quick Start

1. **Load environment variables:**
   ```bash
   source scripts/setup-env.sh
   ```

2. **Start services:**
   ```bash
   docker-compose up -d image-refinement-api image-refinement-web
   ```

3. **Access applications:**
   - **Web UI**: http://localhost:5000
   - **API**: http://localhost:5001
   - **API Swagger**: http://localhost:5001/swagger

4. **View logs:**
   ```bash
   docker-compose logs -f image-refinement-api image-refinement-web
   ```

## Docker Architecture

```
┌─────────────────────────────────────────┐
│         Docker Network                  │
│  (image-refinement-network)            │
│                                         │
│  ┌──────────────────┐                 │
│  │  Web UI          │                 │
│  │  Port: 5000     │                 │
│  │  Debug: 5003    │                 │
│  └────────┬─────────┘                 │
│           │                            │
│           │ http://image-refinement-api │
│           │                            │
│  ┌────────▼─────────┐                 │
│  │  API             │                 │
│  │  Port: 5001     │                 │
│  │  Debug: 5002    │                 │
│  └──────────────────┘                 │
│                                         │
└─────────────────────────────────────────┘
```

## Container Details

### Image Refinement API

- **Container name**: `openai-sandbox-image-refinement-api-1`
- **Port mapping**: `5001:8080` (application), `5002:5000` (debugging)
- **Command**: `dotnet watch run` (hot reload enabled)
- **Volumes**:
  - Source code mounted for hot reload
  - Logs directory for persistent logs
  - Data volume for exports

### Image Refinement Web

- **Container name**: `openai-sandbox-image-refinement-web-1`
- **Port mapping**: `5000:8080` (application), `5003:5001` (debugging)
- **Command**: `dotnet watch run` (hot reload enabled)
- **Volumes**:
  - Source code mounted for hot reload
  - Logs directory for persistent logs

## Debugging

### VS Code Debugging

1. **Start services:**
   ```bash
   docker-compose up -d image-refinement-api image-refinement-web
   ```

2. **Open VS Code** in the project root

3. **Set breakpoints** in your code

4. **Attach debugger:**
   - Press `F5` or go to Run and Debug
   - Select "Attach to Image Refinement API (Docker)" or "Attach to Image Refinement Web (Docker)"
   - Select the dotnet process when prompted

5. **Debug!** - Your breakpoints will now hit

### Manual Process Attachment

If automatic attachment doesn't work:

1. **Find the process ID:**
   ```bash
   docker exec openai-sandbox-image-refinement-api-1 ps aux | grep dotnet
   ```

2. **Attach using the process ID** in VS Code launch configuration

## Hot Reload

Both containers run with `dotnet watch`, which means:
- **Code changes** automatically trigger rebuild and restart
- **No manual restart needed** for most changes
- **Fast iteration** during development

**Note:** Some changes (like `Program.cs` or `appsettings.json`) may require container restart:
```bash
docker-compose restart image-refinement-api image-refinement-web
```

## Environment Variables

The `.env` file is automatically loaded by Docker Compose. Variables are passed to containers as:
- `OpenAI__ApiKey` - Your OpenAI API key
- `OpenAI__BaseUrl` - API base URL (defaults to https://api.openai.com/v1)
- `ASPNETCORE_ENVIRONMENT=Development` - Development mode
- `DOTNET_USE_POLLING_FILE_WATCHER=true` - Enable file watching

## Storage and Exports

Exported images and metadata are saved to:
- **Container path**: `/app/data` (mounted volume)
- **Host path**: Docker volume `image-refinement-api-data`
- **Default location**: Managed by Docker

To access exports:
```bash
# Find volume location
docker volume inspect openai-sandbox_image-refinement-api-data

# Or copy from container
docker cp openai-sandbox-image-refinement-api-1:/app/data ./exports
```

## Troubleshooting

### Services won't start

**Check logs:**
```bash
docker-compose logs image-refinement-api image-refinement-web
```

**Common issues:**
- Port conflicts: Ensure ports 5000, 5001, 5002, 5003 are available
- Missing .env file: Create `.env` with `OPENAI_API_KEY`
- Docker not running: Start Docker Desktop

### Debugging not working

**Verify VSDBG is installed:**
```bash
docker exec openai-sandbox-image-refinement-api-1 ls -la /vsdbg
```

**Check process is running:**
```bash
docker exec openai-sandbox-image-refinement-api-1 ps aux | grep dotnet
```

**Verify port mapping:**
```bash
docker-compose ps
```

### Hot reload not working

**Check file watcher:**
```bash
docker exec openai-sandbox-image-refinement-api-1 env | grep DOTNET_USE_POLLING
```

**Restart containers:**
```bash
docker-compose restart image-refinement-api image-refinement-web
```

### Network connectivity issues

**Verify containers can communicate:**
```bash
docker exec openai-sandbox-image-refinement-web-1 curl http://image-refinement-api:8080/api/imagerefinement/health
```

**Check network:**
```bash
docker network inspect openai-sandbox_image-refinement-network
```

## Useful Commands

```bash
# Start services
docker-compose up -d image-refinement-api image-refinement-web

# Stop services
docker-compose stop image-refinement-api image-refinement-web

# Restart services
docker-compose restart image-refinement-api image-refinement-web

# View logs
docker-compose logs -f image-refinement-api image-refinement-web

# Rebuild containers (after code changes)
docker-compose build image-refinement-api image-refinement-web

# Remove containers and volumes
docker-compose down -v

# Execute command in container
docker exec -it openai-sandbox-image-refinement-api-1 bash

# Check container status
docker-compose ps

# View resource usage
docker stats openai-sandbox-image-refinement-api-1 openai-sandbox-image-refinement-web-1
```

## Production Deployment

For production, use the `runtime` target instead of `development`:

```yaml
image-refinement-api:
  build:
    target: runtime  # Instead of development
```

This uses the smaller `aspnet:8.0` runtime image instead of the full SDK, reducing image size significantly.

## Related Documentation

- [Main Image Refinement Assistant Guide](image-refinement-assistant.md)
- [Docker Deployment Guide](../../deployment/docker-guide.md)
- [Security Guide](../../README-SECURITY.md)
