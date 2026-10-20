# Gloomhaven Party AI

BepInEx mod for **offline Gloomhaven Digital**. Control one mercenary and let AI
play the others, or toggle each mercenary manually. Supports basic movement,
attacks, healing, rests, damage decisions, and selected items and card recovery.

**Experimental:** v0.5.1 still needs in-game validation. Complex abilities and
class mechanics can require manual input. Back up your saves.

## Build and install

Requires a legally installed copy of Gloomhaven Digital, the .NET 8+ SDK, and
[BepInEx 5 (Windows x64)](https://github.com/BepInEx/BepInEx/releases) extracted
beside `GH.exe`. Launch the game once to initialize BepInEx.

On Linux/Steam Deck with Proton, set the Steam launch option:

```text
WINEDLLOVERRIDES="winhttp=n,b" %command%
```

From this mod's directory (`Gloomhaven/GloomhavenPartyAI`):

```sh
dotnet build -c Release -p:GameRoot="/path/to/Gloomhaven"
```

If BepInEx is elsewhere, add `-p:BepInExCore="/path/to/BepInEx/core"`.
Copy `bin/Release/net472/GloomhavenPartyAI.dll` into the game's
`BepInEx/plugins/GloomhavenPartyAI/` directory. Restart the game and check
`BepInEx/LogOutput.log` for the plugin's loaded message.

To uninstall, remove `BepInEx/plugins/GloomhavenPartyAI/`. No prebuilt binaries
or release automation are maintained here.

## Controls and configuration

- Click **AI ON / AI OFF** on a mercenary's initiative portrait to toggle control.
  With a controller, focus the portrait and press Down. Toggles reset each scenario.
- **AI HELP** means manual input is needed; check the BepInEx log for the reason.
- After the first launch, edit `BepInEx/config/com.jsm.gloomhaven.partyai.cfg`.
  Set `Party.HumanCharacter` to a character name or class ID; otherwise the first
  mercenary starts manual. Other settings control delay, logging, items, rests,
  and damage handling. `General.Enabled = false` disables automation.
- Automation is disabled in online games.

## Tests

Run from this mod's directory with the .NET 8 SDK/runtime:

```sh
dotnet run --project tests/RegressionTests.csproj -c Release
```

On a .NET 10-only installation, add `-p:RegressionTargetFramework=net10.0`.
These checks do not run the game. See [test documentation](tests/README.md) for
optional game-reference checks and [development notes](DEVELOPMENT.md) for
diagnostics, known gaps, and manual validation.
