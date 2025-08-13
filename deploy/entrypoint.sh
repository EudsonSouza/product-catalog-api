#!/usr/bin/env bash
set -euo pipefail

PORT="${PORT:-8080}"
export ASPNETCORE_URLS="http://0.0.0.0:${PORT}"

echo "==> Applying EF Core migrations (with retry) ..."
max_attempts=30
sleep_seconds=5
attempt=1

while true; do
  if dotnet /app/migrator/ProductCatalog.Migrator.dll; then
    echo "==> Migrations applied successfully."
    break
  fi

  if [ "$attempt" -ge "$max_attempts" ]; then
    echo "ERROR: Could not apply migrations after ${max_attempts} attempts. Exiting."
    exit 1
  fi

  echo "Attempt ${attempt} failed. Retrying in ${sleep_seconds}s..."
  attempt=$((attempt+1))
  sleep "${sleep_seconds}"
done

echo "==> Starting API on ${ASPNETCORE_URLS} ..."
exec dotnet /app/api/ProductCatalog.API.dll
