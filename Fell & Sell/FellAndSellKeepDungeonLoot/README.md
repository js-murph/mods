# Fell & Sell — Keep Dungeon Loot

Keeps your backpack, equipped items, quick-slot items, and gold when you die in a
dungeon or the forest, including existing item upgrade levels and stored contents.
Death, the return to town, and the normal save still happen. Works independently
of the other Fell & Sell mods.

Built against Fell & Sell 1.8.0 / Unity 6000.3.10f1 and MelonLoader 0.7.3.
**In-game death/return/save behavior remains unverified. Back up your saves.**

## Build and install

Requires a .NET 6-compatible SDK and MelonLoader's generated IL2CPP assemblies.
See [Auto Collect's setup](../FellAndSellAutoCollect/README.md#build-and-install)
for loader requirements and Proton launch options. From this mod's directory:

```sh
dotnet build KeepDungeonLoot.csproj -c Release -p:GameDir="/path/to/Fell & Sell"
```

With the game closed, copy `bin/Release/net6.0/FellAndSell.KeepDungeonLoot.dll`
into the game's `Mods/` directory. No hotkeys are needed; protection is enabled
by default.

## Scope and removal

Protection applies to dungeon and forest deaths. It does not recover
items already consumed, sold, dropped, or lost before installation, or collect
loot left on the ground. Dungeon progress and temporary effects still reset
normally. New games and ordinary inventory/gold changes remain unaffected.

No custom save data is added. Retained items use the game's normal save format.
Close the game to remove `Mods/FellAndSell.KeepDungeonLoot.dll`, or set
`Enabled = false` under `[KeepDungeonLoot]` in `UserData/MelonPreferences.cfg`.

## Checks

`dotnet run --project tests -c Release -p:GameDir="/path/to/Fell & Sell"` tests
the actual mod patches with Harmony and simulated game objects. It does not
launch the game or verify native IL2CPP hooks, scene transitions, or real saves.
