# Supabase production migration strategy

**Goal:** Apply EF Core migrations to the production Supabase PostgreSQL instance safely, with a single source of truth and rollback plan.

## Principles

1. **EF migrations are canonical** — `backend/Archi.Api/Migrations/` matches application code; avoid hand-editing prod schema outside migrations.
2. **Dev → staging → prod** — never run untested migrations directly on prod.
3. **Backup before apply** — Supabase Dashboard → Database → Backups (Pro) or manual `pg_dump` before major releases.
4. **Pooler for runtime, direct for migrate** — App uses **transaction pooler** (port 5432); `dotnet ef database update` may need **session mode** or **direct** connection (port 5432 session / 6543) per Supabase docs for your project.

## Migration inventory (MVP)

| Migration | Tables / changes |
|-----------|------------------|
| `20260424211820_CreateUsersTable` | `users` |
| `20260516005007_AddUserPrdFields` | PRD fields on `users` |
| `20260516121729_AddArchiveAndVibeTags` | `archive_items`, `vibe_tags`, indexes |

Verify pending migrations:

```bash
cd backend/Archi.Api
dotnet ef migrations list
```

## Pre-production checklist

- [ ] All migrations applied on **dev** Supabase; API smoke tests pass.
- [ ] Staging DB (optional separate Supabase project) receives same migration set.
- [ ] `ConnectionStrings__DefaultConnection` in Render points at **prod** pooler URI.
- [ ] No destructive migration without data migration script (drops, column type changes).
- [ ] Redis and API keys configured for prod (stubs disabled).

## Apply to production

### Option A — CI/CD job (recommended)

Add a manual workflow or Render pre-deploy hook:

```bash
export ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require;"
cd backend/Archi.Api
dotnet ef database update --connection "$ConnectionStrings__DefaultConnection"
```

Run once per release **before** or **during** deploy when new migrations exist. Do not run on every container start (race conditions, slow boot).

### Option B — Local one-shot (MVP)

From a trusted machine with prod credentials:

```bash
cd backend/Archi.Api
dotnet ef database update \
  --connection "Host=YOUR-POOLER;Port=5432;Database=postgres;Username=postgres.PROJECT_REF;Password=SECRET;SSL Mode=Require;"
```

Confirm with:

```sql
SELECT "MigrationId" FROM "__EFMigrationsHistory" ORDER BY "MigrationId";
```

### Option C — Supabase SQL editor

Generate idempotent SQL from EF (review carefully):

```bash
dotnet ef migrations script --idempotent -o migration.sql
```

Paste into Supabase SQL Editor only after peer review. Prefer Option A/B to avoid drift.

## Rollback

EF Core has no automatic down-migration in production. Options:

1. **Forward fix** — new migration correcting the issue (preferred).
2. **Restore backup** — Supabase point-in-time restore (Pro).
3. **`dotnet ef database update PreviousMigrationName`** — only if no incompatible data was written.

Document each prod apply in release notes (migration id + timestamp).

## Environment separation

| Environment | Supabase project | Connection string |
|-------------|------------------|-------------------|
| Local dev | Dev project | `.env` / user secrets |
| CI tests | InMemory / test DB | `appsettings.Testing.json` |
| Production | Prod project | Render env `ConnectionStrings__DefaultConnection` |

Never commit real prod passwords. Use Render/Supabase secret stores.

## Post-migration validation

```bash
curl https://YOUR-API/api/v1/health
# Auth + archive smoke (Postman collection)
# backend/docs/postman/Archi-API.postman_collection.json
```

Run integration tests locally against a clone DB when possible:

```bash
cd backend
dotnet test Archi.Api.Tests/Archi.Api.Tests.csproj
```

## Future: automated pipeline

When team grows, add:

- Separate Supabase project for staging
- GitHub Action `workflow_dispatch` migration job with approval gate
- Migration diff in PR checks (`dotnet ef migrations has-pending-model-changes`)
