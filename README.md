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
dotnet user-secrets set "Email:ApiKey" "<resend-or-smtp-key>"
```

In production, set the same keys as environment variables (`ConnectionStrings__Default`,
`Email__ApiKey`, …). `appsettings.json` holds only non-secret defaults.

Settings that must exist (document every new one here):

| Key | Purpose |
|---|---|
| `ConnectionStrings:Default` | SQL Server connection string; falls back to SQLite when empty |
| `Email:Provider` | `noop` locally, `resend` or `smtp` in production |
| `Email:ApiKey` | Provider API key |
| `Email:FromAddress` | e.g. `hi@bhugol.dev` |
| `Email:ToAddress` | where lead notifications go |
| `Admin:Username` / `Admin:PasswordHash` | access to `/admin/leads` |
| `Site:BaseUrl` | canonical URL, used for absolute links and sitemap |

## Database

```powershell
dotnet ef migrations add <Name> --project src/BhugolLab.Web
dotnet ef database update --project src/BhugolLab.Web
```

Migrations are applied explicitly. `EnsureCreated()` is not used.

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
