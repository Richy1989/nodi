# ── Stage 1: build ────────────────────────────────────────────────────────────
# Build context must be the repo root so nodeCommon is reachable.
# Run: docker build -f nodiCore/Dockerfile -t nodi-core .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy shared library and API project files first to leverage layer cache.
# If only source changes (not .csproj), the restore layer is reused.
COPY nodeCommon/nodeCommon.csproj nodeCommon/
COPY nodiCore/nodiCore.csproj     nodiCore/
RUN dotnet restore nodiCore/nodiCore.csproj

# Copy full source and publish a Release build
COPY nodeCommon/ nodeCommon/
COPY nodiCore/   nodiCore/
RUN dotnet publish nodiCore/nodiCore.csproj -c Release -o /app/publish --no-restore

# ── Stage 2: runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Create the data folder used by SQLite; mount a volume here in production
RUN mkdir -p /app/data

# Secrets and environment-specific settings should be passed via environment
# variables at runtime — do NOT bake them into the image.
# Required at runtime:
#   Jwt__Key           — long random secret, min 32 chars
#   Admin__Password    — initial admin password (only used on first run)
# Optional overrides:
#   Database__Provider=postgresql
#   ConnectionStrings__PostgreSQL=Host=...
#   ASPNETCORE_URLS=http://+:5100

ENV ASPNETCORE_URLS=http://+:5100
EXPOSE 5100

ENTRYPOINT ["dotnet", "nodiCore.dll"]
