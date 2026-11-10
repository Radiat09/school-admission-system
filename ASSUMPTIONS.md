# Assumptions & Design Decisions

## Concurrency & Capacity
- **Row-level locking (`SELECT ... FOR UPDATE`)** is used instead of relying solely on application-level checks, so concurrent applicants for the last seat are serialized at the database level and only one succeeds.
- **Postgres `max_connections=200`** and **Npgsql `MaxPoolSize=200`** are configured because the default `max_connections=100` was exceeded under the 100-request concurrency test, producing intermittent `500` (`53300: too many clients`).
- Hold duration defaults to **600 seconds (10 minutes)** via `HOLD_DURATION_SECONDS`; the expiration service polls every **10 seconds**.

## Data Integrity
- A **partial unique index** `IX_AdmissionHolds_StudentId_ClassId_Active` (`WHERE Status IN ('HELD','CONFIRMED)`) enforces at most one active hold per student per class at the DB level, as a backstop to application checks.
- Timestamps use `timestamptz` for correct timezone handling.
- Database schema is created idempotently via `EnsureSchemaAsync` at startup as a safety net; EF Core migrations remain the source of truth.

## API & Auth
- Student identity is passed via the `X-Student-Id` request header (no full auth system in scope).
- CORS is restricted to `CORS_ALLOWED_ORIGINS` (default `http://localhost:3000`).
- Responses use camelCase JSON; enums serialize as strings.

## Frontend
- Next.js 14 App Router with **standalone output** for Docker.
- TanStack Query 5 for server state; Zod for client-side validation.
- `vitest` + React Testing Library for tests; `jsdom` environment.
- No backend test files exist in the frontend repo initially; a small validation test suite was added so `npm run test` passes cleanly.

## Infrastructure
- Docker Compose orchestrates postgres → backend → frontend with `depends_on` health conditions.
- A named volume `postgres_data` persists the database across container restarts.
- Only .NET Core 3.1 runtime is installed locally, so backend build/test runs inside the `mcr.microsoft.com/dotnet/sdk:8.0` image.

## What Was Intentionally Not Done
- No email/SMS notification service.
- No admin UI beyond the class-creation page (held out of scope).
- No pagination on class listing (expected volume is low).
- No JWT/session auth; `X-Student-Id` header is the student identifier.
