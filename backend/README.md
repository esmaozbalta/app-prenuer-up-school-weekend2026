# Archi Backend

.NET 9 minimal API — auth, omni-search, archive, global feed, sync, share cards.

## Quick start

```bash
cd backend
cp .env.example .env          # set JWT + DB connection
dotnet run --project Archi.Api
```

Swagger UI: http://localhost:5000/

## Tests

```bash
dotnet test Archi.Api.Tests/Archi.Api.Tests.csproj
```

## Docker (API + Redis)

```bash
docker compose up --build
```

→ http://localhost:8080

## Deploy (Render)

See [docs/deploy-render.md](docs/deploy-render.md) and root [`render.yaml`](../render.yaml).

## Load tests (Sprint 5)

[k6](https://k6.io/) scripts in [load-tests/](load-tests/README.md) — PRD KPIs: search p95 &lt; 800 ms, feed p95 &lt; 300 ms.

## API collection

Import [docs/postman/Archi-API.postman_collection.json](docs/postman/Archi-API.postman_collection.json) into Postman or Bruno.

## Database migrations

```bash
cd Archi.Api
dotnet ef database update
```

Production process: [docs/supabase-prod-migration.md](docs/supabase-prod-migration.md).

## Environment variables

See [.env.example](.env.example). Production secrets go in the host (Render env vars), not in committed config files.

## Project layout

```
Archi.Api/           Web API
Archi.Api.Tests/     xUnit integration + unit tests
load-tests/          k6 performance scripts
docs/                Deploy, migration, Postman
```
