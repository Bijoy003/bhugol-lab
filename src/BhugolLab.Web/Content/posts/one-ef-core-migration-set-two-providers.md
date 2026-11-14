---
title: "One EF Core migration set that runs on SQL Server and SQLite"
description: "Production wants SQL Server, local dev wants zero setup. Instead of maintaining two migration folders, shape the model so one set of migrations applies cleanly on both providers."
date: 2026-09-30
tags: [dotnet, efcore, sql-server]
draft: false
---

This site runs on SQL Server in production and on a SQLite file locally, so a
fresh clone works with nothing installed. The tempting approach — two
migration folders, one per provider — doubles the maintenance and drifts
within weeks. The alternative is to keep **one** migration set, generated
against SQL Server, and make it apply cleanly on SQLite too. Three model
decisions make that possible.

## Why SQL Server migrations break on SQLite

A migration is a list of operations, and each column operation carries the
column type the design-time provider chose. SQL Server picks types SQLite's
DDL parser chokes on or misinterprets:

- `nvarchar(max)` — SQLite type names may only contain numbers in
  parentheses, so `(max)` is a syntax error.
- `int` identity keys — SQL Server bakes in an identity annotation SQLite
  ignores, leaving a NOT NULL primary key with no generated value.
- `datetime2` — accepted by SQLite, but worth knowing it lands in a column
  with NUMERIC affinity; date strings still store and sort correctly.

Fix the first two at the model level and the migration set becomes portable.

## Decision 1: explicit max length on every string

`HasMaxLength` on each string property turns `nvarchar(max)` into
`nvarchar(400)`, which SQLite parses as a TEXT-affinity column. It also makes
the schema honest about its limits, which the form validation already
enforces:

```csharp
builder.Property(lead => lead.Name).HasMaxLength(100);
builder.Property(lead => lead.Email).HasMaxLength(254);
builder.Property(lead => lead.Message).HasMaxLength(4000);
```

## Decision 2: Guid primary keys

With a `Guid` key the application always supplies the value, so there is no
identity mechanism to disagree about. SQL Server stores `uniqueidentifier`,
SQLite stores the text form, and both are happy. For a leads table with a few
thousand rows a year, sequential-Guid generation at the application layer
removes any index-fragmentation argument against it.

## Decision 3: a design-time factory pinned to SQL Server

Migrations must be scaffolded with one provider or the other. Pin the design
time to SQL Server so the committed migrations always carry the production
types, and let the runtime pick the provider from configuration:

```csharp
public sealed class DesignTimeFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args) =>
        new(new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlServer("Server=localhost;Database=design-time-only")
            .Options);
}
```

The connection string is never opened by `dotnet ef migrations add` — only
the model is needed.

## Applying migrations at startup

With the model shaped this way, `Database.Migrate()` works against either
provider, so the app applies migrations explicitly on startup and
`EnsureCreated()` never appears:

```csharp
using var scope = app.Services.CreateScope();
scope.ServiceProvider.GetRequiredService<AppDbContext>().Database.Migrate();
```

SQLite gets its schema from the same operations SQL Server uses; SQL Server
gets the production types it was designed with. One folder, one snapshot, no
drift — and `dotnet run` on a clean machine still just works.
