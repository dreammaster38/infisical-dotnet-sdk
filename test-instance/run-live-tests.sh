#!/usr/bin/env bash
# Starts a throw-away Infisical instance, bootstraps it and runs the live tests of the SDK against it.
#
#   ./run-live-tests.sh              start (Docker Compose), bootstrap, run the live tests, remove the instance
#   ./run-live-tests.sh --keep       ... and leave the instance running afterwards
#   ./run-live-tests.sh --no-tests   only start + bootstrap and print the INFISICAL_LIVE_* variables
#
# Use an instance that is already running instead of Docker (it must not be bootstrapped yet):
#   INFISICAL_URL=http://localhost:8080 ./run-live-tests.sh
# An instance that is already bootstrapped can be used by passing its credentials:
#   INFISICAL_URL=... INFISICAL_LIVE_TOKEN=... INFISICAL_LIVE_ORG_ID=... \
#     [INFISICAL_LIVE_USER_EMAIL=... INFISICAL_LIVE_USER_PASSWORD=...] ./run-live-tests.sh
#
# Needs: bash, curl, dotnet, and either jq or python3. For the Docker mode also docker with the compose plugin.
# On Windows run it in WSL or Git Bash.
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")"
ROOT="$(cd .. && pwd)"

KEEP=false
RUN_TESTS=true
for arg in "$@"; do
  case "$arg" in
    --keep) KEEP=true ;;
    --no-tests) RUN_TESTS=false ;;
    -h|--help) sed -n '2,15p' "${BASH_SOURCE[0]}" | sed 's/^# \{0,1\}//'; exit 0 ;;
    *) echo "unknown argument: $arg (see --help)" >&2; exit 2 ;;
  esac
done

die() { echo "error: $*" >&2; exit 1; }
need() { command -v "$1" >/dev/null 2>&1 || die "'$1' is required but was not found"; }
log() { echo "==> $*"; }

need curl
need dotnet
command -v jq >/dev/null 2>&1 || command -v python3 >/dev/null 2>&1 || die "either 'jq' or 'python3' is required"

# json_get '<json>' 'a.b.c' - prints the value at the path
json_get() {
  if command -v jq >/dev/null 2>&1; then
    printf '%s' "$1" | jq -r ".$2 // empty"
  else
    printf '%s' "$1" | python3 -c '
import json, sys
value = json.load(sys.stdin)
for part in sys.argv[1].split("."):
    value = value.get(part) if isinstance(value, dict) else None
    if value is None:
        break
print("" if value is None else value)' "$2"
  fi
}

# random <bytes> [hex|base64]
random() {
  local bytes="$1" format="${2:-hex}"
  if command -v openssl >/dev/null 2>&1; then
    openssl rand "-$format" "$bytes" | tr -d '\n'
  else
    python3 -c "import secrets,base64,sys; b=secrets.token_bytes($bytes); sys.stdout.write(b.hex() if '$format'=='hex' else base64.b64encode(b).decode())"
  fi
}

COMPOSE_STARTED=false
cleanup() {
  if [ "$COMPOSE_STARTED" = true ] && [ "$KEEP" = false ]; then
    log "removing the test instance and its data"
    docker compose --env-file .env down -v >/dev/null 2>&1 || true
  fi
}
trap cleanup EXIT

# ---- 1. the instance ------------------------------------------------------------------------------------------------
if [ -z "${INFISICAL_URL:-}" ]; then
  need docker
  docker compose version >/dev/null 2>&1 || die "the 'docker compose' plugin is required"
  docker info >/dev/null 2>&1 || die "the Docker daemon is not reachable - start Docker, or point INFISICAL_URL at an instance that is already running"

  if [ ! -f .env ]; then
    log "creating .env with random secrets"
    {
      echo "ENCRYPTION_KEY=$(random 16 hex)"
      echo "AUTH_SECRET=$(random 32 base64)"
      echo "POSTGRES_PASSWORD=$(random 16 hex)"
    } > .env
  fi

  log "starting the Infisical test instance (the first start pulls the images and runs the database migrations)"
  docker compose --env-file .env up -d
  COMPOSE_STARTED=true
  INFISICAL_URL="http://localhost:${INFISICAL_PORT:-8080}"
