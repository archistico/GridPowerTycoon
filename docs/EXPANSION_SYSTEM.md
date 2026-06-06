# Expansion System

This document records the current map expansion and obstacle feedback rules after Milestone 23.

The expansion loop is built around three concepts: cloud tiles, obstacles and tools.

Cloud tiles hide future map content. Forests and mountains are revealed terrain obstacles that block construction until they are cleared. Axes clear forests. Mines clear mountains.

## Cloud unlock

A cloud tile can be unlocked when it is a valid cloud with hidden terrain and the player has enough money and research.

The unlock preview on the map uses `AreaUnlockSystem.GetUnlockableCloudTiles`, so the preview should match the real unlock result. The preview highlights every cloud tile that will be revealed by the selected unlock.

Preview colors:
- blue means the unlock is currently valid and affordable;
- red means the selected cloud cannot currently be unlocked.

The marker on the selected cloud shows how many tiles will be revealed.

## Cloud properties

When a cloud tile is selected, the properties panel shows:
- `STATE`, which becomes `READY TO UNLOCK` when the unlock is currently possible;
- `ISSUE`, which explains what is missing;
- `REVEAL`, which shows the first hidden terrain type and the number of tiles involved;
- `UNLOCK COST`, which compares current money/research against required money/research.

Typical `ISSUE` values:
- `READY TO REVEAL`;
- `NEED MONEY`;
- `NEED RESEARCH`;
- `NEED MONEY AND RESEARCH`;
- `NOTHING TO REVEAL`.

## Unlock result feedback

After a successful cloud unlock, the status bar summarizes the revealed terrain types.

Example:

```text
AREA UNLOCKED 5: FOREST 1, LAND 3, MOUNTAIN 1
```

The summary is generated from `AreaUnlockResult.RevealedTiles`.

## Terrain obstacles

Forests and mountains block construction.

Forests require axes. Mountains require mines. The selected obstacle preview appears directly on the map:
- green means the player has enough tools to clear;
- red means tools are missing.

Preview badges:
- `A#` means the forest requires that many axes;
- `M#` means the mountain requires that many mines.

## Obstacle properties

When a forest or mountain is selected, the properties panel shows:
- `PURPOSE`, explaining that the tile blocks building and which tool clears it;
- `ISSUE`, explaining whether the tile is ready to clear or how many tools are missing;
- `CLEAR COST`, formatted as available / required tools.

Examples:

```text
ISSUE       NEED 1 AXES
CLEAR COST  2 / 3 AXES
```

```text
ISSUE       READY TO CLEAR
CLEAR COST  4 / 4 MINES
```

## Implementation notes

`AreaUnlockSystem` remains the source of truth for cloud unlock validation and revealed-tile selection.

`TerrainClearSystem` remains the source of truth for terrain clear validation and resource spending.

`MapRenderer` is responsible for:
- cloud unlock preview;
- terrain clear preview.

`UiRenderer` is responsible for:
- cloud property rows;
- obstacle property rows;
- area unlock success/failure messages.

Milestone 23 does not change save-data format.

## Building range overlay

Selecting a built heat converter now shows its operational range on the map.

The range overlay uses Chebyshev distance, which is the same distance model used by the heat conversion systems. This means the displayed range is square-shaped around the converter and should match actual heat coverage.

Visual rules:
- covered cells receive a light range overlay;
- heat producers inside the selected converter range receive a stronger outline;
- the overlay is shown only for built heat converters with `HeatRange > 0`.

This overlay is informational only and does not change heat conversion behavior.

## Heat coverage inverse feedback

Selecting a heat producer highlights active heat converters that cover it.

This is the inverse of the selected-converter range overlay:
- selecting a converter shows its covered cells and covered producers;
- selecting a heat producer highlights the converter or converters that can absorb its heat.

The inverse feedback uses Chebyshev distance, matching the actual heat conversion systems.

## Heat converter placement range preview

When a heat converter building tool is selected, the map previews the future range around the hovered cell.

Rules:
- the preview appears only for heat converter definitions with `HeatRange > 0`;
- valid placement uses the normal range overlay;
- invalid placement uses a red/attenuated overlay;
- the hovered cell shows an `R#` marker where `#` is the heat range.

The preview uses the same visual helper as selected built converters, but it does not change build validation or heat conversion behavior.

## Milestone 23 final state

Milestone 23 makes the map much more readable without changing core gameplay rules.

The final map expansion and range feedback set is:

- cloud unlock preview shows which hidden cells will be revealed;
- selected cloud properties show issue, reveal count and unlock cost;
- successful cloud unlock status messages summarize revealed terrain types;
- selected forest and mountain cells show clear preview and required tools;
- selected obstacle properties show issue and available / required tool counts;
- selected heat converters show operational range;
- heat producers inside the selected converter range are highlighted;
- selected heat producers highlight active converters that cover them;
- heat converter build tools preview their future range while hovering over the map.

The heat range overlays use Chebyshev distance, matching `HeatSystem` and `BuildingOperationalStatusCalculator`.

These features are UI/readability features only. They do not change:
- map data format;
- cloud unlock validation;
- terrain clear validation;
- heat conversion rules;
- range values;
- economy;
- save-data format.
