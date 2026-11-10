# Agent Instructions

Project: School Admission System (.NET 8 backend + Next.js 14 frontend, Dockerized)

## Commands

### Full system (Docker)
```bash
# From project root
docker compose up --build        # build + start all services
docker compose down              # stop (keeps named volume)
docker compose down -v           # stop + remove data volume
```

### Backend tests (must run in Docker; only .NET Core 3.1 runtime is installed locally)
```bash
docker run --rm --network school-admission-system_default \
  -e ConnectionStrings__DefaultConnection="Host=postgres;Port=5432;Database=schooladmission;Username=postgres;Password=postgres;MaxPoolSize=200;" \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -v "${PWD}/backend:/src" -w /src \
  mcr.microsoft.com/dotnet/sdk:8.0 \
  dotnet test SchoolAdmission.Tests/SchoolAdmission.Tests.csproj
```
Expected: 11 passed, 0 failed.

### Frontend
```bash
cd frontend
npm install
npm run typecheck   # tsc --noEmit  (expect: no errors)
npm run lint        # next lint     (expect: warnings OK, no errors)
npm run test        # vitest run    (expect: 6 passed)
npm run build       # next build    (expect: succeeds)
```

### Verification checklist (before declaring done)
- [ ] `docker compose ps` → all three services `Up (healthy)` / `Up`
- [ ] Backend tests → 11/11 pass
- [ ] Frontend typecheck → clean
- [ ] Frontend lint → no errors
- [ ] Frontend tests → 6/6 pass
- [ ] Frontend build → succeeds
- [ ] `GET http://localhost:8080/health` → `Healthy`

## Key Files
- `backend/SchoolAdmission.Api/Program.cs` — DI, migrations, CORS, health, middleware
- `backend/SchoolAdmission.Api/Services/AdmissionService.cs` — apply/confirm/cancel with row locking
- `backend/SchoolAdmission.Api/Repositories/ClassRepository.cs` — `GetByIdLockedAsync` (`FOR UPDATE`)
- `backend/SchoolAdmission.Api/BackgroundServices/HoldExpirationService.cs` — hold expiry poller
- `frontend/vitest.config.ts` — vitest config (`globals: true`, jsdom)
- `docker-compose.yml` — service orchestration, `max_connections=200`
- `.env.example` — environment variable template

## Notes
- Backend build/test must run in Docker (no .NET 8 SDK locally).
- Student identity is via `X-Student-Id` header; no JWT/auth in scope.
- `expect` is a global in vitest (enabled via `globals: true`); do not remove.
