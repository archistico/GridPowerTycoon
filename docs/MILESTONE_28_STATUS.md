# Milestone 28 Status

Milestone 28 focuses on user-facing clarity. The goal is not to change the economy, but to make the existing rules easier to understand while playing.

## Current state

| Step | Feature | Status | Role |
|---|---|---:|---|
| 28A | Better build/research/upgrade feedback messages | Completed | Explain blocked actions with concrete costs and prerequisites |
| 28B | Hover details for build/research/upgrade cards | Completed | Show immediate context directly from hovered cards |
| 28C | Critical resource warnings | Completed | Surface energy, heat, maintenance and tool-resource risks before they become hidden failures |
| 28D | Production/consumption summary panel | Completed | Show a compact grid-wide balance summary for energy, heat, research, money, maintenance and tools |
| 28E | Building details panel consistency | Completed | Keep the right properties panel stable across selected buildings, terrain, cloud cells and build-tool preview |
| 28F | Locked reason visibility pass | Completed | Use one Core-side availability contract for visible cards and hover details |
| 28G | Feedback regression documentation | Prepared | Close the milestone with a documented feedback contract and regression tests |

## Step 28A result

Step 28A introduces `GameplayFeedbackFormatter` in the Core project. The formatter receives the current `GameWorld` and turns build, research and upgrade failures into messages that include the relevant context instead of only the generic failure reason.

The improved messages now include details such as:

| Action | Example information shown |
|---|---|
| Build without money | building name, cost, current money and missing money |
| Build locked by research | building name and required research name |
| Build on invalid cell | position and reason, such as occupied cell or non-land cell |
| Research without points | research name, cost, current research and missing research |
| Research locked by prerequisite | missing prerequisite research name |
| Upgrade without resources | upgrade name, next level, cost and available resource |
| Upgrade locked by research | upgrade name and required research name |
| Upgrade at max level | current level and max level |

`ResearchResult` and `UpgradeResult` now preserve the attempted id even when the action fails. This allows the UI and tests to describe the specific blocked research or upgrade instead of only showing `MISSING PREREQUISITE` or `RESEARCH REQUIRED`.

The MonoGame status bar uses the new formatter for build, research and upgrade failures. Existing success messages and gameplay rules are unchanged.

Regression tests were added in `GameplayFeedbackFormatterTests` to lock the main expected messages for missing money, missing research prerequisites and locked upgrades.

## Step 28B result

Step 28B extends the same `GameplayFeedbackFormatter` contract with card-detail helpers for build, research and upgrade cards. These helpers return short lines that describe the hovered card state without duplicating lookup logic inside MonoGame.

The MonoGame left panel now shows a compact floating detail panel when the mouse is over a visible build, research or upgrade card. The panel is drawn outside the card list, stays inside the safe viewport area and explains the current state of the card:

| Card type | Detail examples |
|---|---|
| Build | ready/locked/need money, cost, required research, main effect, size and lifetime |
| Research | completed/locked/ready/need research, cost, missing prerequisites and unlock summary |
| Upgrade | max/locked/ready/need resources, next level, cost, effect and target building |

This keeps the card itself compact while giving the player a clearer reason before clicking. The gameplay and balance are unchanged.

Additional tests were added to `GameplayFeedbackFormatterTests` for locked build card details, locked research card details and locked upgrade card details.



## Step 28C result

Step 28C extends `GameplayFeedbackFormatter` with critical warning evaluation. The formatter now reads the current `GameWorld`, the resource-rate snapshot and operational building states to produce short warnings for the most important risk currently visible to the player.

The warning layer covers energy starvation, heat coverage/risk, expired or near-expired buildings, exploded buildings and missing tools for clearable terrain. It does not change the simulation, costs, production, heat, lifetime or terrain rules. It only gives the player a clearer status-bar diagnosis when there is no more specific click result, save/load message, demolition confirmation or selected-building status taking priority.

The status bar now uses this warning before falling back to the generic progression objective. Direct action feedback still has higher priority, so failed build/research/upgrade messages remain visible immediately after the click.

Additional tests in `GameplayFeedbackFormatterTests` cover empty-energy consumers, heat producers without coverage, low remaining lifetime and exploded-building priority.


## Step 28D result

Step 28D adds a compact production/consumption summary panel to the main game screen. The panel is intentionally read-only and does not change the simulation, economy or balance. Its purpose is to make the current grid state understandable at a glance after the expansion of heat, maintenance, tools and end-game production.

The summary is generated in Core by `GameplayFeedbackFormatter.FormatProductionSummaryLines()`, so the calculations are testable without MonoGame. The panel shows:

| Area | Information shown |
|---|---|
| Energy | gross production, consumption and net change |
| Research / money | research rate and autosell money rate |
| Heat | produced heat, managed heat and unmanaged/free heat |
| Maintenance | active buildings, at-risk buildings and current lifetime-wear multiplier |
| Tools | current axes/mines, capacities and generation rates |

`UiRenderer` draws the panel in the free central area between the left command panel and the right properties panel when the viewport is wide enough. If the screen is too narrow, the panel is hidden instead of overlapping the core UI.

Additional tests in `GameplayFeedbackFormatterTests` lock the main summary lines for energy, research, money, heat, maintenance and tools.

## Step 28E result

Step 28E reorganizes the right properties panel into a stable row contract. The panel keeps the same high-level rows for selected buildings, terrain, cloud cells, empty cells and build-tool preview, using `-` when a value is not applicable. This makes it easier to compare different buildings without hunting for moved fields.

The fixed row order is now centered on the information the player needs most often: type, purpose, state, issue, build tool, cost, footprint, requirement, production, consumption, storage, heat, maintenance, lifetime, manager, next upgrade, economy, payback, reveal/terrain/unlock costs and action.

Building-specific details that previously appeared as many separate rows are now grouped into compact summaries:

| Row | Meaning |
|---|---|
| `PRODUCES` | energy, converted energy, research and estimated money output |
| `CONSUMES` | energy input and autosell draw |
| `STORAGE` | battery capacity, tool capacity and grid-efficiency support |
| `HEAT` | heat output/input/cooling, stored heat and practical risk |
| `MAINTENANCE` | wear reduction from maintenance-support buildings |
| `LIFETIME` | remaining lifetime against effective lifetime |

This is a UI-only cleanup. It does not change economy, simulation, save format or build rules.


## Step 28F result

Step 28F consolidates the availability/lock explanation for build, research and upgrade cards. The same `GameplayFeedbackFormatter` methods now drive both the visible left-panel card status and the hover detail line, avoiding mismatches such as a card saying only `REQ RESEARCH` while the tooltip names the exact research.

The new availability helpers cover ready, locked, completed/maxed and missing-resource states. Build cards show missing money with cost, current money and missing amount. Research cards show missing prerequisites or missing research points. Upgrade cards now distinguish missing money, missing research points and mixed shortages.

This is UI feedback only: no costs, prerequisites, gameplay behavior, save format or simulation rules changed.

## Next step

Recommended next milestone:

```text
Milestone 29 - Gameplay flow and progression polish
```

## Step 28G result

Step 28G closes the Milestone 28 feedback pass. No gameplay, economy, save-format or rendering behavior was changed. The step adds regression coverage and records the final feedback contract in `MILESTONE_28_FEEDBACK.md`.

The new regression tests verify that the visible availability line used by cards remains aligned with the first status line shown in hover details for build, research and upgrade entries. They also lock completed/maxed states and the fixed six-line production summary contract.

Milestone 28 is now ready to be considered complete after the full test suite passes and the main manual UI checks are done.
