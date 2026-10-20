# Failure Talent Point

A small Sephiria add-on that grants one permanent Talent point after each
normal failed solo run.

## Behaviour

- Counts ordinary defeat and manual surrender (`victoryType == 0`).
- Does not count victories or story-scripted "fake defeats".
- Starts counting failures after the mod is installed; it does not grant points
  retroactively.
- Works only in solo sessions. It deliberately disables itself in multiplayer
  to avoid network and save desynchronisation.
- Stores its bonus in two namespaced values in the current Sephiria profile:
  `FailureTalentPoint_Bonus` and `FailureTalentPoint_LastAwardedRetry`.
- Does not modify Sephiria's DLLs or its normal Talent-tree purchases.

## Install

1. Exit Sephiria.
2. Build the add-on (see below), then copy the entire `package` directory to:

   `<Sephiria installation>/AddOns/FailureTalentPoint`

3. Start Sephiria.
4. Check `Player.log` for:

   `[AddOnLoader] ✓ 'Failure Talent Point'`

The packaged folder must contain both `metadata.json` and
`FailureTalentPoint.dll`.

## Uninstall

Exit Sephiria and remove its `AddOns/FailureTalentPoint` directory. The two
namespaced save values remain harmlessly in the save, but the extra Talent
points are no longer applied.

## Build

The project targets Sephiria's Unity/Mono `netstandard2.1` runtime. With the
.NET 8 SDK installed:

```bash
./build.sh
```

Set `SEPHIRIA_GAME_DIR` if the game is installed in a different Steam library,
or set `DOTNET_BIN` to select a particular `dotnet` executable.

## Compatibility

Developed against Sephiria 1.0.23, Steam build 24548451. The game's
`HorayModAPI` describes itself as work in progress, so future game updates may
require rebuilding or adapting this add-on.
