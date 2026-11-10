# School Admission System

A seat-hold admission system with optimistic concurrency control, row-level locking, and automatic hold expiration. Built with .NET 8 (ASP.NET Core + EF Core + Npgsql + PostgreSQL) backend and Next.js 14 (App Router) frontend, fully Dockerized.

## Architecture

```
postgres:15-alpine   (port 5432)  — database, max_connections=200
backend (.NET 8)     (port 8080)  — ASP.NET Core API, EF Core migrations applied on startup
frontend (Next.js 14)(port 3000)  — App Router, Tailwind, TanStack Query
```

Named volume: `postgres_data` (persists DB across restarts).

## Concurrency Model

- `ClassRepository.GetByIdLockedAsync` issues `SELECT ... FOR UPDATE` to lock the admission class row within a transaction.
- `AdmissionService.ApplyAsync`/`ConfirmAsync`/`CancelAsync` perform the read-lock-check-write sequence inside `BEGIN`/`COMMIT`, so two concurrent requests for the last seat serialize — only one succeeds.
- `AdmissionHold.Status` transitions: `HELD` → `CONFIRMED` (student confirms) or `CANCELLED` (student cancels or hold expires).
- `HoldExpirationService` polls every 10s and flips `HELD` holds older than `HOLD_DURATION_SECONDS` (default 600s) to `EXPIRED`, releasing the seat.
- A partial unique index `IX_AdmissionHolds_StudentId_ClassId_Active` (`WHERE Status IN ('HELD','CONFIRMED)`) enforces at most one active hold per student per class at the database level.
- Npgsql `MaxPoolSize=200` and Postgres `max_connections=200` so the 100-request concurrency test does not exhaust connections.

## Prerequisites

- Docker Desktop (Docker Compose v2)
- (Optional, for local dev without Docker) .NET 8 SDK, Node 20+

## Quick Start

```bash
# From the project root (where docker-compose.yml lives)
docker compose up --build
```

Services:
- Backend API: `http://localhost:8080` (health: `GET /health`)
- Frontend: `http://localhost:3000`
- PostgreSQL: `localhost:5432` (db `schooladmission`, user/pass `postgres`)

Stop: `docker compose down` (preserves the named volume).  
Stop and remove data: `docker compose down -v`.

## Configuration

Copy `.env.example` to `.env` and adjust as needed. Key variables:

| Variable | Default | Description |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | `Host=localhost;...;MaxPoolSize=200;` | Npgsql connection string |
| `HOLD_DURATION_SECONDS` | `600` | Seconds before a HELD hold expires |
| `CORS_ALLOWED_ORIGINS` | `http://localhost:3000` | Comma-separated CORS origins |
| `ASPNETCORE_ENVIRONMENT` | `Production` | ASP.NET Core environment |
| `NEXT_PUBLIC_API_BASE_URL` | `http://localhost:8080` | Frontend API base URL |

## API Reference

### Classes
- `GET /classes` — list all admission classes (with available seats)
- `GET /classes/{id}` — get a single class
- `POST /classes` — create a class (`{ name, totalSeats }`)
- `POST /classes/{id}/holds` — apply / create a hold (header `X-Student-Id`)

### Holds
- `POST /holds/{id}/confirm` — confirm a hold (header `X-Student-Id`)
- `POST /holds/{id}/cancel` — cancel a hold (header `X-Student-Id`)
- `GET /me/holds` — current student's holds (header `X-Student-Id`)

### Health
- `GET /health` — returns `Healthy`

## Testing

### Backend (.NET 8 tests via Docker)
Only .NET Core 3.1 runtime is installed locally, so tests run inside the .NET 8 SDK image against the running compose postgres:

```bash
docker run --rm --network school-admission-system_default \
  -e ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=schooladmission;Username=postgres;Password=postgres;MaxPoolSize=200;" \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -v "${PWD}/backend:/src" -w /src \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet test SchoolAdmission.Tests/SchoolAdmission.Tests.csproj
```

### Frontend (Next.js + vitest)
```bash
cd frontend
npm install
npm run typecheck   # tsc --noEmit
npm run lint        # next lint
npm run test        # vitest run
npm run build       # next build
```

## Project Layout

```
backend/
  SchoolAdmission.Api/     Controllers, Services, Repositories, Middleware, Data, BackgroundServices, Program.cs
  SchoolAdmission.Tests/   xUnit tests (AdmissionFlow, Concurrency, AdditionalBehavior)
frontend/
  app/                     Next.js App Router pages
  src/lib/                 api.ts, validations.ts, utils.ts
  vitest.config.ts
docker-compose.yml
.env.example
```

## Test Coverage

- Backend: 11 xUnit tests — admission flow (apply/confirm/cancel/reapply), concurrency (100 simultaneous applicants for limited seats), seat-release-on-cancel, duplicate-hold prevention, missing-student-header, invalid-student-id.
- Frontend: 6 vitest tests — Zod validation schemas (student ID, class creation).
