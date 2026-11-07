"""Check loader patterns against a local executable without running the game."""

import argparse
import hashlib
import os
from pathlib import Path
import re
import struct
import subprocess


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("executable", type=Path)
    data = parser.parse_args().executable.read_bytes()
    # Independent references from build 25898267's PDB, except ConsoleManager:
    # that is PatternSleuth's r.DumpingMovie initializer anchor, verified below.
    expected = {
        "FName_Constructor": 0x0436D550,
        "StaticConstructObject": 0x045E5360,
        "ProcessLocalScriptFunction": 0x045B8290,
        "GUObjectHashTables": 0x045F92C0,
        "ConsoleManager": 0x01BA0220,
    }
    digest = hashlib.sha256(data).hexdigest()
    if digest != "aa3ff2e0759c90bae58054f2a68398e5b705fdfd05659242404d8c3602c6f36e":
        raise SystemExit("Unknown executable build; do not assume these signatures are compatible.")

    pe = struct.unpack_from("<I", data, 0x3C)[0]
    section_count = struct.unpack_from("<H", data, pe + 6)[0]
    optional_size = struct.unpack_from("<H", data, pe + 20)[0]
    sections = {}
    for index in range(section_count):
        offset = pe + 24 + optional_size + index * 40
        name = data[offset:offset + 8].rstrip(b"\0").decode()
        _, rva, size, raw = struct.unpack_from("<IIII", data, offset + 8)
        sections[name] = (rva, raw, size)
    text_rva, text_raw, text_size = sections[".text"]
    code = data[text_raw:text_raw + text_size]
    signatures = Path(__file__).resolve().parents[1] / "loader" / "UE4SS_Signatures"

    for name, expected_rva in expected.items():
        # Execute the real scanner callbacks instead of duplicating the patterns.
        script = "dofile(os.getenv('SIGNATURE_FILE')); print(Register()); print(OnMatchFound(4096))"
        output = subprocess.check_output(
            ["lua", "-e", script], text=True,
            env={**os.environ, "SIGNATURE_FILE": str(signatures / (name + ".lua"))},
        ).splitlines()
        pattern = b"".join(
            b"." if token == "?" else re.escape(bytes.fromhex(token))
            for token in output[0].split()
        )
        matches = [text_rva + match.start() for match in re.finditer(pattern, code, re.DOTALL)]
        if matches != [expected_rva] or output[1] != "4096":
            raise SystemExit(f"FAIL {name}: matches={matches!r}, callback={output[1]!r}")
        print(f"PASS {name}: unique match at RVA 0x{expected_rva:08X}")

    # Check the selected console initializer actually references the resolver's
    # anchor string, rather than merely matching some unrelated function prologue.
    anchor_rva = 0x01BA042A
    anchor_raw = text_raw + anchor_rva - text_rva
    if data[anchor_raw:anchor_raw + 3] != b"\x48\x8d\x15":
        raise SystemExit("Console initializer is missing its LEA instruction")
    target = anchor_rva + 7 + struct.unpack_from("<i", data, anchor_raw + 3)[0]
    literal = "r.DumpingMovie\0".encode("utf-16le")
    for rva, raw, size in sections.values():
        if rva <= target < rva + size:
            start = raw + target - rva
            if data[start:start + len(literal)] == literal:
                print("PASS ConsoleManager anchor: r.DumpingMovie")
                break
    else:
        raise SystemExit("Console initializer does not reference r.DumpingMovie")
    print("Offline checks passed; loader startup still requires an in-game test.")


if __name__ == "__main__":
    main()
