"""Package Lua sources, loader signatures, documentation, and licenses; never install."""

import argparse
from pathlib import Path
from zipfile import ZIP_DEFLATED, ZipFile


def main():
    source = Path(__file__).resolve().parent
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--output", type=Path, default=source / "dist" / "SquadRandomizer.zip")
    output = parser.parse_args().output
    files = {
        "scripts/main.lua": source / "scripts" / "main.lua",
        "scripts/squad_plan.lua": source / "scripts" / "squad_plan.lua",
        "README.md": source / "README.md",
        "LICENSE": source.parents[1] / "LICENSE",
        "loader/README.md": source / "loader" / "README.md",
    }
    for name in ("FName_Constructor.lua", "StaticConstructObject.lua", "ProcessLocalScriptFunction.lua",
                 "GUObjectHashTables.lua", "ConsoleManager.lua", "LICENSE-UE4SS"):
        relative = "loader/UE4SS_Signatures/" + name
        files[relative] = source / relative
    # Read everything first so missing inputs cannot leave a partial archive.
    contents = {name: path.read_bytes() for name, path in files.items()}
    output.parent.mkdir(parents=True, exist_ok=True)
    with ZipFile(output, "w", compression=ZIP_DEFLATED) as archive:
        for name, data in contents.items():
            archive.writestr("SquadRandomizer/" + name, data)
    print(output)


if __name__ == "__main__":
    main()
