# Website Implementation Prompt

Paste everything inside the "Prompt" section into a new chat. Fill in the
`<PLACEHOLDER>` values first (at minimum: niche/ICP, package prices, booking link,
GitHub/LinkedIn handles, hosting target). Brand, naming, domain, and stack are
already decided — don't re-litigate them.

---

## Prompt

You are a senior .NET engineer. Build a production-ready marketing/lead-generation website for a solo software consultant. Work autonomously, but ask me once for any `<PLACEHOLDER>` value you need before writing code, then proceed without further questions unless something is genuinely blocking.

### Objective
A single, fast, credible site whose only business job is converting visitors into inquiries. Marketing content + a contact form + a blog. Nothing else.

### Hard constraints (do not change these)
- Stack: ONE ASP.NET Core Razor Pages project on .NET 10 (LTS). Razor Pages, not MVC, not Blazor.
- No separate API project, no Azure Functions, no client-side SPA framework.
- Markdown files in git for blog content, rendered with Markdig.
- EF Core + SQL Server provider (database only for leads; blog stays in Markdown).
- Dockerfile so the app is host-agnostic; deployable to Azure App Service OR any Docker host.
- Canonical domain: bhugol.dev (apex). `www.bhugol.dev` must 301-redirect to the apex. All marketing/lead pages must live on top-level paths, never subdomains.
- No fabricated content: do not invent testimonials, client names, metrics, or case-study results. Where real data is missing, insert clearly marked `TODO:` placeholders.
- Keep it minimal. Six to eight pages. No CMS, no admin panel beyond a simple protected leads list, no microservices, no message queue, no Redis, no Kubernetes.

### Placeholders (replace everywhere they appear)
- Consultant display name: Bhugol <LASTNAME> (brand: Bhugol Lab)
- Contact email: <PERSONAL EMAIL, e.g. yourname@gmail.com> (no custom-domain mailbox at launch)
- Lead notifications: send to the same personal email
- Booking link: <CAL.COM OR CALENDLY URL>
- GitHub: github.com/<HANDLE>
- LinkedIn: linkedin.com/in/<HANDLE>
- Positioning line: <"I help [CLIENT TYPE] achieve [OUTCOME] by building [THING]">
- Primary niche / ICP: <NICHE>
- Service packages and price ranges: <PACKAGE 1 + RANGE>, <PACKAGE 2 + RANGE>
- City / timezone: <CITY>, <TZ>
- Hosting target: <Azure App Service | Fly.io | other>

### Step 1 — Repository and git (do this first)
1. `git init -b main` in the project root.
2. Create a `.gitignore` using `dotnet new gitignore`; additionally ignore `.env`, `*.db`, `*.db-shm`, `*.db-wal`, `appsettings.Development.local.json`, and IDE folders.
3. Create `.editorconfig` (`dotnet new editorconfig`).
4. Add a `README.md` containing: what the site is, prerequisites (.NET 10 SDK, EF tools), local setup steps, how to run, how to add a blog post, how to apply migrations, deployment steps, and where secrets live.
5. Commit in small Conventional Commits (`feat:`, `fix:`, `chore:`, `docs:`, `test:`). Suggested sequence: `chore: initialize repository` -> `feat: base layout and homepage` -> `feat: services and hire pages` -> `feat: work/case study pages` -> `feat: blog with markdown content` -> `feat: contact form with persistence and email` -> `feat: seo, sitemap, structured data` -> `test: smoke tests` -> `chore: docker and ci`.
6. Create the remote with GitHub CLI if available: `gh repo create bhugol-lab --private --source=. --push`. If `gh` is unavailable, print the exact `git remote add origin ...` and `git push -u origin main` commands for me to run, and stop there.
7. Never commit secrets, connection strings, API keys, or certificates. Use `dotnet user-secrets` locally and environment variables in production. Commit an `appsettings.Example.json` and `.env.example` with empty values only.
8. Add a `docs/` note listing the DNS records I must create manually (A/CNAME, `www` redirect) and the optional email records for later. Do not assume a custom-domain mailbox exists.

### Step 2 — Naming, layout, and project structure

Use these names exactly:

- GitHub repo: `bhugol-lab`
- Solution: `BhugolLab` (`BhugolLab.slnx` or `.sln`)
- Project: `BhugolLab.Web`
- Root namespace: `BhugolLab.Web`
- Test project: `BhugolLab.Web.Tests`

Layout, created from day one so a second project can be added later without restructuring:

```
bhugol-lab/
  BhugolLab.slnx
  .gitignore
  .editorconfig
  README.md
  src/
    BhugolLab.Web/
  tests/
    BhugolLab.Web.Tests/
  docs/
```

Scaffold commands:

```
dotnet new sln -n BhugolLab
dotnet new razor -n BhugolLab.Web -o src/BhugolLab.Web -f net10.0
dotnet new xunit -n BhugolLab.Web.Tests -o tests/BhugolLab.Web.Tests -f net10.0
dotnet sln add src/BhugolLab.Web tests/BhugolLab.Web.Tests
dotnet add tests/BhugolLab.Web.Tests reference src/BhugolLab.Web
```

