# CLAUDE.md

## Project Overview

Ordning is an inventory system for organizing items and locations. Locations are the core organizing concept (each with a unique, human-readable string ID, and a parent/child chain), and items live inside locations and can be moved between them. See [spec.md](spec.md) for the full requirements (must/should/could/won't).

| Component    | Technology                                                 |
|-------------|-----------------------------------------------------------|
| Backend     | .NET 8 / ASP.NET Core 8.0 / C#                            |
| Frontend    | React 19 + TypeScript + Vite + Tailwind CSS               |
| Database    | PostgreSQL (dbup migrations, auto-run at startup)         |
| API Client  | Auto-generated TypeScript types via `openapi-typescript`  |
| Data Access | Dapper + EasyReasy.Database / EasyReasy.Database.Npgsql   |
| Auth/Config | EasyReasy.EnvironmentVariables, EasyReasy.Auth            |
| Testing     | xUnit (backend), Vitest (frontend)                        |

## Git Workflow

Default branch is `master`, NOT `main`. Always use `master` for PRs and git operations.

## Build & Development Commands

### Backend
```bash
dotnet build                                  # Build entire solution
dotnet test Ordning.Server.Tests              # Run unit/integration tests (requires PostgreSQL)
dotnet test --filter "FullyQualifiedName~ClassName.MethodName" Ordning.Server.Tests  # Single test
```

Tests need a PostgreSQL instance on `localhost:5432`. `setup_test_db.sh` spins one up in Docker (`postgres:14-alpine`, user/password `postgres`, db `ordning_test`).

### Frontend
```bash
cd ordning.client
npm install        # Install dependencies
npm run build      # Type-check + build
npm run lint       # Run ESLint
npm run dev        # Vite dev server (normally started automatically by the backend via SpaProxy)
npm run codegen    # Regenerate API types from backend Swagger
```

The frontend is never started separately — the backend serves it automatically (SpaProxy on `https://localhost:55903`). The backend listens on `https://localhost:7152` / `http://localhost:5196`.

### Codegen (regenerating API types)
Swagger is only enabled in Development. After changing backend API endpoints or DTOs, start the backend in Development and regenerate:
```bash
# Terminal 1: run the backend (Development) so the Swagger JSON is served on :5196
dotnet run --project Ordning.Server

# Terminal 2: regenerate src/types/api.ts
cd ordning.client && npm run codegen
```
`npm run codegen` reads `http://localhost:5196/swagger/v1/swagger.json`.

## Solution Structure

```
Ordning.sln
├── Ordning.Server/                  # ASP.NET Core 8.0 API
│   ├── Auth/                        # Auth request validation, password hashing
│   ├── Database/                    # Migrator, DI config, default admin seeding
│   ├── Items/                       # Items domain
│   ├── Locations/                   # Locations domain
│   ├── Users/                       # Users domain
│   ├── Middleware/                  # Exception handling middleware
│   ├── RateLimiting/                # Rate limit policies (see RateLimiting.md)
│   ├── Migrations/                  # dbup SQL migrations (embedded resources)
│   └── Properties/                  # Launch settings
├── Ordning.Server.Tests/            # xUnit tests (requires PostgreSQL)
└── ordning.client/                  # React 19 + TypeScript + Vite + Tailwind frontend
    └── src/
        ├── components/              # UI components (reusable library in ui/)
        ├── contexts/               # React contexts (AuthContext)
        ├── pages/                  # Page components
        ├── services/               # API and auth service clients
        ├── styles/                 # Global styles / CSS variables
        └── types/                  # Generated API types (api.ts)
```

Each backend domain follows: **Controllers → Services → Repositories → Models**.

All `EasyReasy.*` NuGet packages are in separate repos but we own the source.

## Architecture

### Key Patterns

- **Repository Pattern**: Repositories (data access) → Services (business logic) → Controllers (HTTP only, no business logic)
- **SOLID principles**: Dependency inversion, low coupling, one public type per file
- **Test coverage**: Aim for full unit test coverage with proper dependency injection. Integration tests run against PostgreSQL in `Ordning.Server.Tests`.
- **Convention-first**: Look at how similar features are implemented before building new ones

### Database

- PostgreSQL with dbup migrations in `Ordning.Server/Migrations/` (embedded resources, auto-run at startup via `DatabaseMigrator`)
- **Feature branch migration rule**: If a migration was added on the current feature branch and needs changes, modify it in-place rather than adding a second migration. This keeps `master` clean — one migration per feature, not iterative fixups. Never modify migrations that have already been merged to `master`.

### Auth

- JWT-based via EasyReasy.Auth (`AddEasyReasyAuth`, issuer `ordning`). Username/password login is enabled; API keys are not.
- `DefaultAdminUserInitializer` (hosted service) seeds a default admin user on startup.
- Passwords hashed via `IPasswordHasher` / `SecurePasswordHasher`.

### Key Libraries

| Library                       | Purpose                                       |
|------------------------------|-----------------------------------------------|
| EasyReasy.EnvironmentVariables | Strongly-typed env vars, validated at startup |
| EasyReasy.Auth               | Authentication primitives (JWT)               |
| EasyReasy.Database / .Npgsql | Database session/connection layer over Npgsql |
| Dapper                       | Query mapping extension methods               |
| dbup                         | Database migration runner                     |

Use the `nugetdocs` MCP tool for NuGet package usage questions.

## Hard Rules — Stop and Ask

If any of these arise, **stop immediately** and ask for guidance before continuing:

1. **Architectural dead-end**: Implementation cannot be solved cleanly — explain the constraint, suggest clean alternatives
2. **Missing API types**: Frontend needs types that don't exist yet — never use `any` or manual type definitions; codegen must run first
3. **Hacky path**: Mid-implementation you realize the approach involves workarounds or quality compromises — discard and propose the clean path. No sunk cost fallacy. No "just make it work for now."

## Code Style — C#

| Rule | Detail |
|------|--------|
| Types | **Never use `var`** — always explicit types |
| Nullability | Nullable reference types enabled. No `null!` or `string.Empty` bypass (exception: test classes with good reason) |
| Null safety | Use `required` keyword or proper constructors to enforce nullability |
| File structure | One public type per file, file name matches type name |
| Return types | No tuples for public return types |
| Namespaces | Block-scoped namespaces and using statements |
| Async | Async all the way — never use `.Result()` |
| Naming | Microsoft C# conventions; prefer clear names over short ones |
| Tests | `MethodName_Scenario_ExpectedBehavior` |

## Code Style — TypeScript

- Reusable, small, focused components
- **Always use UI components from `components/ui/`** (`Button`, `Input`, `Select`, `Textarea`, `Modal`, `ConfirmationModal`, `IconButton`) — never raw `<button>`, `<input>`, etc. If the needed component doesn't exist, stop and ask
- **No hardcoded colors** — define once (CSS variables in `src/styles/`) and reference from there
- **No duplicate styled elements** — extract to a reusable component
- Mobile-friendly UI is a core requirement (see spec.md)
- Run `npm run codegen` after backend API changes

## Environment Variables

Defined in `Ordning.Server/EnvironmentVariables.cs` with strong typing via EasyReasy.EnvironmentVariables (validated at startup). All env vars must go through this file. Current vars: `JWT_SECRET`, `DATABASE_CONNECTION_STRING`.

Local dev config: copy `Ordning.Server/Properties/launchSettings_git.json` → `launchSettings.json`.

## Publishing

Cut a release by creating and pushing a tag starting with `v` (e.g. `v1.0.0`) — this triggers `.github/workflows/release.yml`:
```bash
git tag v1.0.0
git push origin v1.0.0
```
Deployment on the target host is handled by updaemon (config in `updaemon.json`).

## MCP Tools

Prefer these over alternative methods:

- **`glider`**: Roslyn-powered C# semantic analysis. **Must be loaded at conversation start** via `load` with `Ordning.sln` and `workingDirectory` set to repo root. If it stops working, run `dotnet tool update --global glider`.
- **`filemover`**: Move files and folders during refactoring.
- **`sr`**: Search-and-replace. Preferred for bulk refactoring and renaming.
- **`nugetdocs`**: NuGet package docs, readme files, and member search. Try this first for third-party library questions.
