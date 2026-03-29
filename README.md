# Nodi

> ⚠️ **Work in progress**  
> This project is under heavy active development and has not been released yet.  
> Expect breaking changes, incomplete features, and rough edges.

A multi-platform note-taking app built with **.NET 10**.

---

## 🧩 Projects

| Project | Type | Description |
|--------|------|-------------|
| `nodiCore` | ASP.NET Core Web API | REST API backend with JWT authentication |
| `nodiWeb` | Blazor Server | Web frontend using MudBlazor |
| `nodiApp` | .NET MAUI | Mobile/desktop app with offline sync |
| `nodeCommon` | Class Library | Shared enums and DTOs |

---

## ✨ Features

- Create text and checklist notes  
- Pin, archive, and soft-delete notes  
- Tag-based organisation and search  
- 12 colour themes per note  
- User accounts with admin panel  
- Offline support in the mobile app (syncs when reconnected)

---

## 🛠 Tech Stack

**Backend**
- ASP.NET Core  
- Entity Framework Core 9  
- SQLite / PostgreSQL  
- JWT authentication  

**Web**
- Blazor Server  
- MudBlazor  

**Mobile**
- .NET MAUI  
- SQLite (local storage)  
- MVVM Toolkit  

**Shared**
- `nodeCommon` (enums, DTOs)

---

## 🚀 Getting Started

### Prerequisites

- .NET 10 SDK

---

### ▶️ Run the API

```bash
cd nodiCore
dotnet run
```

API runs on:  
👉 http://localhost:5100

A default admin account is seeded on first run:

- **Username:** `admin`  
- **Password:** `Admin1234!`

---

### 🌐 Run the Web App

```bash
cd nodiWeb
dotnet run
```

If needed, configure the API URL in `appsettings.json`.

---

### 📱 Run the Mobile App

```bash
cd nodiApp

# Android
dotnet build -t:Run -f net10.0-android  

# Windows
dotnet build -t:Run -f net10.0-windows10.0.19041.0
```

---

## ⚙️ Configuration

`nodiCore` is configured via `appsettings.json`.

All values can be overridden via environment variables  
(use `__` as separator for nested keys).

---

## 🔑 Environment Variables

| Environment variable | appsettings.json key | Default | Description |
|---------------------|---------------------|--------|-------------|
| `DataFolder` | `DataFolder` | `programData` | Directory for persistent data (database, uploads, etc.) |
| `Database__Provider` | `Database:Provider` | `sqlite` | `sqlite` (dev) or `postgresql` (prod) |
| `ConnectionStrings__PostgreSQL` | `ConnectionStrings:PostgreSQL` | — | Required for PostgreSQL |
| `Jwt__Key` | `Jwt:Key` | *(required)* | Secret for signing JWT tokens (min. 32 chars) |
| `Jwt__Issuer` | `Jwt:Issuer` | `nodiCore` | JWT issuer |
| `Jwt__Audience` | `Jwt:Audience` | `nodiClients` | JWT audience |
| `Jwt__ExpiryHours` | `Jwt:ExpiryHours` | `72` | Token lifetime in hours |
| `Admin__Username` | `Admin:Username` | `admin` | Seeded admin username |
| `Admin__Email` | `Admin:Email` | `admin@nodi.local` | Seeded admin email |
| `Admin__Password` | `Admin:Password` | `Admin1234!` | Seeded admin password (**change this**) |

> ℹ️ The admin account is only created if no admin user exists (first run only).  
> Changing these values later has no effect.

---

## 🐳 Docker

nodi ships as a **single combined image**. Both the API (`nodiCore`) and the web app (`nodiWeb`) run inside one container managed by `supervisord`. Only the web port is exposed to the host — the API stays internal.

```
┌─────────────────────────────────┐
│  nodi container                 │
│                                 │
│  nodiWeb  :8080  ───────────────────► browser
│      │                          │
│  nodiCore :5100  (internal)     │
│      │                          │
│  /app/nodiData  (SQLite + files)    │
└─────────────────────────────────┘
```

### Quick start

```bash
docker run -d \
  -e Jwt__Key="replace-with-a-long-random-secret-at-least-32-chars" \
  -e Admin__Password="replace-with-a-strong-password" \
  -v /host/nodi-data:/app/nodiData \
  -p 8080:8080 \
  richy1989/nodi
```

Web app available at `http://localhost:8080`.

### Docker Compose

Copy `.env.example` to `.env` and fill in the required values, then:

```bash
docker compose up
```

### Build and push

```powershell
# Requires: Docker Desktop, docker login

# Build and push as latest
.\build.ps1 -Username yourname

# Build with a version tag (also tag as latest separately if needed)
.\build.ps1 -Username yourname -Tag 1.0.0

# Build locally without pushing
.\build.ps1 -Username yourname -NoPush
```

### Run on Unraid

1. **Docker → Add Container**, set repository to `richy1989/nodi`
2. Add a **Port** mapping: host `8080` → container `8080`
3. Add a **Path** mapping: host `/mnt/user/appdata/nodi` → container `/app/nodiData`
4. Add the required environment variables (see table above)
5. Click **Apply**

The API port (`CORE_PORT`) does **not** need to be mapped — it never leaves the container.

### Environment variables (Docker)

| Variable | Default | Description |
|---|---|---|
| `Jwt__Key` | *(required)* | Secret for signing tokens — min 32 characters |
| `Admin__Password` | `Admin1234!` | Seeded admin password — **change this** |
| `Admin__Username` | `admin` | Seeded admin username |
| `Admin__Email` | `admin@nodi.local` | Seeded admin email |
| `WEB_PORT` | `8080` | Port the web app listens on |
| `CORE_PORT` | `5100` | Internal API port — no host mapping needed |
| `Database__Provider` | `sqlite` | `sqlite` or `postgresql` |
| `ConnectionStrings__PostgreSQL` | — | Required when using PostgreSQL |

### Data persistence

All persistent files (SQLite database, uploads) live at `/app/nodiData` inside the container. Mount a volume or host path there to keep data across container recreations.

---

## 💾 SQLite (Local / Dev)

No configuration required.

The database is automatically created at:

```
programData/nodi.db
```

(relative to the working directory)

---