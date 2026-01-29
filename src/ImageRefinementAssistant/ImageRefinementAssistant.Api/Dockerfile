# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files for dependency restoration
COPY *.sln ./
COPY shared/*/*.csproj ./shared/
COPY src/ImageRefinementAssistant/*/*.csproj ./src/ImageRefinementAssistant/

# Restore dependencies
RUN dotnet restore

# Copy all source files
COPY . .

# Build the project
WORKDIR /src/src/ImageRefinementAssistant/ImageRefinementAssistant.Api
RUN dotnet build -c Debug --no-restore

# Publish the project (no publish needed for watch mode, but keep for consistency)
RUN dotnet publish -c Debug --no-build -o /app/publish || true

# Development/Debug stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS development
WORKDIR /app

# Install debugging tools and curl for health checks
RUN apt-get update && \
    apt-get install -y curl procps unzip && \
    rm -rf /var/lib/apt/lists/*

# Install VSDBG for remote debugging
RUN curl -sSL https://aka.ms/getvsdbgsh | bash /dev/stdin -v latest -l /vsdbg

# Copy source files (for watch mode)
WORKDIR /src
COPY . .

# Ensure shared projects are accessible
RUN mkdir -p /src/shared/OpenAIShared /src/shared/Common

# Expose ports
EXPOSE 8080 5000

# Set environment variables for development
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Development
ENV DOTNET_USE_POLLING_FILE_WATCHER=true

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/api/imagerefinement/health || exit 1

# Entry point (will be overridden in docker-compose for debugging)
# Default to watch mode for hot reload
WORKDIR /src/src/ImageRefinementAssistant/ImageRefinementAssistant.Api
ENTRYPOINT ["dotnet", "watch", "run", "--urls", "http://+:8080"]

# Production runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published files
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
  CMD curl -f http://localhost:8080/api/imagerefinement/health || exit 1

# Entry point
ENTRYPOINT ["dotnet", "ImageRefinementAssistant.Api.dll"]
