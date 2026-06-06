# Milestone 27 Persistence Contract

Milestone 27 stabilizes the save/load layer after the Milestone 26 building expansion. The goal is not to add more economy content, but to make longer play sessions safe: old saves should either load predictably, migrate through explicit rules, recover from a backup, or fail with a clear message instead of corrupting the active world.

## Save files

The standard runtime files are:

```text
Saves/savegame.json
Saves/savegame.backup.json
```

`savegame.json` is the preferred save. `savegame.backup.json` is the previous snapshot and is used as fallback when the main file cannot be loaded or restored.

The backup is produced by `SaveGameService.SaveToFile`. The service writes the new save to a temporary file first, copies the current main save to the backup path if it exists, and then replaces the main save. Because this behavior lives in the Core save service, manual save, autosave and offline-progress save all share the same backup policy.

## Versioning

Two version numbers are stored in each save:

```text
SaveGame.CurrentVersion -> save JSON schema version
GameData.CurrentVersion -> runtime data/catalog compatibility version
```

The schema version describes the shape of the save payload. The data version describes compatibility with the current runtime catalogs: buildings, research and upgrades. `RestoreWorld` rejects unsupported versions before reconstructing the world.

## Runtime data is not duplicated

The save stores runtime state, not full definition data. Building definition properties such as energy efficiency bonuses, heat dissipation, maintenance bonuses and tool capacity bonuses remain in the current JSON catalogs. A save stores that a building instance exists; the current catalog defines what that building does after loading.

This keeps balance changes possible without rewriting old save files, while still requiring explicit migration if an id is renamed.

## ID migration

`SaveIdMigrationMap` is the explicit bridge for renamed ids. It can translate:

```text
old building definition id -> current building definition id
old research id            -> current research id
old upgrade id             -> current upgrade id
```

Unknown ids continue to fail when no migration exists. If two saved ids resolve to the same target after migration, restore fails with a duplicate-after-migration error instead of silently applying state twice.

## Integrity validation

`RestoreWorld` validates the save before reconstructing the world. It checks map dimensions, tile count, duplicate and missing tile coordinates, building ids, building footprints, negative lifetime or heat, research ids and upgrade levels. This prevents partially restored invalid worlds.

Malformed JSON is reported as a readable save-load failure. Structurally valid JSON that cannot restore is also treated as a failed save and can trigger backup fallback.

## Dirty state and autosave

`SaveDirtyState` tracks whether the current session has changes after the last clean snapshot. Clean snapshots are created after successful load, save and new game setup. Dirty sessions require a repeated click before `NEW` or `EXIT` proceeds.

`AutoSaveState` is the optional autosave timer. It accumulates elapsed time only while the session is dirty, triggers after the configured interval, and resets after a successful autosave or clean snapshot. Autosave is enabled by default in MonoGame and can be toggled with `F6`.

## Load fallback

`SaveGameService.LoadFromFileWithBackup` tries the main save first. If loading or restore fails, it tries the backup path. The fallback path still uses normal restore validation and id migration, so a backup is not a weaker or special-case format.

When backup fallback succeeds, MonoGame reports:

```text
MAIN SAVE FAILED - BACKUP LOADED
```

If both main and backup fail, startup begins a new game and manual load keeps the current world.

## Final regression coverage

The final regression pass adds `PersistenceRegressionTests`, covering the interactions most likely to regress later:

- repeated writes keep the main save as the latest snapshot and backup as the immediately previous snapshot;
- backup fallback still applies id migration for old building, research and upgrade ids;
- autosave requires fresh dirty time after a trigger before firing again.

Milestone 27 should be considered complete once the full local test suite passes after Step 27I.
