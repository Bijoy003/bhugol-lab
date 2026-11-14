# Bhugol Lab — bhugol.dev

Marketing and lead-generation site for **Bhugol Lab**, a solo software consultancy.
The site's job is to turn visitors into inquiries: marketing pages, case studies,
a blog, and a contact form.

Production URL: https://bhugol.dev

## Stack

| Concern | Choice |
|---|---|
| Runtime | .NET 10 (LTS) |
| Web | ASP.NET Core **Razor Pages** (single project) |
| Content | Markdown in `Content/posts`, rendered with Markdig |
| Data | EF Core + SQL Server (SQLite fallback for local dev) |
| Hosting | Docker image; Azure App Service or any container host |
| DNS / CDN | Cloudflare |

Deliberately **one** web project. No separate API, no Functions, no SPA framework.
See `docs/IMPLEMENTATION-PROMPT.md` for the full specification.

## Prerequisites

- .NET 10 SDK (`dotnet --version` → 10.0.400 or later)
- Git
- Optional: SQL Server / LocalDB, or Docker
- Optional: EF Core CLI — `dotnet tool install --global dotnet-ef`

The SDK version is pinned in `global.json`. If you have a newer SDK only, adjust
`version` and keep `rollForward: latestFeature`.

## Repository layout

```
bhugol-lab/
  BhugolLab.slnx
  Directory.Build.props      shared compiler settings (warnings are errors)
  global.json                pinned SDK
  .editorconfig
  docs/                      spec, deployment and DNS notes
  src/
    BhugolLab.Web/           the only web app
      Interfaces/            ports: IEmailSender, ILeadRepository
      Services/              application services: MarkdownService, Notifications/
      Infrastructure/        adapters & helpers: Email/, Security/, Seo/
      Data/                  AppDbContext, Entities/, Repositories/, Migrations/
      Models/ Options/ Middleware/ Pages/
  tests/
    BhugolLab.Web.Tests/     xUnit + WebApplicationFactory
```

## Getting started

```powershell
dotnet restore
dotnet build
dotnet test
dotnet run --project src/BhugolLab.Web
```

The app starts on the URLs in `src/BhugolLab.Web/Properties/launchSettings.json`
(typically https://localhost:7xxx and http://localhost:5xxx).

## Configuration and secrets

Never commit secrets. Locally:

```powershell
cd src/BhugolLab.Web
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Server=(localdb)\MSSQLLocalDB;Database=BhugolLab;Trusted_Connection=True;"
dotnet user-secrets set "Email:Smtp:Username" "<smtp-username>"
dotnet user-secrets set "Email:Smtp:Password" "<smtp-password>"
```

In production, set the same keys as environment variables (`ConnectionStrings__Default`,
`Email__ApiKey`, …). `appsettings.json` holds only non-secret defaults.

Settings that must exist (document every new one here):

| Key | Purpose |
|---|---|
| `ConnectionStrings:Default` | SQL Server connection string; empty → SQLite file under `App_Data/`; contains `Data Source` → SQLite with that string |
| `Email:Provider` | `noop` locally, `smtp` or `resend` in production |
| `Email:Smtp:Host` / `Port` | SMTP host and port (`587` with STARTTLS is the usual default) |
| `Email:Smtp:Username` | SMTP username for the configured provider |
| `Email:Smtp:Password` | SMTP credential — secret, never committed |
| `Email:FromAddress` | sender address for lead notifications |
| `Email:ToAddress` | where lead notifications land; also the public contact address |
| `Admin:Username` / `Admin:PasswordHash` | access to `/admin/leads` |
| `Site:BaseUrl` | canonical URL, used for absolute links and sitemap |
| `Site:AuthorName` | consultant display name (footer, hero, JSON-LD) |
| `Site:GitHubUrl` / `Site:LinkedInUrl` | public profile links (footer) |
| `Site:BookingUrl` | Cal.com / Calendly link for "Book a call" CTAs |
| `Site:Location` | city and timezone shown in the footer |

## Database

```powershell
dotnet ef migrations add <Name> --project src/BhugolLab.Web --output-dir Data/Migrations
dotnet ef database update --project src/BhugolLab.Web
```

Migrations are applied explicitly at startup (`Database.Migrate()`); `EnsureCreated()`
is not used. Migrations are scaffolded against SQL Server (see
`Data/DesignTimeDbContextFactory`) and are shaped to apply cleanly on SQLite too
(Guid key, explicit max lengths), so the local fallback needs no extra setup.
On SQLite the `PendingModelChangesWarning` is suppressed by design — the snapshot is
SQL Server's; forgotten model changes still surface on SQL Server and via
`dotnet ef migrations has-pending-model-changes`.

## Admin access

`/admin/leads` uses cookie authentication with a single user from configuration.
Generate the password hash:

```powershell
dotnet run --project src/BhugolLab.Web -- hash-password "<password>"
```

Store the result in `Admin:PasswordHash` (user-secrets locally, environment variables
in production). While unconfigured, every login attempt fails and is logged.

## Rate limits and caching

- Contact form POST: 3 requests / 10 min per client IP. Admin login POST: 5 / 10 min per IP. Global limiter: 120 / min per IP.
- Anonymous GETs of content pages are output-cached for 5 minutes. Pages with forms
  (`/contact`, `/hire`), `/admin/*` and `/contact/thanks` are never cached, because
  form pages embed per-visitor antiforgery tokens.
- The app runs behind Cloudflare: the real client IP is taken from `CF-Connecting-IP`
  (set and overwritten at the edge), so the origin must accept traffic from
  Cloudflare only — see `docs/deployment.md`.

## Adding a blog post

1. Create `src/BhugolLab.Web/Content/posts/<slug>.md`.
2. Add front matter:

```yaml
---
title: "Post title"
description: "One-sentence summary used for meta and OG tags."
date: 2026-11-14
tags: [dotnet, consulting]
draft: false
---
```

3. Write the body in Markdown. The slug comes from the file name.
4. `dotnet run` and check `/blog/<slug>`.

## Deployment

See `docs/deployment.md` for host settings, environment variables, and the DNS /
email records to create in Cloudflare. The image is built from the repo `Dockerfile`
and listens on port 8080.

## Before you start building

The app was scaffolded from `dotnet new razor`, so it still contains the template's
Bootstrap and jQuery assets under `wwwroot/lib` and a placeholder homepage. Per the
specification these should be replaced with hand-written CSS and near-zero JS
(target: < 50 KB total JS, Lighthouse ≥ 95). Do that as the first feature commit,
not as an afterthought.
