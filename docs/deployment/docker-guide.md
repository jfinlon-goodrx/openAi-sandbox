# Docker Deployment Guide

Complete guide to containerizing and deploying OpenAI Platform projects.

## Overview

Docker allows you to package applications with all dependencies, making deployment consistent across environments.

## Prerequisites

- Docker Desktop (or Docker Engine)
- Docker Compose (optional, for multi-container setup)

## Building Docker Images

### Single Project

```bash
# Build Requirements Assistant
cd src/RequirementsAssistant/RequirementsAssistant.Api
docker build -t requirements-assistant:latest -f ../../../Dockerfile ../../..
```

### Using Docker Compose

```bash
# Build all services
docker-compose build

# Build specific service
docker-compose build requirements-assistant
```

## Running Containers

### Single Container

```bash
docker run -d \
  -p 5001:8080 \
  -e OpenAI__ApiKey=your-api-key \
  -e ASPNETCORE_ENVIRONMENT=Development \
  --name requirements-assistant \
  requirements-assistant:latest
```

### Docker Compose

```bash
# Start all services
docker-compose up -d

# Start specific service
docker-compose up -d requirements-assistant

# View logs
docker-compose logs -f requirements-assistant

# Stop all services
docker-compose down
```

## Environment Variables

Create a `.env` file for Docker Compose:

```env
OPENAI_API_KEY=your-openai-api-key
JIRA_BASE_URL=https://your-domain.atlassian.net
JIRA_USERNAME=your-email@example.com
JIRA_API_TOKEN=your-jira-token
CONFLUENCE_BASE_URL=https://your-domain.atlassian.net/wiki
CONFLUENCE_USERNAME=your-email@example.com
CONFLUENCE_API_TOKEN=your-confluence-token
GITHUB_TOKEN=your-github-token
SLACK_WEBHOOK_URL=https://hooks.slack.com/services/YOUR/WEBHOOK/URL
```

## Health Checks

All containers include health checks:

```bash
# Check health
curl http://localhost:5001/health

# Check container health status
docker ps
# Look for "healthy" status
```

## Logs

### View Logs

```bash
# Single container
docker logs requirements-assistant

# Follow logs
docker logs -f requirements-assistant

# Docker Compose
docker-compose logs -f
```

### Log Volume

Logs are persisted to `./logs` directory (mounted volume).

## Multi-Stage Build

The Dockerfile uses multi-stage builds:
1. **Build stage**: Compiles the application
2. **Runtime stage**: Contains only runtime dependencies (smaller image)

## Production Considerations

### Security

1. **Secrets Management**: Use Docker secrets or environment variables
2. **Non-root User**: Run containers as non-root user
3. **Image Scanning**: Scan images for vulnerabilities
4. **Network Policies**: Restrict container networking

### Performance

1. **Resource Limits**: Set CPU and memory limits
2. **Health Checks**: Configure appropriate intervals
3. **Logging**: Use structured logging (Serilog)
4. **Monitoring**: Integrate with monitoring tools

### Example Production Dockerfile

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Create non-root user
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copy published files
COPY --from=build /app/publish .

# Change ownership
RUN chown -R appuser:appuser /app

# Switch to non-root user
USER appuser

EXPOSE 8080
ENTRYPOINT ["dotnet", "RequirementsAssistant.Api.dll"]
```

## Kubernetes Deployment

### Basic Deployment

```yaml
apiVersion: apps/v1
kind: Deployment
metadata:
  name: requirements-assistant
spec:
  replicas: 3
  selector:
    matchLabels:
      app: requirements-assistant
  template:
    metadata:
      labels:
        app: requirements-assistant
    spec:
      containers:
      - name: requirements-assistant
        image: requirements-assistant:latest
        ports:
        - containerPort: 8080
        env:
        - name: OpenAI__ApiKey
          valueFrom:
            secretKeyRef:
              name: openai-secrets
              key: api-key
        livenessProbe:
          httpGet:
            path: /health
            port: 8080
          initialDelaySeconds: 30
          periodSeconds: 10
        readinessProbe:
          httpGet:
            path: /health/ready
            port: 8080
          initialDelaySeconds: 10
          periodSeconds: 5
```

## Troubleshooting

**Issue:** Container won't start
- **Solution:** Check logs: `docker logs <container-name>`
- **Solution:** Verify environment variables are set
- **Solution:** Check port conflicts

**Issue:** Health check failing
- **Solution:** Ensure `/health` endpoint is accessible
- **Solution:** Check application logs for errors
- **Solution:** Verify dependencies are available

**Issue:** Out of memory
- **Solution:** Increase Docker memory limit
- **Solution:** Optimize application memory usage
- **Solution:** Use resource limits in docker-compose

## Resources

- [Docker Documentation](https://docs.docker.com/)
- [.NET Docker Images](https://hub.docker.com/_/microsoft-dotnet-aspnet)
- [Docker Compose Documentation](https://docs.docker.com/compose/)
