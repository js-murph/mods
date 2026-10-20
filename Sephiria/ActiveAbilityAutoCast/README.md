# Active Ability & Grimoire Auto-Cast

A Sephiria add-on that lets active abilities and single-shot grimoire spells be
toggled to cast automatically whenever they are ready.

Built against Sephiria 1.0.23 (Steam build 24614196), using its built-in add-on API.
Game updates may require rebuilding. Charged grimoires, weapon attacks, and weapon
skills are not automated; casting pauses when paused, dead, or blocked by a menu.

## Controls

- **Keyboard/mouse:** `Alt` + `1`–`8`, or middle-click a quick-slot icon.
- **Xbox-layout controller:** hold `LT` + `RT`, then `A/B/X/Y` for slots 1–4 or
  D-pad `Down/Right/Up/Left` for slots 5–8.
- A green square means enabled. Repeat the toggle to turn it off.

Toggles follow the ability or grimoire between slots and reset when the game closes
or the add-on unloads. The controller toggle may also cast the selected action once.

## Build and install

Requires the .NET 8+ SDK and a local game installation. From this mod's directory,
with Sephiria closed:

```bash
export SEPHIRIA_GAME_DIR="/path/to/Sephiria"
./build.sh
./install.sh
```

Alternatively, copy the built `package/` contents into the game's
`AddOns/ActiveAbilityAutoCast/` directory. It must contain `metadata.json` and
`ActiveAbilityAutoCast.dll`. `DOTNET_BIN` can select a different SDK executable.

Launch Sephiria and check `Player.log` for the add-on's loaded message.

## Uninstall

Close Sephiria and remove `AddOns/ActiveAbilityAutoCast/`. The add-on does not
alter save data.
