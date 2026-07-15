#!/usr/bin/env bash
# Mints a short-lived HS256 JWT signed with the same key configured in
# src/BitCoin.API/appsettings.json (Jwt:Key), for local/dev testing only.
#
# Usage:
#   ./scripts/generate-dev-jwt.sh
#   JWT_KEY="..." JWT_ISSUER="..." JWT_AUDIENCE="..." ./scripts/generate-dev-jwt.sh
#
# Prints the bare token to stdout so it can be captured, e.g.:
#   TOKEN=$(./scripts/generate-dev-jwt.sh)

set -euo pipefail

JWT_KEY="${JWT_KEY:-__USE_ENVIRONMENT_JWT_SECRET_32_CHARS__}"
JWT_ISSUER="${JWT_ISSUER:-Test.com}"
JWT_AUDIENCE="${JWT_AUDIENCE:-BitCoin.API.Clients}"
JWT_TTL_SECONDS="${JWT_TTL_SECONDS:-3600}"

base64url() {
    base64 | tr '+/' '-_' | tr -d '='
}

now=$(date +%s)
exp=$((now + JWT_TTL_SECONDS))

header='{"alg":"HS256","typ":"JWT"}'
payload=$(printf '{"iss":"%s","aud":"%s","iat":%d,"exp":%d,"sub":"dev-user"}' \
    "$JWT_ISSUER" "$JWT_AUDIENCE" "$now" "$exp")

header_b64=$(printf '%s' "$header" | base64url)
payload_b64=$(printf '%s' "$payload" | base64url)
signing_input="${header_b64}.${payload_b64}"

signature=$(printf '%s' "$signing_input" \
    | openssl dgst -sha256 -hmac "$JWT_KEY" -binary \
    | base64url)

printf '%s.%s\n' "$signing_input" "$signature"
