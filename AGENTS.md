# Working in this repository

This is a public collection of personal game mods, not a shared application.

## Structure and scope

- Keep each mod self-contained under `Game Name/ModName/`. Preserve existing
  folder names. Source, build files, tests, ignore rules, and documentation belong
  in the mod's folder.
- Keep the root limited to the README, LICENSE, this guidance, `.agents/skills/`,
  and game directories. Update the root mod index when adding or removing a mod.
- Do not add a shared build system, CI workflows, release automation, tracked
  release binaries, or Renovate unless explicitly requested.
- Import source-only snapshots by default. Do not bring in another repository's
  Git history, workflows, releases, generated dependencies, or local artifacts.
  Leave original projects and installed mods untouched unless asked to move or
  remove them. Preserve applicable licenses and credits.

## Licensing

- The repository uses MIT with Commons Clause v1.0, not plain MIT or an
  OSI-approved open-source license. Preserve the combined terms and scope
  exclusions in `LICENSE`; do not advertise the code as unrestricted MIT.
- Include the full combined license when distributing covered mods separately.
  Do not remove exclusions for code of unconfirmed provenance without confirming
  the rights to license it. Requests for sale permissions are not grants.

## Privacy

- Before committing or publishing, use the `auditing-mod-privacy` skill below.
  Review source, documentation, build scripts, metadata, and the actual files to
  be committed, not just the runtime code.
- Never commit credentials, personal or machine-specific paths, private endpoints,
  account identifiers, saves, logs, diagnostic captures, debug symbols, build
  caches, or proprietary game assemblies/assets. Keep generated outputs ignored
  within each mod.
- Use environment variables, existing build properties, and generic path
  placeholders instead of paths tied to a particular user or machine. Test both
  defaults and overrides, including paths with spaces.
- Normal Git author identity and public repository ownership are acceptable.
  Do not change Git identity or rewrite history as part of file sanitization.
- Do not rename existing plugin IDs, config filenames, or save keys merely to
  remove an author-like substring; those are compatibility contracts. Report
  concerns separately and require an intentional migration before changing them.
- If sensitive data is already published, stop further publication and report
  affected paths without reproducing the values. History cleanup and credential
  revocation require a separately authorized response.

## Documentation and verification

- Use `preparing-game-mods` when adding/importing a mod or revising its README.
- Keep mod READMEs short: purpose, requirements/compatibility, build/install,
  controls/configuration, removal, and important limitations. Aim for roughly
  150–300 words, but retain necessary safety and save-format warnings.
- Avoid implementation diaries, local-machine setup narratives, lengthy test
  inventories, and release instructions for infrastructure that does not exist.
  Link to existing developer/test notes when detail is useful.
- Run relevant local builds and existing tests where dependencies are available.
  Never substitute a successful compile or simulated test for an in-game check.
  State missing dependencies and unverified runtime behavior explicitly.
- Do not launch/restart games, install into game folders, or write real saves
  merely to validate an import. Use disposable fixtures; obtain approval for
  live-game changes. Inspect rendered results when changing UI appearance.
- Recheck the final diff and local documentation links. Report separately what
  was edited, verified, committed, and pushed. A request to commit is not a
  request to push; publishing and deleting old repositories need authorization.

## Repository skills

- [Preparing game mods](.agents/skills/preparing-game-mods/SKILL.md)
- [Auditing mod privacy](.agents/skills/auditing-mod-privacy/SKILL.md)
