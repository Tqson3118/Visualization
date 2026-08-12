# Backend Dockerfile - multi-stage (.NET 10 / 2 project SDD A-5.1)

# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj truoc de tan dung layer cache cua restore
COPY src/DsaVisual.Api/DsaVisual.Api.csproj src/DsaVisual.Api/
COPY src/DsaVisual.Application/DsaVisual.Application.csproj src/DsaVisual.Application/
RUN dotnet restore src/DsaVisual.Api/DsaVisual.Api.csproj

# Copy toan bo source va publish
COPY src/ src/
RUN dotnet publish src/DsaVisual.Api/DsaVisual.Api.csproj -c Release -o /app/publish --self-contained false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# curl cho health check
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Non-root: image .NET 10 da co san user 'app' (uid 1654) - chi can chown
COPY --from=build /app/publish .
RUN chown -R app:app /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=30s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

USER app
ENTRYPOINT ["dotnet", "DsaVisual.Api.dll"]
