# Active Ability & Grimoire Auto-Cast

A Sephiria add-on that lets active abilities and single-shot grimoire spells be
toggled to cast automatically whenever they are ready.

## Controls

- Hold `Alt` and press `1` through `8` to toggle the active ability or grimoire
  currently assigned to that quick slot.
- Alternatively, middle-click its quick-slot icon.
- On an Xbox-layout controller, hold `LT` + `RT`, then press the normal magic
  slot button:
  - `A`, `B`, `X`, `Y` toggle slots 1, 2, 3, 4.
  - D-pad `Down`, `Right`, `Up`, `Left` toggle slots 5, 6, 7, 8.
- A green square on the icon means auto-cast is enabled.
- Repeat the toggle to turn it off.

The controller chord deliberately mirrors Sephiria's existing `LT` magic-slot
layout. Because the game also receives that slot press, toggling may cast the
selected action once.

The toggle follows the ability or grimoire if you move it to another slot.
Toggles last until Sephiria is closed or the add-on is unloaded.

## Behaviour

- Uses Sephiria's normal quick-slot cast path, aim marker, auto-aim target, and
  server command.
- Casts active abilities only when their cooldown indicator reports ready.
- Casts single-shot grimoire spells only when their charge, MP, spell cooldown,
  character casting state, and global cooldown permit it.
- Charged grimoires are rejected with an on-screen message; automatically
  holding and releasing charge spells is intentionally not implemented yet.
- Pauses while the game is paused, the character is dead, or character input
  is blocked by a menu.
- Retries at most four times per second if an ability is ready but its own
  requirements are not met.
- Does not automate weapon attacks or weapon skills.

## Install

1. Exit Sephiria.
2. Build the add-on (see below), then run:

   ```bash
   ./install.sh
   ```

   Or copy the entire `package` directory manually to:

   `<Sephiria installation>/AddOns/ActiveAbilityAutoCast`

3. Start Sephiria.
4. Check `Player.log` for:

   `[AddOnLoader] ✓ 'Active Ability & Grimoire Auto-Cast'`

The installed folder must contain both `metadata.json` and
`ActiveAbilityAutoCast.dll`.

## Uninstall

Exit Sephiria and remove its `AddOns/ActiveAbilityAutoCast` directory. The
add-on does not alter save data.

## Build

The project targets Sephiria's Unity/Mono `netstandard2.1` runtime. With a
.NET 8 or newer SDK installed:

```bash
./build.sh
```

Set `SEPHIRIA_GAME_DIR` if Sephiria is installed in a different Steam library,
or set `DOTNET_BIN` to select a particular `dotnet` executable.

## Compatibility

Developed against Sephiria 1.0.23, Steam build 24614196. The game's
`HorayModAPI` is marked as work in progress, so future updates may require a
rebuild or small adaptation.

Version 1.2.0 adds an Xbox-layout controller toggle chord. Version 1.1.0 added
safe auto-casting for single-shot grimoire spells. Version 1.0.1 corrected the
package target to `netstandard2.1`, which Sephiria's add-on loader requires.
