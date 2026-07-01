# CLAUDE.md — Meridian Project

Instructions for working in this repository. Read before every task.

---

## Commits

- **Never** add `Co-Authored-By` trailers, `Generated with Claude Code`, or any
  attribution line to commit messages. Commit under the repository owner's
  identity only.
- Work in **small, incremental commits** — never one large commit at the end.
- Use **Conventional Commits**: `feat:`, `fix:`, `refactor:`, `test:`, `docs:`,
  `chore:`, `ci:`.
- One logical change per commit. Each commit should build and pass tests.

---

## Stack (fixed — do not substitute)

- **Backend:** .NET (latest LTS), C#
- **ORM:** EF Core — pure EF, **no hand-written SQL files**
- **Database:** MSSQL
- **Schema + seed:** EF migrations for schema, `HasData` for seed
  (idempotent by key — re-running must never duplicate rows)
- **Frontend:** Angular + TypeScript (built later)
- **Logging:** Serilog (structured)
- **Validation:** FluentValidation
- **Mapping:** Mapster
- **DI / decoration:** Scrutor

---

## Solution structure

```
Meridian.Domain   → entities, repository INTERFACES, domain rules. Depends on NOTHING.
Meridian.Dal      → EF Core, DbContext, migrations, repository IMPLEMENTATIONS,
                    caching decorators. References Domain only.
Meridian.Bll      → business logic, services, validators, query pipeline,
                    DTOs + Mapster config. References Domain only.
Meridian.Api      → controllers, middleware, DI composition root, Swagger.
                    References Bll + Domain. May reference Dal ONLY in Program.cs
                    for DI registration (composition root).
```

Test projects:
```
Meridian.UnitTests          → BLL logic, mock repository interfaces.
Meridian.ArchitectureTests  → NetArchTest rules below.
```

---

## Dependency rules (enforced by ArchitectureTests)

- `Meridian.Domain` depends on **nothing** (no EF Core, no other project).
- `Meridian.Bll` **must not** reference `Meridian.Dal` or `EF Core`.
- `Meridian.Api` **must not** reference `Meridian.Dal` **except** in the DI
  composition root (`Program.cs`).
- Repository interfaces live in **Domain**; implementations live in **Dal**.
- Dependencies always point **inward** (toward Domain).

---

## Layering conventions

- **Controllers stay thin.** No business logic, no validation logic, no mapping
  in controllers — they call BLL services and return results.
- Business logic lives in **BLL** only.
- **Repository pattern, no Unit of Work.** `DbContext` is the unit of work;
  save directly via `SaveChangesAsync`. Do **not** add an `IUnitOfWork`.
- **Generic repository base + specific repositories** only where a specific one
  earns its place (extra queries, caching seam). No empty ceremony repositories.
- **Query pipeline:** composable steps for filtering, paging, sorting.
  - `IQueryStep<T>` and the concrete steps live in **BLL** and operate on
    `IQueryable<T>` using **only standard LINQ operators** (`Where`, `OrderBy`,
    `Skip`, `Take`). These are `System.Linq` (BCL) — **not** EF Core — so BLL
    stays free of any EF reference. Do **not** use `Include`, `AsNoTracking`,
    or any `Microsoft.EntityFrameworkCore` API inside a pipeline step.
  - **Dal** runs the pipeline against `DbSet<T>` and then **materializes** with
    `ToListAsync`/`FirstOrDefaultAsync`. EF-specific calls (`Include`,
    `AsNoTracking`, async materializers) live here only.
  - The repository **never returns `IQueryable` to the outside** — it accepts
    steps/spec, applies them, and returns materialized results. `IQueryable`
    never escapes Dal.
- **Caching:** decorator pattern (Scrutor) wrapping repository interfaces,
  applied **only** to hot, rarely-changing data (HR onboarding template).
- **Error handling:** one catch-all middleware → returns **ProblemDetails
  (RFC 7807)**. Include the **correlation id** in every error response.
- **Correlation id:** generate/propagate per request, enrich Serilog logs with it.
- Expose **DTOs**, never entities, across the API boundary (map with Mapster).
- **Auth:** JWT, **policy-based** (not just `[Authorize(Roles=...)]`).
  Read-only restrictions must be enforced at the authorization layer on
  write endpoints — never only hidden in the UI.

---

## Decisions already made — do NOT reintroduce

- **No `IUnitOfWork`** — `DbContext` already provides it.
- **No hand-written SQL files** (`IF NOT EXISTS` scripts) — use EF `HasData`.
- **No NgRx** on the frontend — Angular signals cover state at this scope.
- **No push notifications**, **no auto video-completion tracking** — these are
  documented as future work in `WHAT_I_WOULD_DO_NEXT.md`, not built.

---

## Quality gates

- Provide `.editorconfig`; treat warnings as errors where reasonable.
- **Swagger/OpenAPI** with JWT support enabled.
- **Health check** endpoint.
- **docker compose** that brings up MSSQL + API, applies migrations, and seeds —
  `docker compose up` must work from a clean checkout.
- **GitHub Actions**: build + test on every push.
- Keep a **walking skeleton green first** (one thin slice through all layers +
  compose + CI + one test), then thicken feature by feature.

---

## Language

All code, comments, identifiers, commit messages, and documentation in **English**.