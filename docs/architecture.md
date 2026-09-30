# Architecture

## Solution layout

Two projects, no separate `Shared` library — DTOs live inside `MedConnect`'s
`Features/<X>/` folders, and `MedConnect.Client` defines its own matching
request/response models in `Services/`.

- **MedConnect** (host) — ASP.NET Core Web API. Serves the Blazor host page,
  exposes REST endpoints per feature, owns the EF Core `ApplicationDbContext`,
  ASP.NET Core Identity, JWT issuance, and the `ReferralHub` SignalR hub.
- **MedConnect.Client** — Blazor WebAssembly. Runs entirely in the browser,
  calls the host's REST endpoints over HTTPS, and holds a SignalR connection
  to `/hubs/referrals` for live referral status updates.

Each `Features/<X>` folder owns its controller, service, and repository (host)
or pages/components (client) together — not split across horizontal
Controllers/Services/Repositories folders.

## Auth flow

1. Client posts credentials to `/api/v1/auth/login` (or `/register`).
2. API validates via ASP.NET Core Identity, issues a JWT access token with
   role and `FacilityId` claims embedded (no refresh token in this build —
   access tokens are short-lived per `Jwt:AccessTokenMinutes`; re-login on
   expiry).
3. Client stores the token in `localStorage` (`TokenStorage`), and
   `AuthorizationMessageHandler` attaches it as a Bearer header to every
   outgoing API call and the SignalR handshake. `CustomAuthStateProvider`
   parses the token's claims client-side to drive `AuthorizeView`.
4. `[Authorize(Roles = "...")]` enforces the four-role matrix
   (Patient, CHW, Clinician, Admin) server-side on every endpoint.

Because the token lives in `localStorage` rather than a cookie, the server
has no way to know a request's auth state on the very first plain-HTML page
load — so page-level route protection is deliberately **not** done via
`@attribute [Authorize]` (that attribute also registers as ASP.NET Core
endpoint metadata and triggers a hard 401 at the HTTP layer before the SPA
can boot). Instead, every protected page wraps its content in
`<AuthorizeView>` and redirects via a client-side `RedirectToLogin`
component in the `NotAuthorized` fragment, resolved entirely after WASM
loads. Relatedly, prerendering is disabled for the WebAssembly render mode
(`App.razor`) — prerendering runs server-side, and this project's auth
provider only exists in the Client project's own DI container.

## Real-time referrals

`ReferralHub` groups connections by `FacilityId` on connect
(`ReferralHub.FacilityGroup`). When a referral's status changes, the
Referrals feature service pushes to the relevant facility group via
`IHubContext<ReferralHub>` — no polling on the client.

## Scope note

Offline-first CHW data entry (IndexedDB caching + background sync queue) was
scoped out of this build. The CHW field visit form submits directly to the
API; it requires connectivity to work, same as the rest of the app.

## Pipeline gotcha: status-code-page re-execution vs. the REST API

`UseStatusCodePagesWithReExecute("/not-found", ...)` re-executes *any*
non-2xx response against the Blazor `/not-found` page by default —
including a 401/403 from the REST API. That re-executed request hits a
Razor Component endpoint requiring antiforgery validation, which rejects
the original JSON body and returns a confusing 400 "incorrect Content-type"
in place of the real status. Fixed in `Program.cs` by scoping that
middleware to non-`/api` requests via `UseWhen`, so the API keeps clean
REST semantics (401/403/404) while the SPA's not-found page still works for
browser navigation.

## Current status

All 7 backend features (Identity, Facilities, Patients, Visits,
Appointments, Referrals, FieldVisits) and their corresponding client pages
are implemented: Login/Register, role-aware Dashboard, Patient My Record,
staff Patients list + walk-in registration, Referral Tracker with live
SignalR updates, CHW Field Visit form, and Admin Facilities management.
Verified end-to-end against a real LocalDB instance via HTTP; not yet
verified via an actual browser (no browser automation available in the
environment this was built in) — worth a manual pass before treating the
UI as done.
