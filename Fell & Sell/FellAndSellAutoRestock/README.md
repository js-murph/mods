# Fell & Sell — Auto Restock Tables

Local MelonLoader 0.7.3 mod for Fell & Sell 1.8.0 / Unity 6000.3.10f1.
Works independently alongside Auto Collect Register Gold.

## Controls

Put an item on a table normally, close the inventory/shelf screen, then look at
that table within normal interaction range:

- **F6** toggles **Always** restocking.
- **F7** toggles **Demand only** restocking: Normal or High demand is allowed;
  Low demand pauses replenishment. It resumes if demand recovers.
- Press the active mode's key again to switch **Off**. Press the other key to
  change modes. Each table starts Off.

The table's interaction text shows its mode and the hotkeys. On multi-slot shelves,
aim at the individual display slot; the mod will not guess a slot from the whole
piece of furniture. An empty, never-configured table cannot be enabled until you
place an item on it. Keyboard controls only.

## Stock and pricing

- Restocks from matching **main backpack inventory only**, not chests, merchants,
  equipped slots, or quick slots. It does not create or buy items.
- Excludes upgraded items and items containing other inventory, since the game's
  shelf representation cannot preserve that metadata.
- Tops up every half-second within the display capacity and size/stack limits.
  If the backpack is empty, it waits for stock instead of disabling the flag.
- Remembers the selected item after the table sells out. Manually displaying a
  different item makes it the new restock target.
- Retains the table's base price and lets the game apply demand/furniture price
  modifiers once. Low demand never removes items already displayed.
- Pauses during saving/loading, paused gameplay, blocked player actions/manual
  inventory UI, shelf mode, and furniture placement.
- Uses native `ShelfSlot.PlaceItem` and `InventoryManager.RemoveItemAmount`.
  Only the accepted quantity is debited. An exception stops automation for that
  session rather than repeatedly attempting a failed transfer.

## Saving flags

**Save the game after changing flags.** Flags, remembered items, and base prices
are stored as JSON under the additional Easy Save key
`FellAndSell.AutoRestock.v1` when the game saves its shelf data. Existing keys and
the original save format are not replaced. The mod does not force an immediate
save when a hotkey is pressed.

The game uses shelf positions rather than persistent table IDs. Flags restore by
scene, slot name, and position (within 5 cm); ambiguous matches stay Off. Picking
up and replacing a table destroys its old instance, so configure it again.
Loading a save without mod data or starting a new game leaves all tables Off.
Ordinary backups made by the game carry the mod key with the rest of the save.

## Installed files and disabling

Installed DLL: `<game>/Mods/FellAndSell.AutoRestock.dll`.
The existing MelonLoader installation and Steam launch options need no changes.
The new DLL is loaded on your next normal game launch, not into a running session.

To disable this mod, close the game and move that DLL out of `Mods`.
Auto Collect Register Gold will continue working. Unused mod save data is harmless;
do not hand-edit your save to remove it.

Diagnostics: `<game>/MelonLoader/Latest.log`, under `Auto_Restock_Tables`.
If the interaction text says `Stopped`, check that log before trying again.

## Verification and limits

Built against the installed game's generated IL2CPP assemblies with zero warnings
or errors. Native disassembly confirmed that `PlaceItem` returns accepted count,
applies modifiers to the supplied base price, and does not debit inventory.

Twenty offline scenarios exercised the actual mod source with real Harmony patches
and simulated game objects: mode changes, per-table isolation, stock accounting,
prices, demand transitions, empty tables, source exclusions, capacity, timing,
pause/load/UI guards, item changes, transfer failures, save/load, and new games.
These do not execute IL2CPP or prove native runtime compatibility.

The game was not launched or interrupted. **Actual in-game restocking, native save
serialization, and the rendered interaction prompt remain untested.** Back up
your saves before installing.

## Rebuild

```sh
dotnet build AutoRestock.csproj -c Release
```

Pass `-p:GameDir="/path/to/Fell & Sell"` for a different installation. Install only
`bin/Release/net6.0/FellAndSell.AutoRestock.dll` into `Mods` while the game is closed.
The register mod's source and binary are unchanged. Game updates may require
rebuilding or revisiting the native method contracts.
