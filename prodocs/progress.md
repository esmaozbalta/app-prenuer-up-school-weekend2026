# Archi Progress Log

## Approach We Are Taking
- API-first architecture with strict separation between `backend` (.NET 9 Web API) and `frontend` (Flutter).
- Backend is independently deployable and shared by Android + iOS clients.
- Work is executed story-by-story from the plan, with implementation + verification together.
- We prioritize a thin vertical slice first (Auth/Register), then extend to Login + session.

## What We Completed So Far
- Created repo structure:
  - `backend/Archi.Api`
  - `frontend/archi_app`
- Implemented US-01 Register flow:
  - `POST /api/v1/auth/register`
  - password hashing (BCrypt)
  - username/email uniqueness checks
  - JWT generation
- Added DB modeling + migration:
  - `User` entity
  - `AppDbContext`
  - `CreateUsersTable` migration
- Updated DB connection to Supabase Pooler and applied migration with `dotnet ef database update`.
- Implemented Flutter register UI + API integration.
- Added base quality checks:
  - Flutter analyze + widget tests
  - Backend integration tests for register
- Added basic anti-abuse protections for register:
  - standardized error response
  - temporary lockout guard on repeated failed attempts

## Failures/Issues Encountered and Resolution
- **Supabase migration timeout** on first `database update` call:
  - Resolved by retrying with higher timeout and non-pooled connection override.
- **Backend build lock** due to running process holding `Archi.Api.exe`:
  - Resolved by stopping the locking process and rerunning tests/build.
- **Integration test instability** (DbContext override + in-memory isolation):
  - Resolved by replacing service registrations correctly and using one shared in-memory DB name per test factory.

## US-02 Execution Update
- Added backend login endpoint:
  - `POST /api/v1/auth/login` (email + password verification)
  - returns JWT on success
  - returns `401` on invalid credentials
- Added backend login integration tests:
  - valid credential -> `200` with token
  - invalid password -> `401`
- Added frontend login + session flow:
  - mode switch between Register and Login
  - secure token persistence via `flutter_secure_storage`
  - session restore on app startup
  - logout clears stored token

## Current Failure We Are Working On
- No active blocker right now.
- Latest transient failure was Flutter widget-test timeout during session bootstrap; resolved by timeout fallback in secure storage read path.
