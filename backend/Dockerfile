# ── Build stage ──────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# Restore as a distinct layer for better caching.
COPY src/Virentum.Api/Virentum.Api.csproj src/Virentum.Api/
RUN dotnet restore src/Virentum.Api/Virentum.Api.csproj

# Copy the rest and publish.
COPY . .
RUN dotnet publish src/Virentum.Api/Virentum.Api.csproj \
    -c Release \
    -o /app \
    --no-restore

# ── Runtime stage ────────────────────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Run as a non-root user for security.
RUN adduser --disabled-password --gecos "" --uid 1001 appuser
USER appuser

COPY --from=build /app ./

ENV ASPNETCORE_URLS=http://+:8080 \
    ASPNETCORE_ENVIRONMENT=Production
EXPOSE 8080

ENTRYPOINT ["dotnet", "Virentum.Api.dll"]
