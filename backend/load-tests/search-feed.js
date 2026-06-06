import http from 'k6/http';
import { check, sleep } from 'k6';
import { Trend } from 'k6/metrics';

const baseUrl = __ENV.BASE_URL || 'http://localhost:5000';
const searchQuery = __ENV.SEARCH_QUERY || 'matrix';

const searchDuration = new Trend('search_omni_duration', true);
const feedDuration = new Trend('feed_global_duration', true);

export const options = {
  scenarios: {
    search: {
      executor: 'constant-vus',
      exec: 'searchOmni',
      vus: Number(__ENV.SEARCH_VUS || 10),
      duration: __ENV.DURATION || '30s',
    },
    feed: {
      executor: 'constant-vus',
      exec: 'globalFeed',
      vus: Number(__ENV.FEED_VUS || 15),
      duration: __ENV.DURATION || '30s',
      startTime: '5s',
    },
  },
  thresholds: {
    // PRD KPI: search < 800 ms, feed < 300 ms (p95)
    search_omni_duration: ['p(95)<800'],
    feed_global_duration: ['p(95)<300'],
    http_req_failed: ['rate<0.05'],
  },
};

export function searchOmni() {
  const res = http.get(`${baseUrl}/api/v1/search/omni?q=${encodeURIComponent(searchQuery)}`);
  searchDuration.add(res.timings.duration);

  check(res, {
    'search status 200': (r) => r.status === 200,
    'search has movies or books or games': (r) => {
      try {
        const body = r.json();
        return (
          (body.movies && body.movies.length >= 0) ||
          (body.books && body.books.length >= 0) ||
          (body.games && body.games.length >= 0)
        );
      } catch {
        return false;
      }
    },
  });

  sleep(0.3);
}

export function globalFeed() {
  const res = http.get(`${baseUrl}/api/v1/feed/global?limit=20`);
  feedDuration.add(res.timings.duration);

  check(res, {
    'feed status 200': (r) => r.status === 200,
    'feed has items array': (r) => {
      try {
        const body = r.json();
        return Array.isArray(body.items);
      } catch {
        return false;
      }
    },
  });

  sleep(0.2);
}

export function setup() {
  const health = http.get(`${baseUrl}/api/v1/health`);
  if (health.status !== 200) {
    throw new Error(`Health check failed (${health.status}). Is the API running at ${baseUrl}?`);
  }
}
