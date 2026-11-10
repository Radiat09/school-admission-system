# AI Usage

AI (Kilo) was used as a coding assistant throughout implementation. Specific uses:

- **Backend scaffolding & fixes**: Generated and corrected `AdmissionService`, `ClassRepository` (`SELECT ... FOR UPDATE`), `HoldRepository`, `HoldExpirationService`, `ErrorHandlingMiddleware`, and `Program.cs` (DI, migration application, CORS, health checks).
- **Compile-error resolution**: Fixed NU1605 package version alignment, missing `using` directives, CS1061/CS0103 compile errors, and the `GET /me/holds` route attribute.
- **EF Core migration**: Produced `20261110203000_UseTimestamptz.cs` to switch timestamps to `timestamptz`.
- **Test fixes**: Rewrote `ConcurrencyTests` (shared `HttpClient`, tuple capture for response capture), fixed test constructors, `X-Student-Id` header handling, and student uniqueness in `Get_My_Holds`.
- **Concurrency bug diagnosis**: Added temporary exception logging to the middleware, identified `53300: too many clients` (Postgres default `max_connections=100`), and applied the `max_connections=200` + `MaxPoolSize=200` fix.
- **Frontend**: Resolved TypeScript errors across `page.tsx`, `admin/page.tsx`, `applications/[holdId]/page.tsx`, `my-applications/page.tsx`; added `@vitejs/plugin-react` to devDependencies; enabled vitest `globals: true`; added `src/lib/validations.test.ts` (6 tests).
- **Infrastructure**: `docker-compose.yml` (postgres command flag, connection strings, health checks), `.env.example`, `.gitignore`.
- **Documentation**: Wrote `README.md`, `ASSUMPTIONS.md`, `AI_USAGE.md`, `AGENTS.md`.

All AI-generated code was reviewed, compiled, and verified against the test suite and docker-compose before being accepted.
