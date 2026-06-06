# Feedback System

This document records the current player feedback rules used by GridPower Tycoon after Milestone 21.

The goal of the feedback system is simple: when something is not working, the player should understand why without guessing the internal rules.

## Main feedback surfaces

The game currently communicates operational state through four surfaces.

The map gives immediate visual feedback. Buildings can show compact badges for critical states, so the player can scan the grid without opening every properties panel.

The properties panel gives detailed feedback for the selected cell or building. It includes the building state, the `ISSUE` row, energy/heat/research/money values, lifecycle data, action availability and heat risk.

The status bar gives global and contextual messages. It shows recent action results, save/load feedback, demolish confirmation and selected-building status summaries.

The command/left panel gives action availability for build, research and upgrade cards. Cards explain whether the item is ready, locked, completed or missing resources.

## Map badge legend

Map badges are intentionally short. They are designed to be readable at a glance.

| Badge | Meaning | Typical fix |
| --- | --- | --- |
| `E` | No energy | Add production, storage or wait for energy. |
| `G` | Generator/conversion missing | Place a heat converter in range. |
| `H` | Heat warning | Add or upgrade heat conversion before explosion. |
| `T` | Timed out / expired | Replace the building or unlock management. |
| `X` | Exploded | Restore or demolish the building. |

The same legend appears in the status bar when there is enough horizontal space.

## Properties panel `ISSUE` row

The `ISSUE` row is the selected building's short operational explanation. It is not a second state label; it is an actionable interpretation of the state.

Examples:

| State | ISSUE text |
| --- | --- |
| Active producer | `PRODUCING ENERGY` |
| Active research | `PRODUCING RESEARCH` |
| Active automation | `SELLING ENERGY` |
| Active battery | `STORING ENERGY` |
| Active converter | `ABSORBING HEAT` |
| Active heat producer with converter | `HEAT CONVERSION OK` |
| Missing energy | `NEEDS STORED ENERGY` |
| Missing heat conversion | `PLACE GENERATOR IN RANGE` |
| Heat warning | `HEAT ABOVE WARNING` |
| Expired | `REPLACE OR MANAGE` |
| Exploded | `RESTORE OR DEMOLISH` |

## Heat feedback

Heat feedback is shown in multiple places because it is a major gameplay mechanic.

The properties panel shows:
- `HEAT OUT`, the building's heat production;
- `HEAT STORED`, the current accumulated heat against the explosion threshold;
- `HEAT RISK`, the practical risk message;
- `HEAT IN`, for converters;
- `HEAT TO ENERGY`, for conversion output.

`HEAT RISK` can report:
- `CONTROLLED`;
- `EXPLODES IN X S`;
- `RISK LOW`;
- `NO NEW HEAT`;
- `EXPLOSION`.

The map also shows badge `H` for warning and `G` when a heat producer lacks conversion.

## Status bar priority

The status bar should avoid random message overwrites. Current intended priority is:

1. Demolish confirmation.
2. Save/load message.
3. Latest action result or failure.
4. Selected-building operational summary.
5. Default build/select prompt.

This keeps high-risk or explicit user actions visible while still making the selected building understandable during normal play.

## Failure message style

Failure messages should be short, direct and actionable.

Use this style:

```text
BUILD FAILED: NEED MONEY
BUILD FAILED: CELL OCCUPIED
RESEARCH FAILED: MISSING PREREQUISITE
CLEAR FAILED: NEED AXES
UNLOCK FAILED: NEED RESEARCH
UPGRADE FAILED: MAX LEVEL
```

Avoid showing raw enum names such as `NotEnoughMoney`, `TileAlreadyOccupied` or `MissingPrerequisite` directly to the player.

## Implementation notes

The map badges and properties panel use `BuildingOperationalStatusCalculator`, so they should remain consistent.

`UiRenderer` is currently responsible for:
- properties panel `ISSUE`;
- selected-building status bar summaries;
- status badge legend;
- clear action failure messages.

`MapRenderer` is currently responsible for:
- map building badges;
- heat/lifetime bars;
- expired/exploded visual markers.

No save-data schema is involved in these feedback features.

## Manager badge

Milestone 22 adds a compact `M` badge on the map for buildings currently covered by an unlocked manager.

The `M` badge is separate from operational problem badges:
- `M` means automatic renewal is available for that building type;
- `E/G/H/T/X` still describe operational problems or lifecycle states.

A managed building can still show both a manager badge and an operational problem badge.
