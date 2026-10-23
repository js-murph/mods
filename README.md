# Mods

A collection of game mods I vibe code: small experiments, quality-of-life
improvements, and other things for my own amusement.

## Mods

- **Fell & Sell:** [Auto Collect](Fell%20%26%20Sell/FellAndSellAutoCollect/),
  [Keep Dungeon Loot](Fell%20%26%20Sell/FellAndSellKeepDungeonLoot/), and
  [No Dungeon Weight Limit](Fell%20%26%20Sell/FellAndSellNoDungeonWeight/) — automate
  register collection, retain loot after dungeon and forest
  deaths, or remove the dungeon carrying weight limit.
- **Gloomhaven Digital:** [Party AI](Gloomhaven/GloomhavenPartyAI/) — offline
  automation for your party's mercenaries.
- **Sephiria:** [Auto-Cast](Sephiria/ActiveAbilityAutoCast/) and
  [Failure Talent Point](Sephiria/FailureTalentPoint/) — toggle automatic abilities
  and earn a bonus Talent point after failed solo runs.
- **Tangy TD:** [Save Editor](Tangy%20TD/SaveEditor/) — a React component for
  editing levels and skill points.

## Layout

The root contains this README, the license, agent guidance (`AGENTS.md` and
`.agents/skills/`), and one directory per game. Each mod lives in its own folder
inside its game's directory:

```text
mods/
├── README.md
├── LICENSE
├── AGENTS.md
├── .agents/skills/
└── Game Name/
    ├── ModName/
    │   ├── README.md
    │   └── ...
    └── AnotherMod/
        ├── README.md
        └── ...
```

## Using the mods

> [!WARNING]
> Use these mods entirely at your own risk, make sure you back up saves etc, etc.

Instructions for installation will be included in the mod Readme.

These mods are maintained on a best effort basis and may be out of date for current versions of games.

## License

[MIT with Commons Clause v1.0](LICENSE). Use (including business
use), modification, and free redistribution are permitted under those terms.
Selling the software, or a product or service whose value derives entirely or
substantially from it, requires separate permission. To request permission,
[open an issue](https://github.com/js-murph/mods/issues); a request alone grants
no additional rights.

Provided as-is, without warranty; the license includes a liability disclaimer,
subject to applicable law. This is source-available software, not OSI open source.
Third-party rights are unchanged.
