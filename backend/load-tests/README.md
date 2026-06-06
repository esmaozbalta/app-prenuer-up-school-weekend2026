# Archi API — Load tests (Sprint 5)

[k6](https://k6.io/) scripts for PRD performance KPIs:

| Endpoint | KPI (p95) |
|----------|-----------|
| `GET /api/v1/search/omni` | &lt; 800 ms |
| `GET /api/v1/feed/global` | &lt; 300 ms |

## Prerequisites

- [k6](https://grafana.com/docs/k6/latest/set-up/install-k6/) installed
- API running locally or a deployed base URL

## Run against local API

```bash
cd backend
dotnet run --project Archi.Api

# another terminal
k6 run load-tests/search-feed.js
```

## Run against staging / production

```bash
k6 run -e BASE_URL=https://your-api.onrender.com load-tests/search-feed.js
```

Optional env vars:

| Variable | Default | Description |
|----------|---------|-------------|
| `BASE_URL` | `http://localhost:5000` | API root |
| `SEARCH_QUERY` | `matrix` | Omni-search query |
| `SEARCH_VUS` | `10` | Virtual users for search scenario |
| `FEED_VUS` | `15` | Virtual users for feed scenario |
| `DURATION` | `30s` | Scenario duration |

## Interpret results

- `search_omni_duration` and `feed_global_duration` custom trends map to PRD thresholds.
- Failed thresholds exit with non-zero code (CI-friendly).
- First search request may be slower (cache miss + cold start); run a warm-up or increase duration for production.

## NBomber (alternative)

For .NET-native load tests, add an `Archi.LoadTests` console project with NBomber scenarios mirroring the k6 script. k6 is preferred for CI and Render smoke runs because it needs no .NET build step.
