# Milestone 27 Status

Milestone 27 focuses on save stability, persistence feedback and quality-of-life around loading and exiting. It starts only after Milestone 26 has closed the new-building expansion phase.

## Current state

| Step | Feature | Status | Role |
|---|---|---:|---|
| 27A | Save compatibility check for new building properties | Completed | Persistence safety after Milestone 26 |
| 27B | Save integrity validation and corrupted JSON handling | Completed | Guard against broken or inconsistent saves |
| 27C | Visible save/data version information | Completed | Player/debug visibility |
| 27D | Safe handling for missing/renamed ids in old saves | Completed | Migration strategy |
| 27E | Confirm NEW and EXIT when dirty | Completed | Avoid accidental loss |
| 27F | Optional autosave | Completed | Recovery safety |
| 27G | Backup save on write | Completed | Recovery safety |
| 27H | Load fallback from backup | Completed | Recovery safety |
| 27I | Final persistence regression pass | Prepared | Release-candidate confidence |

## Step 27A result

Step 27A keeps the save schema version at `1` and introduces `SaveGame.CurrentVersion` so the current format is not repeated as a magic number. The save payload remains focused on persistent runtime state: resources, map tiles, building instances, completed research and upgrade levels.

The four Milestone 26 building-definition properties are deliberately not serialized into saves:

| Property | Reason |
|---|---|
| `EnergyEfficiencyBonus` | Loaded from the current building catalog |
| `HeatDissipationPerSecond` | Loaded from the current building catalog |
| `MaintenanceEfficiencyBonus` | Loaded from the current building catalog |
| `ToolCapacityBonus` | Loaded from the current building catalog |

This means balancing can still be changed in JSON without rewriting old saves. A save stores that a `substation`, `heat_sink`, `maintenance_center` or `tool_warehouse` exists; the active runtime data decides what those buildings do after loading.

## Regression coverage added

`SaveCompatibilityTests` now covers these cases:

- save schema version uses `SaveGame.CurrentVersion`;
- Milestone 26 definition-only properties are not duplicated into save JSON;
- restored support buildings still activate their definition-driven bonuses;
- 3x3 buildings such as `nuclear_reactor` preserve their full footprint after save/restore;
- saves referencing unknown building definitions fail explicitly;
- saves referencing unknown research ids or upgrade ids fail explicitly.

## Production change

`SaveGameService.RestoreWorld` now validates saved building, research and upgrade ids against the supplied `GameData` before reconstructing the world. This avoids silent restoration of orphan ids that later systems would skip or interpret inconsistently.

This is not a full migration layer yet. It is an explicit compatibility guard. A later migration step should decide whether renamed or removed ids should be mapped, ignored with warning, or rejected depending on save version.

## Step 27B result

Step 27B strengthens save validation before a world is reconstructed. `SaveGameService.RestoreWorld` now checks map integrity, building instance integrity, building footprint consistency, completed research duplication and upgrade level limits. This prevents corrupted saves from reaching later systems in a partially valid state.

The new validation covers:

- duplicate or missing map tile coordinates;
- tiles outside map bounds;
- duplicate or empty building ids;
- empty building definition ids;
- negative building lifetime or accumulated heat;
- building footprints outside the map;
- building footprints not fully linked by map tiles;
- tiles that point to a building but lie outside that building footprint;
- empty or duplicate completed research ids;
- empty, negative or over-max upgrade levels.

`LoadSaveFromFile` now wraps malformed JSON in a readable `InvalidOperationException` instead of leaking a raw JSON parser failure. MonoGame startup and manual load also catch failed save loading: startup falls back to a new game with a status message, while manual load keeps the current world and shows `SAVE LOAD FAILED`.

`SaveIntegrityTests` was added to lock the most important corruption cases: duplicate tiles, incomplete footprints, extra footprint references, over-max upgrade levels and corrupted JSON.

## Step 27C result

Step 27C makes the persistence contract visible both in code and in the runtime UI. `GameData.CurrentVersion` now sits beside `SaveGame.CurrentVersion`, and new saves store both `Version` and `DataVersion`. This keeps save-schema compatibility and data-catalog compatibility separate: a future migration can decide whether the JSON save format changed, whether the data ids changed, or both.

