---
name: preparing-game-mods
description: "Prepares source-only game mod imports and concise user documentation. Use when adding or migrating a mod, or writing or shortening a mod README in this repository."
---

# Preparing Game Mods

Make a mod self-contained and understandable without importing local baggage.
Follow the root `AGENTS.md`; use `auditing-mod-privacy` before committing.

## Import or update

1. Inspect the source tree, README, build files, scripts, metadata, and relevant
   guidance. Check Git status before editing either checkout. Identify the game,
   loader/runtime, required local dependencies, and source provenance.
2. Identify a destination under `Game Name/ModName/`. Keep originals and installed
   mods unchanged. Copy an explicit set of source files rather than whole local
   directories. Preserve licensing/attribution; flag uncertain authorship rather
   than claiming ownership or inventing a license.
3. Exclude `.git`, CI/release configuration, packaged binaries, build outputs,
   caches, logs, saves, and third-party game/loader assemblies. Retain source tests
   and relevant developer documentation. Add mod-local ignore rules for outputs
   and data that its tooling produces.
4. Keep runtime code unchanged during a migration unless a change is necessary.
   Compare copied source with the original. Replace personal paths with portable
   defaults or existing configuration inputs; do not invent a shared framework.
5. Check nested-project paths, build outputs, packaging scripts, and documentation
   references after relocation. Use an explicit game path in documented commands;
   quote paths that may contain spaces. Never run an installer against the real
   game as a side effect of checking a build script.

## Write the mod README

Use the Gloomhaven Party AI README as a concise example, not as a dependency or
template to copy verbatim. Adapt the following content to the actual mod:

- One short paragraph describing what it does.
- Required game/loader/runtime versions and consequential limitations.
- Commands run from this mod's directory, required dependencies, and the exact
  output to install. Do not imply binaries, a loader, or an app are bundled if
  only source is present.
- Essential controls/settings, including how to disable or uninstall it.
- Save behavior, backup advice, and known verification gaps where relevant.

Aim for roughly 150–300 words. Omit headings that add no information. Remove
machine-specific histories and detailed implementation/test narration; link to
existing notes if necessary. Keep warnings that affect safe use, even if they
make the README longer. A local-only save editor is not a save anonymizer.

Update the root README's mod index. Percent-encode spaces and ampersands in link
destinations. Preserve the user's existing root README wording and disclaimer.

## Validate and hand off

- Build and run existing tests when dependencies are available. Do not distribute
  game assemblies to make a build self-contained. Record unavailable checks.
- Exercise changed path defaults and explicit overrides, including spaces and
  shell metacharacters. Use a disposable destination for installer checks.
- Check local links/anchors and command paths. Remove temporary fixtures and
  generated outputs created for validation.
- Load `auditing-mod-privacy` and review the exact commit/publication candidate.
- Report what was imported, validation performed, and remaining limitations.
  Preserve the distinction between local work, a commit, and a push. Do not
  delete/archive the original remote or publish releases as part of an import
  without explicit authorization.
