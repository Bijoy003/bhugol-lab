# syntax=docker/dockerfile:1

# ---------- build ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY global.json Directory.Build.props ./
COPY BhugolLab.slnx ./
COPY src/BhugolLab.Web/BhugolLab.Web.csproj src/BhugolLab.Web/
COPY tests/BhugolLab.Web.Tests/BhugolLab.Web.Tests.csproj tests/BhugolLab.Web.Tests/
RUN dotnet restore BhugolLab.slnx

COPY .editorconfig ./
COPY src/ src/
COPY tests/ tests/
RUN dotnet publish src/BhugolLab.Web/BhugolLab.Web.csproj -c Release -o /app/publish

# ---------- runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

# Writable home for the SQLite local-dev fallback; production uses SQL Server.
RUN mkdir -p /app/App_Data && chown app /app/App_Data

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER app
ENTRYPOINT ["dotnet", "BhugolLab.Web.dll"]
