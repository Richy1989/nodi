# Combined nodi image — runs nodiCore (API) and nodiWeb (Blazor) in one container.
# supervisord manages both processes; only the web port is exposed to the host.
#
# Build context must be the repo root:
#   docker build -t yourname/nodi .

# ── Stage 1: build nodiCore ───────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-core
WORKDIR /src

COPY nodeCommon/nodeCommon.csproj nodeCommon/
COPY nodiCore/nodiCore.csproj     nodiCore/
RUN dotnet restore nodiCore/nodiCore.csproj

COPY nodeCommon/ nodeCommon/
COPY nodiCore/   nodiCore/
RUN dotnet publish nodiCore/nodiCore.csproj -c Release -o /app/core --no-restore

# ── Stage 2: build nodiWeb ────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-web
WORKDIR /src

COPY nodeCommon/nodeCommon.csproj nodeCommon/
COPY nodiWeb/nodiWeb.csproj       nodiWeb/
RUN dotnet restore nodiWeb/nodiWeb.csproj

COPY nodeCommon/ nodeCommon/
COPY nodiWeb/    nodiWeb/
RUN dotnet publish nodiWeb/nodiWeb.csproj -c Release -o /app/web

# ── Stage 3: runtime ──────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# supervisord manages both dotnet processes
RUN apt-get update \
 && apt-get install -y --no-install-recommends supervisor \
 && rm -rf /var/lib/apt/lists/*

COPY --from=build-core /app/core ./core
COPY --from=build-web  /app/web  ./web

# Persistent data (SQLite database, uploads)
RUN mkdir -p /app/data

COPY docker/supervisord.conf /etc/supervisor/conf.d/nodi.conf
COPY docker/start.sh         /start.sh
RUN chmod +x /start.sh

# Default ports — override at runtime with -e CORE_PORT / -e WEB_PORT.
# CORE_PORT is internal only; only WEB_PORT needs to be mapped on the host.
ENV CORE_PORT=5100
ENV WEB_PORT=8080

# Only the web port is exposed — core is internal to the container
EXPOSE 8080

ENTRYPOINT ["/start.sh"]
