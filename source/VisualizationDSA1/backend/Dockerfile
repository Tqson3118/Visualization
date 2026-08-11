# ── Stage 1: Build ────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Install dependencies for native builds if needed
RUN apt-get update && apt-get install -y --no-install-recommends \
    clang \
    && rm -rf /var/lib/apt/lists/*

# Copy csproj files first for layer caching
COPY src/Domain/Domain.csproj src/Domain/
COPY src/Application/Application.csproj src/Application/
COPY src/Infrastructure/Infrastructure.csproj src/Infrastructure/
COPY src/WebApi/WebApi.csproj src/WebApi/
RUN dotnet restore src/WebApi/WebApi.csproj

# Copy all source and publish
COPY src/ src/
RUN dotnet publish src/WebApi/WebApi.csproj -c Release -o /app/publish \
    --self-contained false \
    /p:PublishTrimmed=false

# ── Stage 2: Runtime ─────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y --no-install-recommends \
    curl \
    && rm -rf /var/lib/apt/lists/*

# Create non-root user for security
RUN groupadd -r appgroup && useradd -r -g appgroup -u 1000 appuser

# Copy published app
COPY --from=build /app/publish .
COPY --from=build /src/src/Infrastructure/Data/SeedContent ./Data/SeedContent

# Change ownership to non-root user
RUN chown -R appuser:appgroup /app

ENV ASPNETCORE_URLS=http://+:5055
ENV ASPNETCORE_ENVIRONMENT=Production
ENV DOTNET_EnableDiagnostics=0
ENV COMPlus_ThreadPool_ForceMinWorkerThreads=20
ENV COMPlus_ThreadPool_ForceMaxWorkerThreads=100

EXPOSE 5055

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:5055/health || exit 1

USER appuser
ENTRYPOINT ["dotnet", "WebApi.dll"]
