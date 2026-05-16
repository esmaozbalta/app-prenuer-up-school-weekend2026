# Auth strategy (Sprint 1 decision)

**Decision:** Keep **email/password + JWT** for MVP (current implementation). Firebase alignment is deferred to Phase 2.

## Rationale

- Flutter auth screen and `AuthApi` already target `POST /api/v1/auth/register` and `POST /api/v1/auth/login`.
- User CRUD, profile, and archive endpoints use `Authorization: Bearer` with `sub` = user `Id`.
- PRD mentions Firebase; `users.OauthId` is nullable and ready for a future link step.

## Current contract

| Flow | Endpoint | Token |
|------|----------|--------|
| Register | `POST /api/v1/auth/register` | JWT in response body |
| Login | `POST /api/v1/auth/login` | JWT in response body |
| Protected APIs | Any `[Authorize]` route | `Authorization: Bearer {token}` |

Claims: `sub` (user id), `email`, `username`.

## PRD gap (US-101)

| PRD | MVP | Phase 2 |
|-----|-----|---------|
| Google / Apple via Firebase | Email + password only | `POST /api/v1/auth/firebase-exchange` |
| `oauth_id` required | Optional, unused | Set on Firebase link |
| Firebase ID token validation | HMAC JWT validation | Firebase Admin / JWKS |

## Phase 2 sketch: `POST /api/v1/auth/firebase-exchange`

1. Validate Firebase ID token.
2. Find user by `oauth_id` or create/link by email.
3. Issue same Archi JWT as login (unchanged mobile header format).

## Flutter

Continue sending the Archi JWT from login/register on protected routes. No Firebase SDK change required until Phase 2.
