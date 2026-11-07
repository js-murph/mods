# UE4SS compatibility signatures

The generic scanner in UE4SS `3.0.1-1214-g69f1eba5` times out on Zero Company
Steam build **25898267**, Unreal Engine 5.6 Shipping. This prevents Lua mods
from starting at all. These files override symbol discovery, not game code.
Copy `UE4SS_Signatures/` beside `UE4SS.dll`, **not** inside `Mods/`.

`FName_Constructor.lua`, `StaticConstructObject.lua`, and
`ProcessLocalScriptFunction.lua` are unmodified copies from
[UE4SS's Zero Company configuration](https://github.com/UE4SS-RE/RE-UE4SS/tree/69f1eba586b2cff1cd8043d1a7fc3f3682e19739/assets/CustomGameConfigs/StarWarsZeroCompany).
They retain the upstream [MIT license](UE4SS_Signatures/LICENSE-UE4SS); the
repository's Commons Clause does not replace those third-party terms.

`GUObjectHashTables.lua` and `ConsoleManager.lua` are original additions under
the repository license. The former resolves the PDB-confirmed hash-table getter;
the latter selects one unambiguous `r.DumpingMovie` initializer, following the
[PatternSleuth resolver's contract](https://github.com/trumank/patternsleuth/blob/1d90b02c7610b595f940af04ce97e0f6feb06923/patternsleuth/src/resolvers/unreal/mod.rs).
The console anchor is a function, not the address of `IConsoleManager::Singleton`.

All five patterns were checked for a unique match against executable SHA-256
`aa3ff2e0759c90bae58054f2a68398e5b705fdfd05659242404d8c3602c6f36e`.
Upstream comments identify older builds; their three patterns also match this
build at the expected PDB addresses. In-game logs under Proton subsequently
confirmed successful scanner initialization, the object-hash-table self-test,
Lua mod startup, F8 delivery, and squad-slot assignment/readback. See the
[mod README](../README.md) for current gameplay verification limits.

From the mod directory, rerun the read-only check with Lua and Python installed:

```sh
python3 tests/check_signatures.py "/path/to/SWZeroCompany/Binaries/Win64/SWZeroCompany.exe"
```

Recheck after game/loader updates. No executable, PDB, or extracted assets are
included. To undo, remove these five Lua files only if no other mods need them.
