# Tangy TD Save Editor

A React component for editing the level and Defender, Archer, and Healer
skill-node points in a Tangy TD `backup.cake` save.

## Use

This is standalone JSX source, not a packaged app. Add
`tangy-td-save-editor.jsx` to a React project with JSX support and render its
default export, `TangyTDSaveEditor`.

1. Back up your save and close the game.
2. Open the component in your browser and select or drop a copy of `backup.cake`.
3. Edit the level or expand a class to change its skill points.
4. Download the modified save and replace the game's file only after preserving
   the original backup.

The component reads and downloads files locally; it contains no upload code.
It preserves unedited bytes, so it does **not** anonymize save files.
Compatibility with current game versions is unverified. Never commit your saves.

## License

[MIT with Commons Clause v1.0](../../LICENSE), like the other original code in
this repository. This is the save editor's code, not Tangy TD game source.
