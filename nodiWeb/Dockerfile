# ── Stage 1: build ────────────────────────────────────────────────────────────
# Build context must be the repo root so nodeCommon is reachable.
# Run: docker build -f nodiWeb/Dockerfile -t nodi-web .
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files first to leverage layer cache for restore
COPY nodeCommon/nodeCommon.csproj nodeCommon/
COPY nodiWeb/nodiWeb.csproj       nodiWeb/
RUN dotnet restore nodiWeb/nodiWeb.csproj

# Copy full source and publish a Release build
COPY nodeCommon/ nodeCommon/
COPY nodiWeb/    nodiWeb/
RUN dotnet publish nodiWeb/nodiWeb.csproj -c Release -o /app/publish --no-restore

# ── Stage 2: runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

# Required at runtime:
#   ApiBaseUrl — URL of the nodiCore container, e.g. http://nodi-core:5100

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "nodiWeb.dll"]
