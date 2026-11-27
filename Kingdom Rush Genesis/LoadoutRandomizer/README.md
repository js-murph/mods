# Loadout Randomizer

Open **Heroes**, **Towers**, or **Spells** on the map in *Kingdom Rush 6: Genesis*. Click **Randomize heroes**, **Randomize towers**, or **Randomize spells** to reroll just that room. Click **Randomize all** in any room to reroll all three categories. There is no hotkey.

Each roll fills the same number of slots with distinct choices from your available roster. A choice can repeat from the previous roll. Changes save immediately, just as if you equipped each unit yourself; you can still change any pick manually.

## Install

Tested on Steam desktop build `26406218` under Wine. No mod loader or archive patch is needed.

1. Close the game and back up your saves.
2. Copy [loadout_randomizer.lua](loadout_randomizer.lua) into the game's existing user-data folder:
   - Windows: `%APPDATA%\kingdom_rush_genesis\`
   - Proton: `<Steam library>/steamapps/compatdata/4259190/pfx/drive_c/users/steamuser/AppData/Roaming/kingdom_rush_genesis/`
3. In Steam → **Properties → General → Launch Options**, use `-custom_script loadout_randomizer`. Keep unrelated launch options. Genesis accepts only one `-custom_script`.

**With [Tower Damage Meter](../TowerDamageMeter/README.md):** put [loadout_with_damage_meter.lua](loadout_with_damage_meter.lua) and [tower_damage_meter.lua](../TowerDamageMeter/tower_damage_meter.lua) alongside `loadout_randomizer.lua`. Use `-custom_script loadout_with_damage_meter` instead of either individual option.

To uninstall, close the game, remove the randomizer scripts, and remove the launch option (or restore `-custom_script tower_damage_meter` to keep the meter). Your last equipped loadout stays in the save.
