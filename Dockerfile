# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY shared/*/*.csproj ./shared/
COPY src/*/*.Api/*.csproj ./src/
COPY tests/*/*.csproj ./tests/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY . .

# Build solution
WORKDIR /src
RUN dotnet build -c Release --no-restore

# Publish Requirements Assistant API (example)
WORKDIR /src/src/RequirementsAssistant/RequirementsAssistant.Api
RUN dotnet publish -c Release --no-build -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080
EXPOSE 8081

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "RequirementsAssistant.Api.dll"]
