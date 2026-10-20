# Fell & Sell — Auto Collect Register Gold

Local MelonLoader mod built for the installed Fell & Sell 1.8.0, Unity
6000.3.10f1, Windows x64 under Proton (Steam app 4627110).

## Enable on your next launch

The mod and its loader are installed beside the game. In Steam → Fell & Sell →
Properties → General → Launch Options, enter:

```sh
WINEDLLOVERRIDES="version=n,b" %command%
```

If you already have launch options, preserve them and add the environment variable
before `%command%`; merge with an existing `WINEDLLOVERRIDES` value if necessary.
This does not activate the mod in an already-running game. Quit normally whenever
you are finished playing, then launch from Steam.

## Behavior

- Checks the active register every 0.25 seconds during unpaused gameplay.
- Uses `CheckoutQueue.CanCollect()` and `CheckoutQueue.CollectCoins()`, the game's
  normal collection path. It does not directly add gold or edit save files.
- Waits while the game restores a save or scene state.
- Reads the current register each time, so scene transitions do not retain an old
  counter reference.
- Disables automatic collection for the session and logs an error if an operation
  throws. Manual collection remains unpatched.

## Disable and troubleshoot

To disable just this mod, close the game and move
`Mods/FellAndSell.AutoCollect.dll` out of the game's `Mods` folder. Alternatively,
after the first run set `Enabled = false` under `[AutoCollectRegisterGold]` in
`UserData/MelonPreferences.cfg` while the game is closed.

To bypass MelonLoader entirely, use this Steam launch option:

```sh
WINEDLLOVERRIDES="version=b" %command%
```

The loader log is `MelonLoader/Latest.log` under the game folder. Look for
`Auto Collect Register Gold` and the `Ready.` message. Runtime behavior has not
been tested in-game, at your request. If loading fails, bypass the loader using the
option above and retain the log for diagnosis. Back up valuable saves before your
first modded session.

## Installation and verification

Only new mod/loader files were added; the game executable, `GameAssembly.dll`,
original data, and saves were not modified. No process injection, game launch,
restart, Wine installer, prefix mutation, or live test was performed.

Dependencies are MelonLoader 0.7.3, its matching Il2CppInterop libraries, Cpp2IL
2022.1.0-pre-release.21, Unity 6000.3.10 base libraries, and a portable Windows x64
.NET 6.0.36 runtime. The portable runtime lives under
`MelonLoader/Dependencies/dotnet`; no system .NET installation is required.
The interop assemblies and their cache were generated offline from this exact
game build. The loader is configured for offline generation with cached tools.
After a game/Unity update, the mod or cached Unity libraries may need updating.

The Release DLL compiled against the real generated game APIs with zero warnings
or errors. Nine isolated checks of the same mod source passed: ready payment,
readiness/poll timing, pause/resume, both save-restoration flags, inactive counter,
missing/replaced scene singletons, already-collected payment, disabled preference,
and failure shutdown. These checks use simulated game objects; they do not prove
native runtime compatibility or actual in-game behavior.

## Rebuild

With a .NET 6-compatible SDK:

```sh
dotnet build AutoCollect.csproj -c Release
```

For another installation, pass `-p:GameDir="/path/to/Fell & Sell"`. Reference the
loader-generated interop assemblies, not Cpp2IL's dummy DLLs. Nullable annotations
are disabled because the game's generated assemblies contain a conflicting
`NullableAttribute`. Both game names are declared because the PlayerSettings name
is `Fell & Sell`, whereas the loader's `app.info` fallback reads `Fell _ Sell`.

Install only `bin/Release/net6.0/FellAndSell.AutoCollect.dll` into the game's `Mods`
folder while the game is closed. Do not distribute the game's generated assemblies.
