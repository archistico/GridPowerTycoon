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


## Milestone 28E properties panel contract

The right properties panel now follows a stable row contract. The same row labels remain in the same order whether the player selects a placed building, a terrain obstacle, a cloud cell, an empty buildable cell or an active build tool. Non-applicable values are shown as `-`.

Building data is grouped into compact summaries instead of moving rows around per building category. `PRODUCES` contains energy/research/money output, `CONSUMES` contains energy input and autosell draw, `STORAGE` contains battery/tool/grid support, and `HEAT` contains output, conversion/cooling, stored heat and risk. This keeps the panel readable while preserving the detailed diagnostics introduced by the heat, maintenance and tool systems.

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

## Expansion feedback

Milestone 23 adds direct feedback for map expansion and obstacles.

Cloud selection now previews which hidden tiles will be revealed. The cloud preview is blue when the unlock is currently valid and red when the selected cloud cannot currently be unlocked. The selected cloud also shows a small number marker with the amount of tiles that would be revealed.

Forest and mountain selection now previews clearing status. The preview is green when the player has enough tools and red when tools are missing. The map badge uses `A#` for axes and `M#` for mines.

The properties panel mirrors these previews:
- cloud tiles show `ISSUE`, `REVEAL` and `UNLOCK COST`;
- obstacle tiles show `ISSUE` and `CLEAR COST`.

After a successful unlock, the status bar summarizes the revealed terrain types, for example:

```text
AREA UNLOCKED 5: FOREST 1, LAND 3, MOUNTAIN 1
```

## Building range feedback

Selecting a built heat converter shows its current operational range on the map. The overlay highlights the covered cells and gives covered heat producers a stronger outline.

The range uses the same Chebyshev distance model as heat conversion, so the visual coverage should match the actual system behavior.

## Heat coverage relation feedback

Heat coverage can now be read from both directions.

Selecting a heat converter shows its operational range and covered heat producers. Selecting a heat producer highlights active heat converters that cover it.

## Heat converter placement preview

Heat converter building tools now show future coverage while hovering over the map. The range preview helps place generators before committing the build.

## Milestone 23 feedback summary

The map now gives immediate visual feedback for expansion, obstacles and heat coverage.

Clouds:
- selected cloud previews reveal area;
- blue means unlock is currently valid;
- red means unlock is currently blocked;
- successful unlocks summarize revealed terrain types in the status bar.

Obstacles:
- forests show axe requirements;
- mountains show mine requirements;
- green means the player can clear;
- red means tools are missing.

Heat coverage:
- selected converters show range;
- selected converters highlight covered heat producers;
- selected heat producers highlight active converters covering them;
- converter build tools preview future range before placement.

## Milestone 24 onboarding feedback

Onboarding feedback is intentionally non-blocking.

The status bar shows the current objective when no higher-priority status message is active. The early checklist appears in the map area until all starting goals are complete. The HELP panel can be opened with `HELP` or `H` and repeats the current objective/checklist together with a short guide.

Priority remains unchanged: action results, selected-building status, save/load messages and demolish confirmation override tutorial text.

## Milestone 25 progression objective feedback

The status bar objective now continues after the early checklist. It can suggest first upgrades, research, expansion, clearing obstacles, managers and next heat tiers.

The objective remains lower priority than direct feedback. Action results, selected-building status, save/load messages and demolish confirmation still override objective text.

## Goal-aware HELP detail feedback

The HELP panel `CURRENT` section now includes a `NEXT` line. This line translates the current objective into a practical next action or a resource gap.

This helps the player understand why progress is blocked without changing the status bar priority system.

## Progression bottleneck feedback

The HELP panel `CURRENT` section now includes a `BOT` line.

`BOT` is a compact diagnosis of the current progression bottleneck. It does not change simulation behavior. It reads the current resource-rate snapshot and world state, then explains the dominant issue in plain UI text.

## Progression advisor extraction

Objective, NEXT and BOT text now come from Core `ProgressionAdvisor`.

The UI still decides where to draw the text, but the decision logic is testable outside MonoGame rendering.

## Milestone 25 final feedback state

Milestone 25 completes the current progression feedback layer.

The final feedback model is:
- status bar objective for passive guidance;
- HELP `CURRENT` section for objective overview;
- HELP `NEXT` line for action/resource gap;
- HELP `BOT` line for bottleneck diagnosis.