else
  INFISICAL_URL="${INFISICAL_URL%/}"
fi

log "waiting for $INFISICAL_URL"
ready=false
for _ in $(seq 1 120); do
  if curl -fsS -m 3 "$INFISICAL_URL/api/status" >/dev/null 2>&1; then ready=true; break; fi
  sleep 3
done
if [ "$ready" != true ]; then
  [ "$COMPOSE_STARTED" = true ] && docker compose --env-file .env logs --tail 40 backend || true
  die "the instance did not become ready in time"
fi

# ---- 2. bootstrap ---------------------------------------------------------------------------------------------------
if [ -n "${INFISICAL_LIVE_TOKEN:-}" ] && [ -n "${INFISICAL_LIVE_ORG_ID:-}" ]; then
  log "using the credentials from INFISICAL_LIVE_TOKEN / INFISICAL_LIVE_ORG_ID"
else
  email="${INFISICAL_LIVE_USER_EMAIL:-admin@example.com}"
  password="${INFISICAL_LIVE_USER_PASSWORD:-T3st-$(random 12 hex)-Xx!}"

  log "bootstrapping the instance (first admin user, organization and machine identity)"
  payload="$(printf '{"email":"%s","password":"%s","organization":"SDK live tests"}' "$email" "$password")"
  response="$(curl -sS -m 60 -w '\n%{http_code}' -X POST "$INFISICAL_URL/api/v1/admin/bootstrap" \
    -H 'Content-Type: application/json' -d "$payload")"
  status="${response##*$'\n'}"
  body="${response%$'\n'*}"
  if [ "$status" != "200" ]; then
    echo "$body" >&2
    die "the bootstrap failed (HTTP $status). An instance can only be bootstrapped once - for one that is already set up pass INFISICAL_LIVE_TOKEN and INFISICAL_LIVE_ORG_ID"
  fi

  INFISICAL_LIVE_TOKEN="$(json_get "$body" identity.credentials.token)"
  INFISICAL_LIVE_ORG_ID="$(json_get "$body" organization.id)"
  [ -n "$INFISICAL_LIVE_TOKEN" ] && [ -n "$INFISICAL_LIVE_ORG_ID" ] || die "could not read the token / organization id from the bootstrap response"
  INFISICAL_LIVE_USER_EMAIL="$email"
  INFISICAL_LIVE_USER_PASSWORD="$password"
fi

export INFISICAL_LIVE_URL="$INFISICAL_URL"
export INFISICAL_LIVE_TOKEN INFISICAL_LIVE_ORG_ID
[ -n "${INFISICAL_LIVE_USER_EMAIL:-}" ] && export INFISICAL_LIVE_USER_EMAIL
[ -n "${INFISICAL_LIVE_USER_PASSWORD:-}" ] && export INFISICAL_LIVE_USER_PASSWORD

# ---- 3. tests -------------------------------------------------------------------------------------------------------
if [ "$RUN_TESTS" = false ]; then
  log "the instance is ready (throw-away credentials, local use only):"
  echo "export INFISICAL_LIVE_URL='$INFISICAL_LIVE_URL'"
  echo "export INFISICAL_LIVE_TOKEN='$INFISICAL_LIVE_TOKEN'"
  echo "export INFISICAL_LIVE_ORG_ID='$INFISICAL_LIVE_ORG_ID'"
  [ -n "${INFISICAL_LIVE_USER_EMAIL:-}" ] && echo "export INFISICAL_LIVE_USER_EMAIL='$INFISICAL_LIVE_USER_EMAIL'"
  [ -n "${INFISICAL_LIVE_USER_PASSWORD:-}" ] && echo "export INFISICAL_LIVE_USER_PASSWORD='$INFISICAL_LIVE_USER_PASSWORD'"
  # the instance has to stay up so that the printed variables are usable
  KEEP=true
  exit 0
fi

log "running the live tests"
set +e
dotnet test "$ROOT/src/Infisical.Sdk.UnitTests" --filter "Category=Live"
code=$?
set -e

if [ "$KEEP" = true ] && [ "$COMPOSE_STARTED" = true ]; then
  log "the instance keeps running at $INFISICAL_URL (docker compose --env-file .env down -v removes it)"
fi
exit $code
