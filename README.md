# MedConnect GH

Ghana's Unified Public Health Records Portal. See `docs/` for architecture,
data model, and design token references, and the full project proposal for
scope and rationale.

## Prerequisites

- .NET 10 SDK
- A local SQL Server instance. The dev connection string uses
  `Server=.\SQLEXPRESS` — change `ConnectionStrings:Default` in
  `MedConnect/appsettings.Development.json` if yours differs.

## Running locally

The client and API are split, so run two processes:

```
# Terminal 1 - Web API on http://localhost:5260
dotnet run --project MedConnect

# Terminal 2 - Blazor WebAssembly client dev server on http://localhost:5001
dotnet run --project MedConnect.Client
```

Then open `http://localhost:5001`. On first boot the API migrates + seeds the
database and creates a dev-only Admin account (`admin@medconnect.gh` /
`Admin@MedConnect2026`, from `AdminBootstrap:` in
`appsettings.Development.json`). Use it to create staff accounts under
**Staff Accounts**.

For a one-off build:

```
dotnet build MedConnect.sln
```

## Database

```
cd MedConnect
dotnet ef database update
```

Requires the `dotnet-ef` global tool: `dotnet tool install --global dotnet-ef`.

To add a new migration after changing an entity in `MedConnect/Domain/`:

```
cd MedConnect
dotnet ef migrations add <Name> -o Data/Migrations
```

## Tests

```
dotnet test MedConnect.Tests
```

Covers the pure-logic units (no live database required): Ghana phone
number normalization, RFC 6238 TOTP vectors, and the login rate limiter.

## Hosting (free tier: Cloudflare Pages frontend + Windows/IIS backend)

The split: `MedConnect.Client` is a static Blazor WebAssembly SPA hosted on
**Cloudflare Pages**; `MedConnect` is a Windows ASP.NET Core host (REST API +
SignalR + EF Core over SQL Server) deployed to a free Windows/IIS provider
such as MonsterASP — hosting the API on anything that can't run ASP.NET Core
natively won't work because it also needs SignalR.

See `.env.example` for the full list of settings — nested config sections use
a double underscore, e.g. `ConnectionStrings:Default` becomes
`ConnectionStrings__Default` as an environment variable.

### 1. Backend on free Windows/IIS hosting (e.g. MonsterASP)

1. Sign up at MonsterASP, create a **free .NET 10 site** (they give you a
   subdomain like `https://<site>.runasp.net`) and a **MS SQL Server
   database**; copy its connection string.
2. Publish the API:
   ```
   dotnet publish MedConnect -c Release -o out
   ```
   and FTP the contents of `out/` to your site's root.
3. Configure it on the server. The simplest place is the generated
   `web.config` — add an `<environmentVariables>` block inside `<aspNetCore>`:
   ```xml
   <aspNetCore processPath="dotnet" arguments=".\MedConnect.dll" hostingModel="inprocess">
     <environmentVariables>
       <environmentVariable name="ConnectionStrings__Default" value="<MonsterASP MSSQL connection string>" />
       <environmentVariable name="Jwt__Key" value="<real random secret, 32+ chars>" />
       <environmentVariable name="Jwt__Issuer" value="MedConnectGH" />
       <environmentVariable name="Jwt__Audience" value="MedConnectGH.Client" />
       <environmentVariable name="Cors__Origins" value="https://<your-project>.pages.dev" />
       <environmentVariable name="Sms__ApiKey" value="<Arkesel API key, optional>" />
       <environmentVariable name="Sms__SenderId" value="MEDCONNECT" />
     </environmentVariables>
   </aspNetCore>
   ```
   Note: production secrets are only ever read from these environment
   variables — nothing secret lives in the committed config files, and
   `appsettings.Development.json` (which contains only the local SQL Server
   connection string and a dev-only JWT key) is never published. Never reuse
   the dev-only JWT key.
4. On first boot the app applies pending EF Core migrations and seeds
   roles/divisions automatically (see `Program.cs`) — no manual
   `dotnet ef database update` needed against the hosted database.
5. Your API base URL is `https://<site>.runasp.net` (referred to below).

### 2. Frontend on Cloudflare Pages

1. Create the project once (browser or `wrangler`):
   ```
   npx wrangler pages project create medconnect
   ```
2. In GitHub, add to the repo's **Secrets**: `CLOUDFLARE_API_TOKEN` (API
   token with "Cloudflare Pages: Edit" permission) and `CLOUDFLARE_ACCOUNT_ID`.
3. Add a repository **Variable**: `API_BASE_URL` = your backend base URL from
   step 1.5, e.g. `https://<site>.runasp.net`.
4. Push to `main` — `.github/workflows/deploy-client.yml` publishes
   `MedConnect.Client`, bakes the API URL into `appsettings.json`, and uploads
   to Cloudflare Pages. The site serves at `https://<project>.pages.dev`
   (SPA deep links work via the checked-in `wwwroot/_redirects`).
5. Set `Cors__Origins` on the backend to `https://<project>.pages.dev`.

Local dev unchanged: API on `http://localhost:5260`, client dev server on
`http://localhost:5001` (see `.env.example` for `Cors__Origins`).

Free-tier gotchas: Windows/IIS hosts often recycle idle app pools (SignalR
reconnects afterwards) and free SQL databases may be wiped/reset periodically;
re-run the app after a wipe to re-migrate and re-seed.

## SMS notifications (Arkesel, optional)

Appointment confirmations, referral updates, and lab-result-ready notices are
sent to the patient's phone (their `ContactInfo`, which is validated and stored
as a +233 Ghana number at registration). Set `Sms:ApiKey` (env:
`Sms__ApiKey`) on the backend to enable real sends — without it every send is a
logged no-op, so local development needs no SMS account. `Sms:SenderId`
defaults to `MEDCONNECT`.

## Solution structure

```
MedConnect.sln
├── MedConnect/              # host + API
│   ├── Data/                 EF Core DbContext + migrations
│   ├── Domain/                entities + enums
│   ├── Features/              per-feature controllers/services/repositories
│   ├── Hubs/                  SignalR (ReferralHub)
│   └── wwwroot/css/           tokens.css (design system) + app.css
├── MedConnect.Client/       # Blazor WebAssembly
│   ├── Features/               per-feature pages/components
│   ├── Shared/                 reusable UI (Sidebar, Card, Button, ...)
│   └── Services/                typed HttpClient wrappers per feature
└── docs/
```
