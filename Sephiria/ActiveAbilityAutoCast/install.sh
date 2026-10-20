#!/usr/bin/env bash
set -euo pipefail

project_dir="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
game_dir="${SEPHIRIA_GAME_DIR:-$HOME/.steam/steam/steamapps/common/Sephiria}"
target_dir="$game_dir/AddOns/ActiveAbilityAutoCast"

if pgrep -f '[S]ephiria.exe' >/dev/null; then
  echo "Sephiria is running. Exit the game before installing the add-on." >&2
  exit 1
fi

if [[ ! -f "$project_dir/package/metadata.json" \
   || ! -f "$project_dir/package/ActiveAbilityAutoCast.dll" ]]; then
  echo "The packaged add-on is incomplete. Run ./build.sh first." >&2
  exit 1
fi

install -d "$target_dir"
install -m 0644 \
  "$project_dir/package/metadata.json" \
  "$project_dir/package/ActiveAbilityAutoCast.dll" \
  "$target_dir/"

echo "Installed Active Ability & Grimoire Auto-Cast at: $target_dir"
