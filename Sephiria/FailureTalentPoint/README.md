# Failure Talent Point

A Sephiria add-on that grants one persistent bonus Talent point after each normal
failed **solo** run, including surrender. Victories and scripted fake defeats do
not count. Awards start after installation; the add-on does not run in multiplayer.

Built against Sephiria 1.0.23 (Steam build 24548451), using its built-in add-on API.
Game updates may require rebuilding. Back up your saves before installing.

## Build and install

Requires the .NET 8+ SDK and a local game installation. From this mod's directory:

```bash
export SEPHIRIA_GAME_DIR="/path/to/Sephiria"
./build.sh
```

With Sephiria closed, copy the built `package/` contents into the game's
`AddOns/FailureTalentPoint/` directory. It must contain `metadata.json` and
`FailureTalentPoint.dll`. `DOTNET_BIN` can select a different SDK executable.

Launch Sephiria and check `Player.log` for the add-on's loaded message.

## Saves and removal

The bonus is stored in the current profile under `FailureTalentPoint_Bonus` and
`FailureTalentPoint_LastAwardedRetry`. Normal Talent purchases are unchanged.

To uninstall, close the game and remove `AddOns/FailureTalentPoint/`. The extra
points stop applying; the mod's saved values remain in the profile.
