---
title: "Why a marketing site should be boring: Razor Pages over an SPA"
description: "A lead-generation site has one job — turn visitors into inquiries. Here is why server-rendered Razor Pages beat a JavaScript SPA for that job."
date: 2026-11-10
tags: [dotnet, aspnet-core, architecture]
draft: false
---

A marketing site has exactly one business job: turn a visitor into an inquiry.
Every technical decision should be judged against that job. For this site the
answer was a single ASP.NET Core Razor Pages project — no separate API, no
client-side framework. Here is the reasoning.

## The job, restated

A visitor arrives from a search result or a cold email, spends a few seconds
deciding whether to stay, and either leaves or sends a message. Three things
decide the outcome:

1. **Speed.** The first paint on a mid-range phone over mobile data.
2. **Credibility.** Clear writing, working links, no layout jumping around.
3. **Findability.** Search engines and link previews seeing real content.

None of these need a client-side framework. Two of them get actively worse
with one.

## What the SPA tax costs

A JavaScript SPA ships an empty HTML document plus a bundle. The content the
visitor came for arrives only after the bundle downloads, parses and fetches
data. On a good connection that is a second or two of blank screen; on a bad
one it is a bounce.

Link previews suffer too. Many chat apps and social crawlers do not execute
JavaScript — they read the HTML they were given. An SPA served to a crawler
looks like an empty `<div id="root">`. Server-rendered pages hand every
crawler the finished article, the meta tags and the Open Graph image in the
first response.

## Why Razor Pages specifically

MVC, Razor Pages and Blazor can all server-render. Razor Pages wins for a
content site because each page is one file pair — `Contact.cshtml` and
`Contact.cshtml.cs` — with its route, model and handler next to each other.
There is no controller to route to, no action name to keep in sync. For a
site with eight pages that simplicity is the feature.

The alternative that keeps coming up is "Blazor for interactivity". This site
needs almost none: a form, a dropdown, links. The entire JavaScript budget is
a few hundred bytes that copy URL parameters into hidden form fields.

## The shape that worked

- One project, one deployment artefact, one Docker image.
- Markdown files in git for posts and case studies, rendered with Markdig.
- A database with exactly one table: leads from the contact form.
- Output caching for anonymous pages, so the server does almost no work.

The result is a site that loads in one request, renders the same HTML for
every crawler, and has no build step between "edit the Markdown" and
"the post is live".

## When I would choose differently

If the product *is* the interface — dashboards, editors, anything with rich
client-side state — a SPA or Blazor is the right call. A marketing site is not
that product. Boring technology, fast pages, real HTML: that is the whole
trick.
