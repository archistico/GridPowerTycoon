# Milestone 28 Feedback Contract

Milestone 28 is the UX feedback milestone. It does not rebalance the game and does not change save data. Its purpose is to make the current rules visible, consistent and testable.

## Final scope

The milestone now covers six feedback surfaces.

| Surface | Role |
|---|---|
| Status bar | Shows the most relevant recent action result, save/load result, selected-building status or passive warning. |
| Left card list | Shows concrete availability for build, research and upgrade entries. |
| Hover card details | Expands the same availability state with cost, effect, level and target information. |
| Right properties panel | Uses a stable row order so different buildings can be compared quickly. |
| Production summary panel | Gives a compact grid-wide balance view for energy, heat, research, money, maintenance and tools. |
| Map badges | Keep critical local states visible directly on the grid. |

## Formatter ownership

`GameplayFeedbackFormatter` is the Core-side contract for player-facing feedback text. MonoGame should not duplicate prerequisite, cost, shortage or summary calculations. The renderer should ask the formatter for UI-ready lines and focus only on layout, drawing and hit-testing.

The formatter currently owns:

| Method | Purpose |
|---|---|
| `FormatBuildFailure` | Status-bar explanation for failed build attempts. |
| `FormatResearchFailure` | Status-bar explanation for failed research attempts. |
| `FormatUpgradeFailure` | Status-bar explanation for failed upgrade attempts. |
| `FormatBuildAvailabilityLine` | One-line build card state. |
| `FormatResearchAvailabilityLine` | One-line research card state. |
| `FormatUpgradeAvailabilityLine` | One-line upgrade card state. |
| `FormatBuildCardDetails` | Hover details for build cards. |
| `FormatResearchCardDetails` | Hover details for research cards. |
| `FormatUpgradeCardDetails` | Hover details for upgrade cards. |
| `FormatCriticalWarning` / `FormatCriticalWarnings` | Passive warning diagnosis. |
| `FormatProductionSummaryLines` | Stable production/consumption summary panel lines. |

The first detail/status line in hover card details must remain aligned with the corresponding availability-line method. This is now covered by regression tests.

## Priority model

When several messages could be shown at once, direct user feedback has priority over passive diagnosis. The intended status-bar priority is:

1. explicit action result, such as build, research, upgrade, save, load, demolish, replace or terrain clear;
2. selected building status or active build-tool guidance;
3. critical passive warning;
4. generic progression guidance.

This prevents background warnings from hiding the result of the action the player just performed.

## Stable panel contracts

The production summary panel has a fixed six-line contract:

```text
GRID SUMMARY
ENERGY PROD ... | USE ... | NET ...
RESEARCH ... | MONEY ...
HEAT PROD ... | MANAGED ... | FREE ...
MAINTENANCE ACTIVE ... | AT RISK ... | WEAR ...
TOOLS AXES ... | MINES ...
```

The right properties panel follows the stable row contract documented in `FEEDBACK_SYSTEM.md`: non-applicable values use `-` instead of moving rows around.

## Regression coverage

`GameplayFeedbackFormatterTests` now covers the milestone-level contract:

- concrete failure messages for build, research and upgrade;
- hover details for locked build/research/upgrade cards;
- availability-line consistency between visible cards and hover details;
- completed research and maxed upgrade states;
- passive critical warnings for energy, heat, maintenance and explosions;
- the fixed production summary line contract.

## Next milestone

Milestone 28 can be considered closed after `dotnet test` passes and the UI is manually checked for the main feedback surfaces. The next recommended milestone is gameplay-flow work rather than another feedback-only pass.
