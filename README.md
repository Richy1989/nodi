# Nodi

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

`nodiCore/appsettings.json` supports switching between SQLite (default) and PostgreSQL:

```json
{
  "DatabaseProvider": "Sqlite",
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=nodi.db"
  }
}
```

Set `"DatabaseProvider": "PostgreSQL"` and update the connection string to use Postgres.
