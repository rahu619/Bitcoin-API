#!/usr/bin/env bash
set -euo pipefail

export PATH="$PATH:$HOME/.dotnet/tools"

LOG_DIR="/tmp/bitcoin-api"
mkdir -p "$LOG_DIR"
APPHOST_PATH="src/BitCoin.AppHost/BitCoin.AppHost.csproj"

is_running() {
  local pattern="$1"
  pgrep -f "$pattern" >/dev/null 2>&1
}

start_api() {
  if is_running "dotnet run --project src/BitCoin.API/BitCoin.API.csproj"; then
    echo "[post-start] API already running"
    return
  fi

  echo "[post-start] starting API on port 8080"
  nohup dotnet run --project src/BitCoin.API/BitCoin.API.csproj --no-build >"$LOG_DIR/api.log" 2>&1 &
}

start_apphost() {
  if is_running "aspire run --non-interactive --apphost $APPHOST_PATH"; then
    echo "[post-start] AppHost already running"
    return
  fi

  echo "[post-start] starting AppHost with Aspire CLI"
  nohup ~/.dotnet/tools/aspire run --non-interactive --apphost "$APPHOST_PATH" --no-build >"$LOG_DIR/apphost.log" 2>&1 &
}

open_dashboard() {
  if [[ ! -f "$LOG_DIR/apphost.log" ]]; then
    return
  fi

  local dashboard_url
  dashboard_url="$(grep -m 1 -oE 'https://localhost:[0-9]+/login\?t=[A-Za-z0-9]+' "$LOG_DIR/apphost.log" || true)"

  if [[ -z "$dashboard_url" ]]; then
    dashboard_url="$(grep -m 1 -oE 'https://localhost:[0-9]+' "$LOG_DIR/apphost.log" || true)"
  fi

  if [[ -n "$dashboard_url" ]]; then
    echo "[post-start] dashboard URL: $dashboard_url"
    if [[ -n "${BROWSER:-}" ]]; then
      "$BROWSER" "$dashboard_url" >/dev/null 2>&1 || true
    fi
  fi
}

wait_for_port() {
  local port="$1"
  local name="$2"

  for _ in {1..30}; do
    if (echo >"/dev/tcp/127.0.0.1/$port") >/dev/null 2>&1; then
      echo "[post-start] $name is listening on $port"
      return
    fi
    sleep 1
  done

  echo "[post-start] warning: $name did not bind port $port within timeout"
}

start_api
start_apphost
wait_for_port 8080 "API"
wait_for_port 18889 "Aspire dashboard"
open_dashboard

echo "[post-start] startup complete"
