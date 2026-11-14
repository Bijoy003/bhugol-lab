# Deployment notes

Target: **https://bhugol.dev** (apex is canonical; `www` 301s to it).

## 1. Host

Pick a host. The database is **SQL Server** in every case — Azure SQL (free tier) or
any instance the host can reach; SQLite is the local-dev fallback only:

| Option | Notes |
|---|---|
| Azure App Service (Linux) | Push the Docker image or use `azure/webapps-deploy`. ~$13/mo at B1 |
| Azure Container Apps | Consumption plan, scale-to-zero; set `minReplicas: 1` to avoid cold starts |
| Fly.io | `fly.toml`, `min_machines_running = 1`, ~$4/mo |
| Any Docker host | The image listens on `8080` and needs no special privileges |

Required environment variables in the host:

```
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
Site__BaseUrl=https://bhugol.dev
ConnectionStrings__Default=<sql-server-connection-string>
Email__Provider=smtp
Email__Smtp__Host=<smtp-host>
Email__Smtp__Port=587
Email__Smtp__Username=<smtp-username>
Email__Smtp__Password=<smtp-password>
Email__FromAddress=<notification-sender-address>
Email__ToAddress=<notification-recipient-address>
Admin__Username=<user>
Admin__PasswordHash=<hash>
```

Use Managed Identity (`Authentication=Active Directory Default`) instead of a
password in the connection string when hosted on Azure.

## 2. DNS records (Cloudflare)

| Type | Name | Content | Proxy |
|---|---|---|---|
| A / CNAME | `bhugol.dev` (@) | host target | Proxied |
| CNAME | `www` | `bhugol.dev` | Proxied |

That's all that's required to launch the site itself. Email records (MX for receiving,
SPF/DKIM/DMARC for sending from a custom domain) depend on the provider and address
configured via the `Email:*` settings — add them when that choice is made.

Cloudflare redirect rule (not app code): `www.bhugol.dev/*` → `https://bhugol.dev/$1`, 301.

SSL/TLS mode: **Full (strict)**. Enable "Always Use HTTPS", Brotli, and
"Email Address Obfuscation" is optional. Never use Flexible — it loops with
`UseHttpsRedirection`.

## 3. Email (config-driven, provider-agnostic)

The app talks to `IEmailSender`; provider and address are pure configuration. One
configured address serves as both the public contact link and the lead-notification
recipient.

- **Public contact:** the `mailto:` link and the `/contact` form both target the
  configured address. Set it in `Email:ToAddress` and in the site config.
- **Sending notifications:** any SMTP provider via `Email:Smtp:*` (host, port,
  credentials), or `Email:Provider=resend` for an API-based sender. Store credentials
  in the host environment and in `dotnet user-secrets` locally — never in
  `appsettings.json`, and never expose them to the browser.
- **Locally:** keep `Email:Provider=noop` so the form still works offline; leads are
  persisted and visible at `/admin/leads` regardless of email state.
- **Branded address, optional:** Cloudflare Email Routing can receive `hi@bhugol.dev`
  and forward it anywhere at $0, and Resend (3,000 emails/month free) can send from a
  verified domain without a mailbox. Both are DNS/config changes behind `IEmailSender`
  — nothing in the app needs rewriting.

## 4. Post-deploy checks

- [ ] `https://bhugol.dev/healthz` returns 200
- [ ] `https://www.bhugol.dev/anything` 301s to the apex, path preserved
- [ ] Contact form submits from a phone on mobile data; notification email arrives
- [ ] `curl -I https://bhugol.dev` shows HSTS and no `Server` header leaks
- [ ] `/admin/leads` requires auth; `/admin` is disallowed in `robots.txt`
- [ ] Lighthouse mobile ≥ 95 on `/` and `/hire`
- [ ] `/hire` OG tags render correctly in WhatsApp, Slack, and LinkedIn
- [ ] Cloudflare Web Analytics receiving page views
- [ ] UptimeRobot (or similar) monitoring `/healthz`
- [ ] Budget alert configured on the cloud account