Create a single web project, `BhugolLab.Web`, with this shape:

- `Program.cs`
- `Pages/` — `Index`, `Hire`, `Services`, `Contact`, `ContactThanks`, `Privacy`, `NotFound`, plus `Work/Index`, `Work/Detail`, `Blog/Index`, `Blog/Post`, `Blog/Tag`, `Admin/Leads`
- `Content/posts/*.md` — blog source with YAML front matter (title, description, date, tags, draft)
- `Services/` — `MarkdownService`, `IEmailSender` + `EmailSender` (Resend or SMTP), `ILeadStore` + `EfLeadStore`
- `Data/` — `AppDbContext`, `Lead` entity, migrations
- `wwwroot/` — CSS, fonts, images, favicon, OG images
- `Dockerfile`, `.dockerignore`
- `.github/workflows/ci.yml`

### Step 3 — Pages and content

Brand usage: the header wordmark is **Bhugol Lab**; the homepage hero and footer name the person ("Bhugol <LASTNAME>") so search and trust attach to a human, not a faceless studio. Site `<title>` pattern: `Bhugol Lab — <page purpose>`.

**Homepage `/`** — headline states the outcome, not a job title. One primary CTA above the fold. Sections: 3 proof points/case-study links, services summary with price ranges, short about, final CTA. No carousel, no hero video, no animated gradients.

**`/hire`** — the cold-outreach landing page. Written for one audience. One offer, one price range, 2–3 most relevant case studies, one CTA (book a call). No blog links, no project links, no distractions. Add a hidden form field that captures `utm_source`, `utm_medium`, `utm_campaign`, and `referrer`.

**`/services`** — 2–3 packages: what's included, what's not, price range, timeline, who it's for.

**`/work`** and **`/work/<slug>`** — case studies as Markdown or strongly-typed content. Each: problem, constraints, what was built, stack, outcome, screenshot. Use `TODO:` placeholders where real results are missing.

**`/blog`**, **`/blog/<slug>`**, **`/blog/tag/<tag>`** — Markdown posts, 3 sample posts, RSS feed at `/rss.xml`, table of contents for long posts.

**`/contact`** — form with name, email, company (optional), message, honeypot field, and hidden UTM fields. Server-side validation, antiforgery token, accessible error messages. On success: redirect to `/contact/thanks` (which fires no analytics conversion on refresh) and send me an email notification.

**`/admin/leads`** — behind auth, simple table of leads sorted by date with a status dropdown. Keep it minimal; cookie auth with a single admin user seeded from configuration is acceptable.

**`/privacy`** — plain-language privacy note covering the form data, analytics, and retention.

### Step 4 — Technical requirements
- Middleware order matters: `UseForwardedHeaders()` before `UseHttpsRedirection()`, `UseRateLimiter()`, and any IP-based logic. The app runs behind Cloudflare, so forwarded headers must be honored or rate limiting will treat all traffic as one client.
- Output caching on anonymous GET page requests (5–10 min). Do not cache POSTs, `/admin`, or `/contact/thanks`. Never render a page from a database query.
- Rate limit the contact endpoint: 3 requests per 10 minutes per IP, plus a global limiter.
- EF Core: `EnableRetryOnFailure(3, TimeSpan.FromSeconds(2), null)`; migrations applied explicitly (no `EnsureCreated()`); `Lead` entity with Id, Name, Email, Company, Message, Source, Referrer, CreatedAt, Status.
- Support SQL Server via connection string; use `Authentication=Active Directory Default` if configured, otherwise fall back to a connection string from user-secrets/env. Include SQLite as a local-development fallback so the project runs without SQL Server installed.
- `IEmailSender` abstraction with two implementations: SMTP (Gmail app password in development and at launch, configurable to Resend/Brevo later via config only) and a no-op logger for tests. Never send email inline on the request thread without a timeout, and never let a failed notification lose the lead — persist first, then notify.
- Email is optional infrastructure: if `Email:Provider` is empty or `noop`, the contact form must still succeed and the lead must still be listed in `/admin/leads`.
- Security headers middleware: HSTS, `X-Content-Type-Options`, `Referrer-Policy: strict-origin-when-cross-origin`, `Permissions-Policy`, and a CSP that allows only what the site uses (self, plus your analytics and Cal.com embed if used).
- Add `/healthz` returning 200 and a `/robots.txt` that disallows `/admin`.
- Structured logging; log every lead submission (id only, no message body) and every email failure.
- Config via `IOptions` bound from `appsettings.json`, overridable by environment variables. Document every setting in the README.
- Custom error page for 404 and 500; no stack traces in production.

