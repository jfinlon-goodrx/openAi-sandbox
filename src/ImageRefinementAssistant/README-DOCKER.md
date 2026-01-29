# Docker Setup for Image Refinement Assistant

This guide explains how to run the Image Refinement Assistant in Docker containers with debugging support.

## Prerequisites

1. **Docker Desktop** (or Docker Engine + Docker Compose)
2. **.env file** in the project root with your OpenAI API key:
   ```env
   OPENAI_API_KEY=sk-your-api-key-here
   ```

## Quick Start

### 1. Load Environment Variables

From the project root, load your `.env` file:

```bash
source scripts/setup-env.sh
```

### 2. Start Services

Start both API and Web UI:

```bash
docker-compose up -d image-refinement-api image-refinement-web
```

Or start all services:

```bash
docker-compose up -d
```

### 3. Access the Applications

- **Web UI**: http://localhost:5000
- **API**: http://localhost:5001
- **API Health Check**: http://localhost:5001/api/imagerefinement/health
- **Swagger UI**: http://localhost:5001/swagger

### 4. View Logs

```bash
# All services
docker-compose logs -f

# Specific service
docker-compose logs -f image-refinement-api
docker-compose logs -f image-refinement-web
```

### 5. Stop Services

```bash
docker-compose down
```

## Debugging

### VS Code Debugging

1. **Start containers**:
   ```bash
   docker-compose up -d image-refinement-api image-refinement-web
   ```

2. **Install VSDBG** (if not already installed):
   ```bash
   docker exec -it openai-sandbox-image-refinement-api-1 bash -c "curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg"
   ```

3. **Attach debugger**:
   - Open VS Code
   - Go to Run and Debug (F5)
   - Select "Docker: Attach to Image Refinement API" or "Docker: Attach to Image Refinement Web"
   - Set breakpoints in your code
   - The debugger will attach to the running container

### Manual Debugging

You can also attach manually:

```bash
# Get process ID
docker exec -it openai-sandbox-image-refinement-api-1 ps aux | grep dotnet

# Attach debugger (requires vsdbg installed in container)
docker exec -it openai-sandbox-image-refinement-api-1 /vsdbg/vsdbg --interpreter=vscode
```

### Hot Reload

The containers use `dotnet watch` for hot reload. Changes to your code will automatically restart the application:

```bash
# Watch logs to see reloads
docker-compose logs -f image-refinement-api
```

## Configuration

### Environment Variables

The containers automatically load from your `.env` file via `docker-compose.yml`:

- `OpenAI__ApiKey` - Your OpenAI API key
- `ASPNETCORE_ENVIRONMENT=Development` - Development mode
- `ApiBaseUrl` - Web UI connects to API at `http://image-refinement-api:8080`

### Storage Path

Exported images are saved to a Docker volume: `image-refinement-api-data`

To access exported files:

```bash
# Find the volume
docker volume ls | grep image-refinement-api-data

# Inspect the volume
docker volume inspect image-refinement-api-data

# Access files (if needed)
docker run --rm -v image-refinement-api-data:/data -it alpine ls -la /data
```

Or mount a local directory by modifying `docker-compose.yml`:

```yaml
volumes:
  - ./exports:/app/data  # Local directory instead of volume
```

## Troubleshooting

### Port Already in Use

If ports 5000 or 5001 are already in use:

```bash
# Change ports in docker-compose.yml
ports:
  - "5002:8080"  # Change 5000 to 5002
```

### Container Won't Start

1. **Check logs**:
   ```bash
   docker-compose logs image-refinement-api
   ```

2. **Verify environment variables**:
   ```bash
   docker-compose exec image-refinement-api env | grep OpenAI
   ```

3. **Rebuild containers**:
   ```bash
   docker-compose build --no-cache image-refinement-api
   docker-compose up -d image-refinement-api
   ```

### API Key Not Found

Ensure your `.env` file is in the project root and contains:

```env
OPENAI_API_KEY=sk-your-key-here
```

Then load it before running docker-compose:

```bash
source scripts/setup-env.sh
docker-compose up -d
```

### Network Issues Between Containers

The Web UI connects to the API using the service name `image-refinement-api`. If you see connection errors:

1. Verify both containers are on the same network:
   ```bash
   docker network inspect openai-sandbox_image-refinement-network
   ```

2. Check API is accessible from Web container:
   ```bash
   docker-compose exec image-refinement-web curl http://image-refinement-api:8080/api/imagerefinement/health
   ```

## Production Build

For production, use the runtime stage:

```bash
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

Or build manually:

```bash
docker build -f src/ImageRefinementAssistant/ImageRefinementAssistant.Api/Dockerfile --target runtime -t image-refinement-api:prod .
docker build -f src/ImageRefinementAssistant/ImageRefinementAssistant.Web/Dockerfile --target runtime -t image-refinement-web:prod .
```

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet-aspnet)
- [VS Code Docker Extension](https://marketplace.visualstudio.com/items?itemName=ms-azuretools.vscode-docker)
