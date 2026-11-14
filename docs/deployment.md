# Deployment notes

Target: **https://bhugol.dev** (apex is canonical; `www` 301s to it).

## 1. Host

Pick one and co-locate the database with it:

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
Email__Smtp__Host=smtp.gmail.com
Email__Smtp__Port=587
Email__Smtp__Username=<personal-email@gmail.com>
Email__Smtp__Password=<gmail-app-password>
Email__FromAddress=<personal-email@gmail.com>
Email__ToAddress=<personal-email@gmail.com>
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

That's all that's required at launch. **No MX, SPF, DKIM, or DMARC records** — the site
does not use a `bhugol.dev` mailbox yet; the public contact address and lead
notifications use a personal email.

Cloudflare redirect rule (not app code): `www.bhugol.dev/*` → `https://bhugol.dev/$1`, 301.

SSL/TLS mode: **Full (strict)**. Enable "Always Use HTTPS", Brotli, and
"Email Address Obfuscation" is optional. Never use Flexible — it loops with
`UseHttpsRedirection`.

## 3. Email (personal address, no mailbox cost)

At launch the site uses a **personal email address** for both the public contact link
and lead notifications. Nothing is billed, and no `bhugol.dev` mailbox exists.

- **Public contact:** a `mailto:` link and the `/contact` form both target the personal
  address. Set this in `Email:ToAddress` and in the site config.
- **Sending notifications:** Gmail SMTP with an **app password**
  (`smtp.gmail.com`, port `587`, STARTTLS). Requires 2FA on the Google account.
  - Store the app password as `Email__Smtp__Password` in the host and in
    `dotnet user-secrets` locally. Never in `appsettings.json`.
  - Gmail limits: ~500 recipients/day, which is far above lead-notification volume.
  - Keep `Email:Provider=noop` locally so the form still works offline; leads are
    persisted and visible at `/admin/leads` regardless of email state.
- **Do not** use the Google account password, and do not expose the app password to
  the browser.

**Later, still free if you want it:** Cloudflare Email Routing receives
`hi@bhugol.dev` and forwards to Gmail at $0, and Resend (3,000 emails/month free)
can send from a verified domain without a mailbox. Both are config/DNS changes
behind `IEmailSender` — nothing in the app needs rewriting.

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