### Step 5 — SEO, performance, accessibility
- Per-page `<title>`, meta description, canonical URL, OG and Twitter card tags. Every case study and post gets its own social image (generate a simple OG image endpoint or commit static images).
- JSON-LD: `Person` on `/`, `ProfessionalService` on `/hire` and `/services`, `Article` on posts, `BreadcrumbList` where relevant.
- `sitemap.xml` generated from routes + posts; correct `lastmod`.
- Target: Lighthouse mobile >= 95 on Performance, Accessibility, Best Practices, SEO. LCP < 1.5s, CLS < 0.05, total JS < 50 KB.
- Images: responsive `srcset`, explicit width/height, lazy loading below the fold, AVIF/WebP.
- Accessibility: semantic landmarks, visible focus states, skip link, labels on every input, 4.5:1 contrast, keyboard-operable nav and form.
- Self-host fonts or use system fonts; no render-blocking third-party CSS.

### Step 6 — Tests
- xUnit integration tests using `WebApplicationFactory`: homepage returns 200, `/hire` returns 200, contact POST with invalid data returns validation errors, contact POST with valid data persists a lead and returns redirect, `/admin/leads` requires auth, rate limiter returns 429 after the limit.
- One Playwright smoke test (optional if it complicates setup): load `/`, submit the form, assert the thanks page.

### Step 7 — Deployment artefacts
- Multi-stage `Dockerfile` (`mcr.microsoft.com/dotnet/sdk:10.0` build -> `mcr.microsoft.com/dotnet/aspnet:10.0` runtime), non-root user, `ASPNETCORE_URLS=http://+:8080`.
- `.github/workflows/ci.yml`: restore, build with warnings as errors, run tests, build the Docker image on PRs to `main`.
- `.github/workflows/deploy.yml`: deploy to the chosen host on push to `main` (App Service publish profile or container registry push, depending on the target), using repository secrets.
- Document the exact host settings, environment variables, and DNS records needed in `docs/deployment.md`.

### Acceptance criteria
- `dotnet build` and `dotnet test` pass with zero warnings.
- `git log` shows the incremental commit sequence above; `git status` is clean; nothing sensitive is tracked (verify with `git ls-files | Select-String -Pattern "secret|connectionstring|appsettings.Production"`).
- `dotnet run` works on a clean machine with only the .NET 10 SDK installed (SQLite fallback), and the contact form succeeds end to end with the no-op email sender.
- `https://www.bhugol.dev` 301s to `https://bhugol.dev`, preserving path and query string.
- Every page passes the Lighthouse targets above; no layout shift on load; usable at 390px width.
- All internal links resolve; sitemap, RSS, robots, OG tags and JSON-LD validate.
- No lorem ipsum, no invented clients, no invented metrics. Unfinished content is a visible `TODO:`.

### Working style
- Start by summarizing your plan and listing every placeholder you still need, then build.
- Commit after each meaningful, working increment.
- Prefer boring, well-known solutions. If you think a constraint above is wrong, say so once with a one-paragraph rationale, then follow the constraint unless I agree to change it.
- When done, give me: a file-by-file summary, the exact commands to run locally, the list of DNS/email records to create, the secrets to set, and the remaining `TODO:` list.

---

## Notes before you paste

- **Must fill in:** personal email, niche/ICP, package names + price ranges, booking link, GitHub/LinkedIn handles, hosting target. The homepage and `/hire` copy depend on the niche and prices.
- **Email is deliberately minimal at launch:** use a personal email (e.g. Gmail) as the public contact address and as the lead-notification recipient. No custom-domain mailbox, no mailbox subscription, no SPF/DKIM/DMARC yet. Sending uses Gmail SMTP with an app password stored in user-secrets/env. The `IEmailSender` abstraction must keep a later switch to Resend/Brevo or a real `hi@bhugol.dev` mailbox a config-only change.
- **Free upgrade path, documented but not implemented:** Cloudflare Email Routing can receive `hi@bhugol.dev` and forward to Gmail at $0, and Resend can send from a verified domain without a mailbox. Add it only when replies from the branded address start mattering.
- **If you don't have a niche yet:** tell the new chat to use `TODO:` placeholders for the ICP and prices, and build everything else around them.
- **Naming already decided:** brand **Bhugol Lab**, repo `bhugol-lab`, solution `BhugolLab`, project `BhugolLab.Web`, namespace `BhugolLab.Web`.
- **Domain decisions already made:** canonical site on `bhugol.dev` (apex), `www` 301s to apex. Marketing pages on paths, not subdomains. `bhugol.com` is held by someone else and `bhugollabs.com` is taken — don't pursue either.
- **Stack decisions already made:** single Razor Pages project, .NET 10, Markdig, EF Core + SQL Server (SQLite fallback locally), Docker, Cloudflare DNS/CDN.
- **Hosting:** pick one and co-locate the database with it. Azure App Service/Container Apps + Azure SQL free tier, or Fly.io + Neon Postgres.
- **Placeholder hygiene:** the "no fabricated content" rule is deliberate. Do not let the agent invent client names, metrics, or testimonials.
