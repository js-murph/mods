# Fell & Sell — Auto Collect Register Gold

Automatically collects ready register payments using the game's normal collection
logic. Pauses during loading and paused gameplay; stops for the session on error.
Does not create gold or edit saves.

Built against Fell & Sell 1.8.0 / Unity 6000.3.10f1 and MelonLoader 0.7.3.
**In-game behavior remains unverified. Back up your saves.**

## Build and install

Requires the .NET 6 SDK (or a compatible SDK), MelonLoader installed in the game,
and its generated IL2CPP assemblies in `MelonLoader/Il2CppAssemblies`.
From this mod's directory:

```sh
dotnet build AutoCollect.csproj -c Release -p:GameDir="/path/to/Fell & Sell"
```

With the game closed, copy `bin/Release/net6.0/FellAndSell.AutoCollect.dll` into
the game's `Mods/` directory. Game and loader assemblies are not bundled.

For Proton, add this Steam launch option, preserving any existing options:

```text
WINEDLLOVERRIDES="version=n,b" %command%
```

Launch the game and check `MelonLoader/Latest.log` for the mod's `Ready.` message.

## Disable

Close the game and remove `Mods/FellAndSell.AutoCollect.dll`, or set
`Enabled = false` under `[AutoCollectRegisterGold]` in
`UserData/MelonPreferences.cfg`. Manual collection remains available.
To bypass MelonLoader entirely under Proton, change the override to `version=b`.
