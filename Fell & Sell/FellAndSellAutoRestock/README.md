# Fell & Sell — Auto Restock Tables

Restocks shop displays from matching items in your main backpack first, then
storage chests in the same loaded scene, retaining the selected item and base
price. Does not create or buy stock; upgraded items and items containing other
inventory are excluded. Dungeon loot chests and packed-up chests are not used.

Built against Fell & Sell 1.8.0 / Unity 6000.3.10f1 and MelonLoader 0.7.3.
**In-game restocking and save persistence remain unverified. Back up your saves.**

## Build and install

Requires a .NET 6-compatible SDK and MelonLoader with its generated IL2CPP
assemblies. See [Auto Collect's setup](../FellAndSellAutoCollect/README.md#build-and-install)
for loader requirements and Proton launch options; that mod is not required.
From this mod's directory:

```sh
dotnet build AutoRestock.csproj -c Release -p:GameDir="/path/to/Fell & Sell"
```

With the game closed, copy `bin/Release/net6.0/FellAndSell.AutoRestock.dll` into
the game's `Mods/` directory, then launch normally.

## Controls

Place an item on a display, close the inventory/shelf screen, and look at its slot
within interaction range:

- **F6:** always restock.
- **F7:** restock only at Normal or High demand.
- Press the active mode's key again to turn it off. All slots start off.

The contextual action menu shows separate **F6** and **F7** rows with **ON/OFF**
status; the enabled mode is highlighted. ON means the mode is enabled, not that
stock is currently available. Empty unconfigured displays say **Place an item
first**. Controls are keyboard-only and require interaction hints to be enabled.
Both modes use chests automatically, with no distance limit within the scene.
Restocking waits when neither source has stock, and pauses during loading,
paused gameplay, inventory/chest interactions, or furniture placement. Replacing
a display item changes its restock target.

## Saves and removal

**Save the game after changing modes.** Modes, items, and prices are stored under
the extra save key `FellAndSell.AutoRestock.v1`. Picking up and replacing furniture
requires configuring it again.

To uninstall, close the game and remove `Mods/FellAndSell.AutoRestock.dll`.
Unused mod save data can remain; do not hand-edit your save to remove it.

If the menu says `Stopped`, check `MelonLoader/Latest.log` under
`Auto_Restock_Tables` before trying again. The updated menu layout has not yet
been checked in-game.
