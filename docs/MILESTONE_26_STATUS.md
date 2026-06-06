# Milestone 26 Status

This document records the pause point after the first block of new buildings and production chains.

## Current state

Milestone 26 is complete through Step 26F.

| Step | Feature | Status | Role |
|---|---|---:|---|
| 26A | Substation / Transformer | Done | Grid efficiency support |
| 26B | Heat sink / Raffreddatore | Done | Heat safety |
| 26C | Maintenance center / Centro manutenzione | Done | Operational stability |
| 26D | Tool warehouse / Magazzino strumenti | Done | Expansion logistics |
| 26E | Geothermal plant / Centrale geotermica | Done | Stable heat source |
| 26F | Data center | Done | Late-game energy sink / research |
| 26G-pre | New buildings consistency pass | Next | Technical consolidation |
| 26G | Nuclear plant / advanced reactor | Planned | High-tier risky heat source |
| 26H | Final docs and balance pass | Planned | Milestone closure |

## New mechanics introduced

The milestone introduced four new building-definition properties:

| Property | Used by | Purpose |
|---|---|---|
| `EnergyEfficiencyBonus` | Substation | Multiplies effective energy output |
| `HeatDissipationPerSecond` | Heat sink | Removes heat without producing energy |
| `MaintenanceEfficiencyBonus` | Maintenance center | Slows lifetime decay |
| `ToolCapacityBonus` | Tool warehouse | Increases max axes and mines |

The Geothermal plant and Data center intentionally reused existing mechanics:
- `geothermal_plant` is a `HeatProducer`;
- `data_center` is a `Corporation` with high `ResearchPerSecond` and high `EnergyConsumptionPerSecond`.

## Why the next step is not nuclear yet

The last implementation block touched Core, JSON data, UI, tests and documentation repeatedly. Some temporary errors happened when a partial fix updated one layer but not the others.

Before adding the nuclear reactor, the project should receive a consistency pass that checks references and UI/data alignment.

## Next step

Resume from:

```text
Step 26G-pre - New buildings consistency pass
```

The pass should add or improve tests for:
- building required research ids;
- research unlock building ids;
- research prerequisite ids;
- UI build ids;
- UI research ids;
- UI property helper consistency where practical.

## After 26G-pre

Implement:

```text
Step 26G - Nuclear plant / advanced reactor
```

The nuclear plant should be a high-tier risky heat producer, not a direct power producer and not a simple larger coal plant.