`SaveGameService` now creates a compact `SaveGameSummary` and can load a summary from disk. The MonoGame status bar displays a compact string such as `SAVE V1 DATA V1 LAST 2026-06-06 14:30 UTC`, or `SAVE V1 DATA V1 UNSAVED SESSION` for a fresh game. The value is updated after startup load, manual save, manual load, offline-progress autosave and new game.

Restore now rejects saves with an unsupported data version explicitly, using the same early-validation style introduced in 27A and 27B.

Regression coverage added:

- saves expose both save version and data version;
- `SaveGameSummary` formats a compact player/debug status string;
- unsupported data versions fail before world reconstruction.

## Next step

Recommended next step:

```text
Step 27D - Safe handling for missing/renamed ids in old saves
```

## Step 27D result

Step 27D adds the first explicit save-id migration layer. It does not rename any current runtime ids; instead it introduces `SaveIdMigrationMap`, which can translate old save ids to current catalog ids during restore.

The migration layer currently covers the three persistent id groups saved by the game:

| Saved id type | Migration target |
|---|---|
| building definition id | current `BuildingCatalog` id |
| completed research id | current `ResearchCatalog` id |
| upgrade id | current `UpgradeCatalog` id |

`SaveGameService` now accepts an optional migration map. During restore, it validates saved ids after applying the migration and then reconstructs the world with the current ids. Unknown ids still fail explicitly when no migration exists. If an old id and a current id resolve to the same target in the same save, restore fails with a clear duplicate-after-migration error instead of silently double-applying research or upgrades.

Regression coverage added in `SaveIdMigrationTests`:

- renamed building definition ids restore as the current building id;
- renamed research ids restore as completed current research;
- renamed upgrade ids restore with the current upgrade level;
- research collisions after migration fail explicitly;
- upgrade collisions after migration fail explicitly;
- unknown ids without a migration still use the existing explicit failure path.

This step makes future content cleanup safer. The actual project data remains unchanged: no building, research or upgrade id was renamed in this step.

## Step 27E result

Step 27E adds a small dirty-state guard around destructive session actions. The game now tracks whether the current run has changed since the last clean snapshot. A clean snapshot is created after startup restore, manual save, manual load, offline-progress autosave and explicit new game. Successful player actions such as build, sell, research, upgrade, replace, demolish, terrain clear and cloud unlock mark the session as modified. Passive simulation time also marks the session as modified after a short grace interval, so idling resources are not silently ignored.

`NEW` and `EXIT` now use a two-step confirmation when the current session is dirty. The first click shows an explicit warning in the status area. Repeating the same action confirms it. Exit keeps the existing safe behavior of saving before closing, but accidental exits are no longer immediate. The status bar appends `MODIFIED` to the compact save/data string while the current session has unsaved changes.

The confirmation policy lives in `SaveDirtyState`, a small Core class covered by `SaveDirtyStateTests`. The MonoGame layer only translates the result into UI messages and actions.

## Step 27F result

Step 27F adds a small optional autosave policy around the dirty-state work from 27E. `AutoSaveState` lives in Core and owns the timer logic: it accumulates time only while the current session is dirty, triggers after the configured interval, resets when the session becomes clean, and does nothing while autosave is disabled.

MonoGame enables autosave by default with a 60-second interval. When the session is `MODIFIED` for long enough, the game writes `Saves/savegame.json`, marks the session clean, refreshes the compact save/data status string and shows `AUTOSAVED`. Manual save, manual load, explicit new game and offline-progress saving all reset the autosave timer through the normal clean-snapshot path.

Autosave can be toggled at runtime with `F6`. The status message reports `AUTOSAVE ON` or `AUTOSAVE OFF`. This keeps the feature optional without adding a settings screen yet.

Regression coverage added in `AutoSaveStateTests`:

- dirty time below interval does not autosave;
- reaching the interval triggers exactly one autosave decision and resets the timer;
- clean sessions reset the timer;
- disabled autosave skips saving and does not accumulate time;
- toggling the enabled state resets the timer.

## Next step

Recommended next step:

```text
Step 27I - Final persistence regression pass
```

