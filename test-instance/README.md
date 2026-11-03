# Test instance for the live tests

The SDK has two kinds of tests. The unit tests run against an in-process fake server. The **live tests**
(`Category=Live` in `src/Infisical.Sdk.UnitTests`) talk to a real Infisical instance. This folder sets up a throw-away
instance for them. **It only contains test credentials; do not put real data into it.**

## With Docker

```bash
cd test-instance
./run-live-tests.sh
```

The script

1. creates a `.env` with random secrets (not committed) and starts Infisical, Postgres and Redis with
   `docker-compose.yml` (the server runs its database migrations itself, the first start pulls the images),
2. waits until `/api/status` answers and bootstraps the instance (first admin user, organization, machine identity),
3. runs `dotnet test --filter Category=Live` with `INFISICAL_LIVE_URL`, `INFISICAL_LIVE_TOKEN`, `INFISICAL_LIVE_ORG_ID`,
   `INFISICAL_LIVE_USER_EMAIL` and `INFISICAL_LIVE_USER_PASSWORD` set,
4. removes the instance and its data again.

| Option | Effect |
| --- | --- |
| `--keep` | leave the instance running afterwards |
| `--no-tests` | only start and bootstrap, print the `INFISICAL_LIVE_*` variables to use with `dotnet test` yourself |
| `INFISICAL_IMAGE_TAG=v0.167.4` | use a specific Infisical version instead of `latest` |
| `INFISICAL_PORT=9090` | publish the instance on another local port |

It needs `bash`, `curl`, `dotnet`, `docker` with the compose plugin, and either `jq` or `python3`. On Windows run it in
WSL or Git Bash. The instance is only published on `127.0.0.1`.

## With an instance that is already running

```bash
INFISICAL_URL=http://localhost:8080 ./run-live-tests.sh
```

skips Docker and bootstraps that instance. An instance can only be bootstrapped once; for one that is already set up pass
its credentials instead:

```bash
INFISICAL_URL=... INFISICAL_LIVE_TOKEN=... INFISICAL_LIVE_ORG_ID=... \
  [INFISICAL_LIVE_USER_EMAIL=... INFISICAL_LIVE_USER_PASSWORD=...] ./run-live-tests.sh
```

Without the user variables the test for the user-only endpoints is skipped.

## Without Docker (from source)

This is how the live tests were first run, in an environment without Docker. You need Postgres and Redis running locally:

```bash
git clone --depth 1 https://github.com/Infisical/infisical && cd infisical/backend
npm ci
# Linux: the native module `odbc` needs libodbc, e.g. `apt-get install unixodbc` before `npm ci`
export ENCRYPTION_KEY=$(openssl rand -hex 16) AUTH_SECRET=$(openssl rand -base64 32)
export DB_CONNECTION_URI=postgres://infisical:infisical@localhost:5432/infisical
export REDIS_URL=redis://localhost:6379 SITE_URL=http://localhost:8080 PORT=8080
npx tsx --tsconfig=./tsconfig.dev.json ./src/main.ts     # runs the database migrations on start
```

Then use `INFISICAL_URL=http://localhost:8080 ./run-live-tests.sh` from this folder. The backend's `package.json` asks for a
newer Node.js than the one it was tried with (22), so use the version it asks for if you run into problems.

## What the live tests do not cover

Features that need an Enterprise license (groups, custom roles, project templates, gateway pools, secret rotations, secret
scanning, creating SCIM tokens) answer with a plan restriction on an instance without a license key. They are therefore not
part of the live tests.

## Status of this folder

The script was tested against a freshly started instance without Docker (bootstrap, reading the token with `jq` and with
the `python3` fallback, all live tests, the error for an already bootstrapped instance, the check for a missing Docker
daemon). The Docker Compose part itself could not be run where the script was written, because that environment has no
Docker daemon; `docker compose config` accepts the file.
