# Render deployment — Archi API

## Overview

| Item | Value |
|------|--------|
| Runtime | Docker (.NET 9) |
| Blueprint | [`render.yaml`](../../render.yaml) at repo root |
| Health check | `GET /api/v1/health` |
| Swagger UI | `/` (root) |

## First deploy

1. Push this repo to GitHub.
2. [Render Dashboard](https://dashboard.render.com/) → **New** → **Blueprint** → select the repo.
3. Render reads `render.yaml` and creates the `archi-api` web service.
4. Set **secret** env vars in the Render service (marked `sync: false` in blueprint):
   - `ConnectionStrings__DefaultConnection` — Supabase pooler URI (see [supabase-prod-migration.md](./supabase-prod-migration.md))
   - `Cache__Redis__ConnectionString` — Upstash or Render Redis URL
   - `Tmdb__ApiKey`, `GoogleBooks__ApiKey`, `Igdb__ClientId`, `Igdb__ClientSecret`, `Steam__ApiKey`
5. Deploy. Note the public URL (e.g. `https://archi-api.onrender.com`).

## Local Docker

```bash
cd backend
cp .env.example .env   # fill ConnectionStrings__DefaultConnection + Jwt__SigningKey
docker compose up --build
```

API: `http://localhost:8080` — Swagger at `/`.

## Environment variables

ASP.NET Core nested config uses double underscore:

| Variable | Required | Notes |
|----------|----------|-------|
| `ConnectionStrings__DefaultConnection` | Yes | Supabase PostgreSQL pooler |
| `Jwt__SigningKey` | Yes | Min 32 chars; Render can auto-generate |
| `Jwt__Issuer` / `Jwt__Audience` | Yes | Defaults in blueprint |
| `Cache__Redis__Enabled` | Prod: `true` | Search/feed KPI |
| `Cache__Redis__ConnectionString` | If Redis enabled | |
| `ExternalApi__UseStubs` | Prod: `false` | `true` for CI/local without keys |
| `Sync__UseStubs` | Prod: `false` | |
| API keys | Prod | TMDB, Books, IGDB, Steam |

Full template: [`backend/.env.example`](../.env.example).

## Cold start (free tier)

Render free web services spin down after ~15 minutes idle. Mitigations:

1. **GitHub Actions keep-alive** — set repo secret `ARCHI_API_URL` to your Render URL; workflow [`.github/workflows/keep-alive.yml`](../../.github/workflows/keep-alive.yml) pings `/api/v1/health` every 10 minutes.
2. **External monitor** — UptimeRobot / cron-job.org on the health URL (same interval).
3. **Paid plan** — always-on instance, no cold start.

## Post-deploy checks

```bash
curl -s https://YOUR-SERVICE.onrender.com/api/v1/health
curl -s "https://YOUR-SERVICE.onrender.com/api/v1/search/omni?q=matrix"
```

Load test (k6): see [`backend/load-tests/README.md`](../load-tests/README.md).

## CI

[`.github/workflows/backend-ci.yml`](../../.github/workflows/backend-ci.yml) runs `dotnet test` and verifies the Docker image builds on backend changes.
