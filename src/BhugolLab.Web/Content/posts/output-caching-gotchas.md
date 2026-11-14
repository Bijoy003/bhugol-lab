---
title: "Output caching in ASP.NET Core: the four gotchas that bite"
description: "Output caching is the cheapest performance win in ASP.NET Core — until it caches your antiforgery token or your admin session. Four traps and how to avoid them."
date: 2026-10-22
tags: [dotnet, aspnet-core, performance]
draft: false
---

`AddOutputCache()` plus one line on your endpoints can take a Razor Pages site
from "renders on every request" to "serves bytes from memory". It is the
cheapest performance win available in ASP.NET Core. It is also easy to misuse
in ways that produce baffling bugs. Four traps I hit, in the order I hit them.

## Trap 1: caching pages that contain antiforgery tokens

Razor Pages validate an antiforgery token on every POST, and the token pair is
per user: the form field embeds the value of the antiforgery cookie. If you
cache the HTML of a page that contains a form, the first visitor's token gets
served to everyone. The second visitor posts a form token that no longer
matches their cookie and gets a 400.

The fix is to exclude form pages from the cache policy:

```csharp
options.AddBasePolicy(policy => policy
    .Expire(TimeSpan.FromMinutes(5))
    .With(context => ValueTask.FromResult(
        HttpMethods.IsGet(context.HttpContext.Request.Method)
        && !context.HttpContext.Request.Path.StartsWithSegments("/contact")
        && !context.HttpContext.Request.Path.StartsWithSegments("/admin"))));
```

Content pages without forms cache happily; form pages render fresh.

## Trap 2: caching authenticated responses

A cached response is shared between users. If an authenticated page is cached,
user A can be served user B's HTML. The default policy refuses to store
responses for requests carrying an `Authorization` header, but cookie
authentication does not set that header — it sets a cookie. Exclude anything
behind auth explicitly, by path or by checking
`context.HttpContext.User.Identity.IsAuthenticated`.

## Trap 3: POSTs and redirects

Caching is for GET (and HEAD). A policy applied to a whole endpoint group will
happily see your POSTs pass through — they are simply not stored — but the
*redirect* response after a successful POST is a different story if you cache
by path alone. Keep the predicate explicit about the method, and exclude
post-redirect landing pages like `/contact/thanks`, which must never be
served stale.

## Trap 4: forgetting where the middleware sits

`UseOutputCache()` must run after `UseRouting()`, `UseAuthentication()` and
`UseAuthorization()`, and before your endpoints. Put it earlier and the
policy cannot see the authenticated user; put it later and it never runs.
The same ordering instinct applies to `UseForwardedHeaders()`, which belongs
even earlier — before HTTPS redirection and any IP-based logic.

## The payoff

With those four handled, anonymous content pages are served from memory with a
five-minute expiry, the database is never touched for a page render, and the
only uncached pages are the ones that genuinely depend on who is asking. For a
marketing site that is exactly the right trade.
