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

## 🐳 Docker (Minimal Production Example)

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

---

## 💾 SQLite (Local / Dev)

No configuration required.

The database is automatically created at:

```
programData/nodi.db
```

(relative to the working directory)

---