## Step 27G result

Step 27G adds backup-on-write behavior to `SaveGameService.SaveToFile`. The save service now writes the new JSON to a temporary file first, copies the existing main save to a sibling backup file, and only then replaces the main save. The backup path follows the fixed naming rule `savegame.backup.json` for the normal `savegame.json` file.

This is implemented at the Core save-service level, so every existing save path benefits from the same behavior without duplicating logic in MonoGame: manual save, autosave and the offline-progress save after startup all use the backup writer. A first save does not create a backup because there is no previous valid main save to preserve.

Regression coverage added in `SaveGameServiceTests`:

- first write creates only the main save;
- second write creates `savegame.backup.json`;
- the main save contains the latest snapshot;
- the backup contains the previous snapshot;
- `GetBackupPath` keeps the `.backup` suffix before the `.json` extension.

## Next step

Recommended next step:

```text
Step 27I - Final persistence regression pass
```


## Step 27H result

Step 27H completes the backup-save loop started in 27G. `SaveGameService` now exposes `LoadFromFileWithBackup`, which attempts to load and restore the main save first and then falls back to the sibling backup file if the main save cannot be used. The fallback applies to both malformed JSON and structurally invalid saves, because it wraps the full load-and-restore path rather than only deserialization.

The standard MonoGame path now behaves as follows:

```text
Saves/savegame.json          -> preferred file
Saves/savegame.backup.json   -> fallback file
```

At startup, if the main save fails but the backup is valid, the game starts from the backup and reports `MAIN SAVE FAILED - BACKUP LOADED`. Manual load uses the same Core path and reports the same message. If neither file can be used, startup still starts a new game with the existing failure message, while manual load keeps the current world and reports `SAVE LOAD FAILED`.

Regression coverage added in `SaveGameServiceTests`:

- corrupted main JSON restores from backup;
- readable but invalid main save restores from backup;
- missing main save can still restore from backup;
- failure of both main and backup is reported explicitly.

Recommended next step:

```text
Step 27I - Final persistence regression pass
```


## Step 27I result

Step 27I is the final persistence regression pass for Milestone 27. It does not change gameplay behavior. Its purpose is to lock the interactions between the persistence features added from 27A through 27H so that future gameplay work can continue on top of a stable save layer.

Regression coverage added in `PersistenceRegressionTests`:

- repeated save writes keep the current `savegame.json` as the latest snapshot and rotate `savegame.backup.json` to the immediately previous snapshot;
- backup fallback still applies the `SaveIdMigrationMap`, so an old backup with renamed building, research and upgrade ids can be restored through the same migration path as a normal main save;
- `AutoSaveState` does not fire repeatedly from one threshold crossing and requires fresh dirty time before the next autosave decision.

The final Milestone 27 persistence contract is now:

```text
SaveGame.CurrentVersion -> JSON save schema version
GameData.CurrentVersion -> runtime data/catalog compatibility version
savegame.json           -> preferred save file
savegame.backup.json    -> previous snapshot and fallback file
SaveIdMigrationMap      -> explicit id rename bridge for old saves
SaveDirtyState          -> NEW/EXIT dirty confirmation policy
AutoSaveState           -> optional autosave timing policy
```

Milestone 27 can be considered complete after local `dotnet test` passes. The next recommended milestone is UX and gameplay feedback, because the persistence foundation is now strong enough to support longer play sessions.

Recommended next step:

```text
Milestone 28A - Better build/research/upgrade feedback messages
```


## Step 27J result

Step 27J fixes a small but important menu-strip consistency issue before moving to Milestone 28. The UI previously kept the real HELP hit-test rectangle between VIEW and EXIT, but the menu strip no longer drew that button in the right command group. This created an invisible clickable area near EXIT that opened the help panel.

The command strip now has a single visible HELP entry in the existing section beside BUILD / RESEARCH / UPGRADE, while the right command group is limited to session actions: NEW, LOAD, SAVE, VIEW and EXIT. EXIT now sits immediately after VIEW, so there is no hidden HELP hotspot between VIEW and EXIT.

Recommended next step:

```text
Milestone 28A - Better build/research/upgrade feedback messages
```
