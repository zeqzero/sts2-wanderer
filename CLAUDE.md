# Wanderer - Slay the Spire 2 Character Mod

A custom character mod for Slay the Spire 2, built with BaseLib. The Wanderer is a samurai-themed character with a stance system, shift/refill mechanics, and dishonor/ofuda curses.

## Project Structure

- `WandererCode/` - C# source code (cards, powers, commands, patches, etc.)
- `Wanderer/` - Godot assets (images, localization, scenes) exported as PCK
- `decompiled/sts2/` - Decompiled base game source for API reference
- `packages/alchyr.sts2.baselib/` - BaseLib modding framework (compiled DLL)

## Reference Projects

These sibling directories contain useful reference material:

- `../WatcherMod/` - Example character mod (WatcherMod) for implementation patterns
- `../ModTemplate-StS2/` - Mod template project
- `../sts2/` - Additional base game reference

## Key Conventions

- Cards extend `WandererCard` and use `[Pool(typeof(WandererCardPool))]`
- Powers extend `WandererPower` (which extends `CustomPowerModel`)
- Localization lives in `Wanderer/localization/eng/` with files matching base game table names (`cards.json`, `powers.json`, `card_keywords.json`, `card_selection.json`, etc.)
- Localization keys are prefixed with `WANDERER-`
- Card images go in `Wanderer/images/card_portraits/`, power images in `Wanderer/images/powers/`
- Harmony patches for extending sealed base game systems live in `WandererCode/Patches/`
- Each card file has `/// <tags></tags>` and `/// <art></art>` comment lines above the class

## Important: Search Scope

Do NOT search outside this project directory. The user's home directory is on a Synology cloud drive and broad searches trigger mass file downloads. Keep all Grep/Glob searches within this project or sibling dirs the user explicitly references.

## Build

```
dotnet build
```

The project targets .NET with Godot 4.5. Analyzer warnings from `LocalizationAnalyzer` are common and non-blocking.
