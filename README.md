# Meridian

Onboarding platform for new hires: HR manages an onboarding template and
publishes articles, managers track progress, and new hires work through their
personal onboarding board.

**Stack:** .NET 10 (C#) · EF Core · MSSQL · Angular · Serilog · FluentValidation · Mapster · Scrutor

---

## Quick start (Docker — recommended)

The whole app (SQL Server + API + frontend) runs with a single command.
Migrations and seed data are applied automatically on startup.

### Prerequisites

- [Git](https://git-scm.com/downloads)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or Docker
  Engine + Compose v2) — running before you start

### 1. Clone the repository

```bash
git clone git@github.com:Sala-Sergiu/Meridian.git
cd Meridian
```

(or via HTTPS: `git clone https://github.com/Sala-Sergiu/Meridian.git`)

### 2. Start the stack

```bash
docker compose up -d
```

What happens on first run:

1. Docker pulls the SQL Server 2022 image and builds the API and frontend
   images (a few minutes the first time; cached afterwards).
2. The `mssql` container starts and Compose waits until its healthcheck
   passes (SQL Server accepts connections).
3. The `api` container starts, **applies all EF Core migrations** (schema +
   seed users + onboarding template) and seeds a demo onboarding board.
4. The `web` container serves the Angular production bundle via nginx and
   proxies `/api` to the API — same origin, no CORS setup needed.

### 3. Open the app

| What | URL |
|---|---|
| App (frontend) | http://localhost:8081 |
| Swagger UI | http://localhost:5044/swagger |
| Health check | http://localhost:5044/health |

### 4. Log in with a demo account

| Role | Email | Password |
|---|---|---|
| New hire | `newhire@meridian.local` | `NewHire#123` |
| HR | `hr@meridian.local` | `HrAdmin#123` |
| Manager | `manager@meridian.local` | `Manager#123` |

These are seeded dev/demo credentials for the Meridian login only.

### Useful commands

```bash
docker compose logs -f api     # follow API logs (migrations, requests)
docker compose ps              # container status + health
docker compose down            # stop the stack (data is kept)
docker compose down -v         # stop AND wipe the database volume (fresh start)
docker compose up -d --build   # rebuild images after pulling new code
```

The SA password defaults to a dev-only value (`Your_strong_Password123`).
Override it if needed:

```bash
MSSQL_SA_PASSWORD='My$tr0ngerPwd!' docker compose up -d
```

---

## Local development (hot reload)

Use this flow when changing code. Only SQL Server runs in Docker; the API and
the Angular dev server run on your machine.

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- [Node.js 24+](https://nodejs.org/) (npm 11+)
- Docker (for the database only)

### 1. Start the database

```bash
docker compose up -d mssql
```

### 2. Run the API

```bash
dotnet run --project src/Meridian.Api
```

- Listens on **http://localhost:5044** (see
  `src/Meridian.Api/Properties/launchSettings.json`).
- Connection string defaults to `localhost,1433` with the dev SA password
  (`src/Meridian.Api/appsettings.json`) — matches the compose `mssql` service.
- Swagger: http://localhost:5044/swagger

### 3. Run the Angular dev server

```bash
cd client
npm install
npm start
```

- Serves on **http://localhost:4200** with hot reload, calling the API on
  `localhost:5044` (CORS for `localhost:4200` is enabled in Development).

### Run the tests

```bash
dotnet test                    # unit tests + architecture tests
cd client && npm test          # frontend tests (vitest)
```

---

## Troubleshooting

- **`api` container exits / restarts on first run** — SQL Server can take a
  while to initialize on the very first start. Compose waits for the
  healthcheck, but if the machine is slow just run `docker compose up -d`
  again; migrations are idempotent.
- **Port already in use (1433, 5044 or 8081)** — stop the conflicting service
  (e.g. a local SQL Server instance on 1433) or edit the host-side port in
  `docker-compose.yml`.
- **Want a clean database** — `docker compose down -v` then
  `docker compose up -d`; migrations and seed run again from scratch.
- **Login fails right after startup** — check `docker compose logs api` and
  confirm migrations finished; `curl http://localhost:5044/health` should
  return `Healthy`.

## Project layout

```
src/Meridian.Domain   entities, repository interfaces, domain rules
src/Meridian.Dal      EF Core, DbContext, migrations, repository implementations
src/Meridian.Bll      services, validators, query pipeline, DTOs
src/Meridian.Api      controllers, middleware, DI composition root, Swagger
client/               Angular frontend
tests/                unit tests + architecture tests
```

Design notes live in `DECISIONS.md`, `ASSUMPTION.md`, `REFLECTION.md` and
`WHAT_I_WOULD_DO_NEXT.md`.