Priority remains unchanged: direct player action feedback, selected-building status, save/load messages and demolition confirmation override general progression guidance.

## Substation feedback

The Substation appears as a special/grid-support building.

UI feedback includes:
- build card: grid boost;
- property panel: `GRID BONUS`;
- map color: cyan special-support color;
- research unlock text through `grid_substation`.

The effect is reflected in resource-rate calculations through `EnergyEfficiencyMultiplier`.

## Heat sink feedback

Heat sink feedback is shown as heat coverage rather than heat conversion.

The property panel includes `HEAT DISSIPATE`, and heat producers now report heat coverage when either a generator or heat sink is in range.

## Maintenance center feedback

The Maintenance center is shown as a maintenance building with a `MAINTENANCE` property row.

The row explains its lifetime-wear reduction. Build cards describe it as a building that extends operational lifetime.

## Tool warehouse feedback

The Tool warehouse is shown as a tool-storage building.

The property panel includes `TOOL STORAGE`, and build cards describe the capacity bonus for axes and mines.

## Geothermal plant feedback

The Geothermal plant uses existing heat-producer feedback.

It appears as a heat source in the build card and property panel. Existing heat coverage messages guide the player to place generators or heat-management buildings nearby.

## Data center feedback

The Data center uses existing Corporation/research feedback.

The property panel shows its high energy input and research output. If energy is missing, existing operational status reports `NO ENERGY`.

## Milestone 28 action feedback

Milestone 28 moves build, research and upgrade action explanations into `GameplayFeedbackFormatter` in the Core project.

The status bar now receives contextual failure text for blocked actions: missing money, missing research points, missing prerequisite research, maxed upgrades and invalid build placement all include the relevant name and resource gap where possible.

The left panel also shows hover card details for visible BUILD, RESEARCH and UPGRADE cards. These details are intentionally compact and repeat the most important context before the click: ready/locked/completed state, cost, missing prerequisite, effect, level progress, target building, size and lifetime where relevant. MonoGame only draws the panel; the text itself comes from Core so the same interpretation can be covered by tests.


## Critical resource warnings

Milestone 28C adds a passive warning layer to the same Core formatter used by the status bar and card hover details. `GameplayFeedbackFormatter.FormatCriticalWarning()` returns the highest-priority current risk, while `FormatCriticalWarnings()` returns the full ordered list for tests and future UI panels.

The first status-bar fallback is now a critical warning when one exists. It appears only when there is no more specific message from a player action, save/load event, demolition confirmation, selected building or active build tool. This preserves the existing priority model while making hidden problems easier to notice during normal play.

The current warning families are energy starvation, heat coverage/risk, maintenance/end-of-life, exploded buildings and missing terrain-clearing tools. The logic is deliberately read-only and does not alter production, storage, heat, lifetime or tool generation.


## Production summary panel

Milestone 28D adds a compact grid-wide summary panel. Unlike the status bar, which is event-driven and shows the most important current message, this panel is a stable dashboard for the player's economy loop. It summarizes energy production and consumption, research and money rates, heat production and coverage, maintenance risk and tool stock/rates.

The panel is intentionally diagnostic. It does not replace the top resource bar and it does not modify any balance rules. It answers the practical question: “what is my grid doing right now?” When the viewport is too narrow, the panel is hidden rather than overlapping the left menu, properties panel or status bar.

The text is produced by `GameplayFeedbackFormatter.FormatProductionSummaryLines()`, keeping the summary calculation inside Core and keeping MonoGame responsible only for drawing.


## Card availability contract

Build, research and upgrade cards now use one Core-side availability contract from `GameplayFeedbackFormatter`. The visible card line and the hover panel should therefore agree on the exact cause.

The expected states are:

| Card | States | Detail shown |
| --- | --- | --- |
| Build | ready, locked by research, missing money, unknown | cost, current money and missing amount when needed |
| Research | ready, completed, locked by prerequisite, missing research points, unknown | prerequisite names or cost/current/missing research points |
| Upgrade | ready, max level, locked by research, missing money, missing research, mixed shortage, unknown | next-level cost, current resources and missing amounts |

The rule is that generic labels such as `REQ RESEARCH` or `NEED RESOURCES` should not be the final explanation shown to the player when the exact blocker is known.
