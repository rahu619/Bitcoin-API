#!/usr/bin/env bash
set -euo pipefail

export PATH="$PATH:$HOME/.dotnet/tools"

echo "[post-create] ensuring Aspire CLI is installed"
dotnet tool update -g aspire.cli || dotnet tool install -g aspire.cli

echo "[post-create] restoring and building solution"
dotnet build BitCoin.API.slnx -v minimal

echo "[post-create] complete"
