# Tower Damage Meter

Select a tower in **Kingdom Rush 6: Genesis** to see its cumulative damage above the normal stats bar. Each placed tower has its own total. Upgrading keeps it; selling and rebuilding starts a new one. Totals live only in memory for the current mission.

## Install

Tested with Steam desktop build `26406218` under Wine. No compiler, mod loader, or archive patch is needed.

1. Launch the game once, then close it. Back up your saves as a precaution.
2. Copy [tower_damage_meter.lua](tower_damage_meter.lua) into the game's existing user-data folder, leaving other files alone:
   - Windows: `%APPDATA%\kingdom_rush_genesis\`
   - Proton: `<Steam library>/steamapps/compatdata/4259190/pfx/drive_c/users/steamuser/AppData/Roaming/kingdom_rush_genesis/`
3. In Steam → **Properties → General → Launch Options**, append `-custom_script tower_damage_meter`. Omit the `.lua` extension. The game accepts one custom script; don't replace another mod's script option.
4. Start a mission and select a built tower. No hotkey is required.

The meter counts enemy HP removed after mitigation, excluding overkill. Soldiers, projectiles, and lingering effects count when the game identifies their owning tower. Hits without a traceable tower are omitted rather than guessed; not every special ability has been verified.

To uninstall, close the game, remove the launch option and delete **only** `tower_damage_meter.lua` from the user-data folder.
