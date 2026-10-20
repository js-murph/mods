# Mods

A collection of game mods we vibe code: small experiments, quality-of-life
improvements, and other things that make our games more fun.

## Layout

The root stays simple: this README and one directory per game. Each mod lives in
its own folder inside its game's directory:

```text
mods/
├── README.md
└── Game Name/
    ├── ModName/
    │   ├── README.md
    │   └── ...
    └── AnotherMod/
        ├── README.md
        └── ...
```

Game and mod directories are added as mods are published; the names above are
examples, not bundled mods.

## Adding a mod

- Use the game's recognizable name for its directory and a short, descriptive
  name for the mod's folder.
- Keep each mod self-contained: source, build configuration, documentation,
  tests, and any ignore rules belong in that mod's folder.
- Include a README explaining what the mod does, supported game versions,
  dependencies or mod loaders, installation and removal, how to build it, and
  known limitations.
- Document the mod's license and any third-party credits or permissions before
  sharing code or assets.
- Do not commit game binaries, proprietary assets, save files, credentials, or
  generated build output. Share downloadable builds through GitHub Releases
  when useful, identifying the game, mod, and version.

There is no shared build system or Renovate configuration. Each mod manages its
own tooling and dependency updates.

## Using the mods

Start with the README in the mod's folder. Mods may break when a game updates;
back up your saves and check the game's modding and multiplayer rules before
using them.

Bug reports and improvements are welcome through issues and pull requests.
Include the game, mod, versions, and steps to reproduce a problem. Remove
personal information and credentials from logs before posting them.

This is an unofficial collection, not affiliated with or endorsed by the games'
developers or publishers.
