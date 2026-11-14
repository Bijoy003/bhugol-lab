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
Email__Provider=resend
Email__ApiKey=<secret>
Email__FromAddress=hi@bhugol.dev
Email__ToAddress=<your-inbox>
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
| TXT | `@` | `v=spf1 include:<mail-provider> ~all` | DNS only |
| TXT | `_dmarc` | `v=DMARC1; p=none; rua=mailto:dmarc@bhugol.dev` | DNS only |
| TXT / CNAME | DKIM selector from mail provider | provider value | DNS only |
| TXT | `resend._domainkey` (or provider equivalent) | for transactional email | DNS only |
| MX | `@` | mail provider MX records | DNS only |

Cloudflare redirect rule (not app code): `www.bhugol.dev/*` → `https://bhugol.dev/$1`, 301.

SSL/TLS mode: **Full (strict)**. Enable "Always Use HTTPS", Brotli, and
"Email Address Obfuscation" is optional. Never use Flexible — it loops with
`UseHttpsRedirection`.

## 3. Email

- Receiving: Cloudflare Email Routing → `hi@bhugol.dev` forwards to your inbox.
  (Or a real mailbox: Zoho Mail Lite ~$1.20/mo, Migadu ~$19/yr.)
- Sending: Resend / Brevo, domain verified (SPF + DKIM + DMARC above), used by
  `IEmailSender` for lead notifications.
- Sending as `hi@bhugol.dev` from your own mail client requires a real mailbox,
  not just forwarding.

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
