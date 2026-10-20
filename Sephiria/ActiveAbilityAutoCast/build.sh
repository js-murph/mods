#!/usr/bin/env bash
set -euo pipefail

project_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
game_dir="${SEPHIRIA_GAME_DIR:-$HOME/.steam/steam/steamapps/common/Sephiria}"
dotnet_bin="${DOTNET_BIN:-dotnet}"

"$dotnet_bin" build \
  "$project_dir/ActiveAbilityAutoCast.csproj" \
  --configuration Release \
  -p:SephiriaManagedDir="$game_dir/Sephiria_Data/Managed"

echo "Package ready at: $project_dir/package"
