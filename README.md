# Nodi

> **Work in progress.** This project is under heavy active development and has not been released. Expect breaking changes, incomplete features, and rough edges.

A multi-platform note-taking app built with .NET 10.

## Projects

| Project | Type | Description |
|---|---|---|
| `nodiCore` | ASP.NET Core Web API | REST API backend with JWT auth |
| `nodiWeb` | Blazor Server | Web frontend using MudBlazor |
| `nodiApp` | .NET MAUI | Mobile/desktop app with offline sync |
| `nodeCommon` | Class Library | Shared enums and DTOs |

## Features

- Create text and checklist notes
- Pin, archive, and delete (soft) notes
- Tag-based organisation and search
- 12 colour themes per note
- User accounts with admin panel
- Offline support in the mobile app (syncs when connected)

## Tech Stack

- **Backend:** ASP.NET Core, Entity Framework Core 9, SQLite / PostgreSQL, JWT auth
- **Web:** Blazor Server, MudBlazor
- **Mobile:** .NET MAUI, SQLite (local), MVVM Toolkit
- **Shared:** `nodeCommon` class library (enums, DTOs)

## Getting Started

### Prerequisites

- .NET 10 SDK

### Run the API

```bash
cd nodiCore
dotnet run
```

API runs on `http://localhost:5100` by default. A default admin account is seeded on first run:

- **Username:** `admin`
- **Password:** `Admin1234!`

### Run the Web App

```bash
cd nodiWeb
dotnet run
```

Configure the API URL in `appsettings.json` if needed.

### Run the Mobile App

```bash
cd nodiApp
dotnet build -t:Run -f net10.0-android   # Android
dotnet build -t:Run -f net10.0-windows10.0.19041.0  # Windows
```

## Configuration

`nodiCore` is configured via `appsettings.json`. Every value can be overridden with an environment variable — useful for Docker or production deployments. ASP.NET Core maps nested JSON keys to env vars using `__` (double underscore) as the separator.

### Environment Variables

| Environment variable | appsettings.json key | Default | Description |
|---|---|---|---|
| `DataFolder` | `DataFolder` | `data` | Directory where all persistent files are stored (SQLite database, future uploads, etc.). Relative paths are resolved from the working directory. |
| `Database__Provider` | `Database:Provider` | `sqlite` | Database backend. Use `sqlite` for local/dev or `postgresql` for production. |
| `ConnectionStrings__PostgreSQL` | `ConnectionStrings:PostgreSQL` | — | PostgreSQL connection string. Required when `Database__Provider` is `postgresql`. |
| `Jwt__Key` | `Jwt:Key` | *(none — required)* | Secret key used to sign JWT tokens. Must be at least 32 characters. **Change this before deploying.** |
| `Jwt__Issuer` | `Jwt:Issuer` | `nodiCore` | JWT issuer claim. |
| `Jwt__Audience` | `Jwt:Audience` | `nodiClients` | JWT audience claim. |
| `Jwt__ExpiryHours` | `Jwt:ExpiryHours` | `72` | How long issued tokens remain valid, in hours. |
| `Admin__Username` | `Admin:Username` | `admin` | Username of the admin account seeded on first run. |
| `Admin__Email` | `Admin:Email` | `admin@nodi.local` | Email of the seeded admin account. |
| `Admin__Password` | `Admin:Password` | `Admin1234!` | Password of the seeded admin account. **Change this before deploying.** |

> The admin account is only created if no admin user exists in the database (i.e. on first run). Changing these values after the database has been seeded has no effect.

### Minimal production example (Docker)

```bash
docker run \
  -e DataFolder=/data \
  -e Database__Provider=postgresql \
  -e ConnectionStrings__PostgreSQL="Host=db;Database=nodi;Username=nodi;Password=secret" \
  -e Jwt__Key="replace-with-a-long-random-secret-at-least-32-chars" \
  -e Admin__Password="replace-with-a-strong-password" \
  -v /host/nodi-data:/data \
  -p 5100:5100 \
  nodicore
```

### SQLite (local / dev)

No configuration needed. The database is created at `data/nodi.db` relative to the working directory on first run.
