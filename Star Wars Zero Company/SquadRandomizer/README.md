# Squad Randomizer

Prototype squad randomizer for **Star Wars Zero Company**, with a **Randomize**
button and **F8** shortcut. Preserves filled
required slots and fills flexible slots without duplicate recruits. Equipment
stays unchanged; you review the result and launch the mission yourself.

## Compatibility and safety

Targets [UE4SS](https://github.com/UE4SS-RE/RE-UE4SS) `3.0.1-1214-g69f1eba5`
and game build **25898267** (UE 5.6). The generic loader scanner times out;
install the included [compatibility signatures](loader/README.md).
F8 randomization and visible lineup updates are confirmed under Proton.
Runtime logs confirm button creation and subsequent rerolls. **Button layout,
click routing, and mission launch have not been independently verified.**
No game files, loader binaries, or debug symbols are bundled.

Back up saves before testing. The mod changes selection through the game's slot
methods, not direct save editing; normal game saves may retain your selection.
Availability/death/dismissal/critical-injury checks and per-slot requirements
filter candidates. Full mission-specific eligibility remains to be verified.
If no complete squad exists, selection stays unchanged. If assignment fails,
restoration is attempted; inspect the squad manually if restoration also fails.

## Build and install

From this directory, with Lua 5.4+ and Python 3:

```sh
lua tests/run.lua
python3 package.py
```

Output: `dist/SquadRandomizer.zip`, including both applicable licenses.
Override with `--output "/path with spaces/mod.zip"`.

After separately installing UE4SS beside the **inner** executable in
`SWZeroCompany/Binaries/Win64`, extract the ZIP into its active `Mods` directory.
Copy `SquadRandomizer/loader/UE4SS_Signatures` from there into the directory
containing `UE4SS.dll` (normally `Win64/ue4ss`). Add `SquadRandomizer : 1` to
`Mods/mods.txt`, then restart the game. Packaging does not install anything.

## Controls and removal

Click **Randomize** in the lineup controls, or press **F8**, with required slots
filled. The button appears only on the full mission lineup. Do not open
**Edit Squad**: that opens an individual member editor. Back out to the full
lineup before randomizing. Click or press again to reroll; the same squad can
legitimately recur. F8 remains available if button setup fails. Results/errors
go to `UE4SS.log`.
Constrained squads are randomized, not sampled uniformly across all combinations.

To disable, set `SquadRandomizer : 0` and restart. To remove, delete its mod
folder and `mods.txt` entry. Remove the five compatibility Lua files only if no
other mods need them. No custom save format is introduced.
