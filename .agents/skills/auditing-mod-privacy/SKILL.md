---
name: auditing-mod-privacy
description: "Audits mod source, documentation, metadata, and Git candidate contents for personal data and secrets. Use before committing or publishing changes, and when importing mods or checking for data leakage."
---

# Auditing Mod Privacy

Review the files that will be shared, not just whether the mod uploads data.
This is a source/content review, not a guarantee of anonymity or an automatic
Git hook. Normal Git attribution is permitted by the root `AGENTS.md`.

## Define the candidate

1. Read `git status --short` and identify the files and commits intended for
   sharing. Include hidden files and untracked imports. Do not overwrite or stage
   unrelated work.
2. For a commit, inspect `git diff --cached --name-status` and the staged snapshot;
   working-tree-only scans can miss staged values. For a push, inspect every
   outgoing commit relative to the upstream, not only the final tree: a secret
   removed in a later commit is still published in earlier history.
3. Review file names and types as well as content. Use `git ls-files` for the
   tracked inventory and `git ls-files --others --exclude-standard` for candidates
   not yet tracked. Ignore rules do not protect files already tracked.

## Inspect for leakage

Review all newly imported text; for existing files, review changes and surrounding
context. Include READMEs, examples, scripts, project files, package metadata,
fixtures, configuration, and lockfiles. Look for:

- Real names, personal email addresses, usernames, hostnames, absolute home
  directories, drive/network-share paths, and personal SDK/tool locations.
- Tokens, passwords, private keys, connection strings, authenticated URLs,
  private service endpoints, and device/account identifiers.
- Save backups, player data, diagnostics, logs, screenshots, and sample payloads
  containing real data. Review media separately before including it.
- DLLs, executables, debug symbols, build caches/manifests, and archives that can
  embed source paths or bundled game data. Source-only imports should omit them,
  not assume a clean filename means clean contents.
- Network calls, telemetry, remote resources, and upload paths. Distinguish game
  networking APIs and local browser Blob downloads from external data transfer.

Use targeted searches for known local identifiers and generic secret/path
patterns, then inspect findings. Derive known identifiers locally; never write
them into tracked scanner configuration or examples. Report paths and categories
rather than reproducing sensitive values. A clean regex scan alone is not proof.

## Sanitize without breaking compatibility

- Replace machine paths with `$HOME`, MSBuild `$(HOME)`, an existing override, or
  `/path/to/Game` in documentation. Preserve working override behavior.
- Remove private installation narratives and backup locations. Exclude generated
  files and add suitable mod-local ignore rules; do not include real saves as
  tests. Keep original private projects intact unless changes there are requested.
- Preserve third-party credits/licenses. Do not rewrite Git identity or existing
  plugin IDs/config paths/save keys as cosmetic privacy cleanup.
- If credentials or private data appear in already published content, stop and
  report the scope. Do not claim deletion from the current tree removes exposure,
  or rewrite published history/revoke credentials without authorization.

## Verify the exact result

- Rescan the final staged snapshot or outgoing commit range after edits. Confirm
  there are no unintended binaries, saves, logs, nested repositories, or symlinks
  pulling in private files. Check ignore behavior with `git check-ignore`.
- Compare runtime source when only documentation/build paths should change.
  Exercise changed defaults and overrides against disposable paths.
- State the scope inspected, findings removed, validation performed, and anything
  not inspected. Say "no findings in the reviewed content," not "no possible
  leakage." Do not push, rewrite history, or modify installed games as a side
  effect of an audit.
