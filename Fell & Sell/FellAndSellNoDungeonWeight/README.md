# Fell & Sell — No Dungeon Weight Limit

Removes the carrying weight limit inside dungeons. Item weights, stack rules,
equipment bonuses, and inventory contents are unchanged. The weight display
shows the actual load followed by `Unlimited`, with an empty capacity bar.
Works independently of the other Fell & Sell mods.

Built against Fell & Sell 1.8.0 / Unity 6000.3.10f1 and MelonLoader 0.7.3.
**Native gameplay and the rendered weight display remain unverified. Back up
your saves.**

## Build and install

Requires a .NET 6-compatible SDK and MelonLoader's generated IL2CPP assemblies.
See [Auto Collect's setup](../FellAndSellAutoCollect/README.md#build-and-install)
for loader requirements and Proton launch options. From this mod's directory:

```sh
dotnet build NoDungeonWeight.csproj -c Release -p:GameDir="/path/to/Fell & Sell"
```

With the game closed, copy `bin/Release/net6.0/FellAndSell.NoDungeonWeight.dll`
into the game's `Mods/` directory. Enabled by default; no hotkeys are needed.

## Scope and removal

Town and forest limits remain unchanged. Returning with an overweight backpack
restores the normal capacity and may prevent further pickups until you unload.
This mod does not prevent death losses; use Keep Dungeon Loot separately for that.

No custom save data or permanent capacity changes are added. Close the game to
remove `Mods/FellAndSell.NoDungeonWeight.dll`, or set `Enabled = false` under
`[NoDungeonWeight]` in `UserData/MelonPreferences.cfg`. Unload excess weight first.

## Checks

`dotnet run --project tests -c Release -p:GameDir="/path/to/Fell & Sell"` exercises
the actual Harmony patches against simulated game objects. It does not launch
the game, render Unity UI, or verify native hooks and real saves.
