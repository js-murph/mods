# Fell & Sell — Auto Restock Tables

Restocks shop displays from matching items in your main backpack, retaining the
selected item and base price. Does not create or buy stock; upgraded items and
items containing other inventory are excluded.

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

The interaction text shows the current mode. Controls are keyboard-only. Empty
backpacks wait for stock; loading, paused gameplay, and inventory interactions
pause restocking. Replacing a display item changes its restock target.

## Saves and removal

**Save the game after changing modes.** Modes, items, and prices are stored under
the extra save key `FellAndSell.AutoRestock.v1`. Picking up and replacing furniture
requires configuring it again.

To uninstall, close the game and remove `Mods/FellAndSell.AutoRestock.dll`.
Unused mod save data can remain; do not hand-edit your save to remove it.

If the interaction text says `Stopped`, check `MelonLoader/Latest.log` under
`Auto_Restock_Tables` before trying again.
