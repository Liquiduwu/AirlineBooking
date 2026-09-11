# Security

This is a portfolio project, not a production system - there's no bug bounty or CVE process
here. This document instead explains, OWASP API Security Top 10 style, what was considered and
where in the code it's addressed, since that reasoning is the actual point of the exercise.

## API1: Broken Object Level Authorization

Every booking/passenger endpoint requires an authenticated staff JWT (`[Authorize]` on every
controller in `src/AirlineBooking.Api/Controllers`). There's no passenger-facing "look up my
booking" endpoint that would need per-object ownership checks - all callers are internal staff
who are trusted to see any booking, which sidesteps this class of bug entirely rather than
solving it. A public passenger portal (listed as future work in the README) would need real
object-level checks (a passenger can only see *their own* booking) before it could ship.

## API2: Broken Authentication

- Passwords hashed with ASP.NET Core Identity's `PasswordHasher<T>` (PBKDF2, random salt,
  versioned output) - see `AirlineBooking.Infrastructure/Auth/AuthService.cs`.
- JWTs are short-lived (60 minutes, configurable via `Jwt:AccessTokenMinutes`) and signed with
  HMAC-SHA256. There is no refresh-token flow yet (documented as future work) - out of scope
  for the agent-facing use case this API targets, where re-login every hour is acceptable.
- Login failures return the same generic "Invalid username or password" message whether the
  username doesn't exist or the password is wrong, to avoid username enumeration.
- `/api/auth/login` has a dedicated, tighter rate-limit policy (5 requests/minute/IP) than the
  rest of the API (100/minute/IP) - see `Program.cs`'s `AddRateLimiter` call - to slow down
  credential-stuffing attempts.

## API3: Broken Object Property Level Authorization

DTOs are explicit, hand-shaped request/response records per endpoint (see
`Application/Features/*/*.Dtos.cs`) rather than serializing EF Core entities directly. This
means a field added to an entity for internal use (e.g. `PasswordHash`) can never leak through
an API response by accident - there is no response type that could carry it.

## API4: Unrestricted Resource Consumption

- Global rate limiting (100 req/min/IP) plus the tighter login-specific limit above, via
  ASP.NET Core's built-in rate limiter (no extra package - see `Program.cs`).
- No endpoint accepts an unbounded page size or `Take()` - list endpoints are intentionally
  small (a handful of aircraft/flights/passengers for a portfolio dataset). A real deployment
  would need pagination on `GET /api/bookings` and `GET /api/passengers` before the dataset
  grew past a few thousand rows; noted as future work.

## API5: Broken Function Level Authorization

Role-based authorization via `[Authorize(Roles = nameof(StaffRole.Admin))]` on every
Admin-only action (creating flights, aircraft, fare classes, registering new staff). Verified
with an integration-style manual test during development: an `Agent`-role token against an
Admin-only endpoint returns `403 Forbidden`, not `401` (the token is valid, the permission
isn't) or a silent 200.

## API6/API7: Unrestricted Access to Sensitive Business Flows / SSRF

Not applicable at this API's current scope - it makes no outbound HTTP calls to
caller-supplied URLs, and its only "sensitive flow" (booking a seat) is protected by the
seat-conflict logic described below plus standard auth.

## API8: Security Misconfiguration

- Secrets never committed: `appsettings.Development.json` (holds the local SQL Server
  connection string and JWT signing key) is gitignored; `appsettings.Development.json.example`
  documents the shape with placeholder values. See `.gitignore`.
- Generic 500 responses in production never include exception details or stack traces (see
  `GlobalExceptionHandler`) - only 4xx responses include a human-readable `Detail`, since those
  messages are meant to be actionable for the caller and don't leak internals.
- HSTS enabled outside Development (`app.UseHsts()`); skipped in dev because of localhost/
  self-signed-cert friction.
- `X-Content-Type-Options: nosniff`, `X-Frame-Options: DENY`, and `Referrer-Policy: no-referrer`
  set on every response (see `Program.cs`).
- Swagger UI is only mapped in Development (`app.Environment.IsDevelopment()` gate) - it isn't
  exposed on a production deployment by default.

## API9: Improper Inventory Management

Swagger/OpenAPI (`/swagger`) documents every endpoint with summaries, response types, and
example payloads generated from XML doc comments - see `Program.cs`'s `AddSwaggerGen` config.

## API10: Unsafe Consumption of APIs

The two places this API executes SQL beyond LINQ-to-Entities - `sp_GetSeatAvailability` and
`sp_CalculateFare` - are called via `FromSqlInterpolated`, which parameterizes every
interpolated value as a real `SqlParameter` rather than string-concatenating it into the
command text. See `Infrastructure/Repositories/SeatRepository.cs` and `FlightRepository.cs`.

## Concurrency-specific hardening (not an OWASP category, but load-bearing here)

Seat double-booking is the one correctness property this whole domain depends on. It's
enforced twice, deliberately:

1. **App-level check** (`IBookingRepository.IsSeatTakenAsync`) - gives a clean `409 Conflict`
   in the common case.
2. **SQL Server filtered unique index** on `(FlightId, SeatId) WHERE Status IN ('Held',
   'Confirmed')` (see `BookingConfiguration.cs`) - the actual race-condition guard. If two
   concurrent requests both pass the app-level check before either commits, only one insert
   satisfies the index; the loser's `DbUpdateException` is translated into the same
   `ConflictException` (see `AirlineBookingDbContext.SaveChangesAsync`).

## Dependency management

`.github/dependabot.yml` opens weekly PRs for NuGet package and GitHub Actions updates, with
minor/patch bumps grouped into one PR to reduce noise.

## Known gaps / explicitly out of scope for v1

- No refresh-token flow (see API2 above).
- No pagination on list endpoints (see API4 above).
- No centralized secrets manager (Azure Key Vault, etc.) - local dev secrets live in a
  gitignored `appsettings.Development.json`; a real deployment would pull the JWT signing key
  and connection string from Key Vault or App Service's connection-string settings, not from a
  committed file.
- No audit log of who confirmed/cancelled which booking - would matter in a real airline
  system for dispute resolution, out of scope here.
