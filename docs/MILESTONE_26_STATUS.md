# Milestone 26 Status

Milestone 26 is complete through Step 26H.

## Current state

| Step | Feature | Status | Role |
|---|---|---:|---|
| 26A | Substation / Transformer | Done | Grid efficiency support |
| 26B | Heat sink / Raffreddatore | Done | Heat safety |
| 26C | Maintenance center / Centro manutenzione | Done | Operational stability |
| 26D | Tool warehouse / Magazzino strumenti | Done | Expansion logistics |
| 26E | Geothermal plant / Centrale geotermica | Done | Stable heat source |
| 26F | Data center | Done | Late-game energy sink / research |
| 26G-pre | New buildings consistency pass | Done | Technical consolidation |
| 26G | Nuclear plant / advanced reactor | Done | High-tier risky heat source |
| 26H | Final docs and balance pass | Done | Milestone closure |

## New mechanics introduced

The milestone introduced four new building-definition properties. They are all data-driven and live in `BuildingDefinition`, with runtime values read from `buildings.json`.

| Property | Used by | Purpose |
|---|---|---|
| `EnergyEfficiencyBonus` | Substation | Multiplies effective energy output |
| `HeatDissipationPerSecond` | Heat sink | Removes heat without producing energy |
| `MaintenanceEfficiencyBonus` | Maintenance center | Slows lifetime decay |
| `ToolCapacityBonus` | Tool warehouse | Increases max axes and mines |

The Geothermal plant, Data center and Nuclear reactor intentionally reuse existing mechanics:
- `geothermal_plant` is a `HeatProducer`;
- `data_center` is a `Corporation` with high `ResearchPerSecond` and high `EnergyConsumptionPerSecond`;
- `nuclear_reactor` is a high-tier `HeatProducer`, not a direct power producer.

## Final balance reference

The detailed final balance table is in `docs/MILESTONE_26_BALANCE.md`. That file records the exact runtime values for the new buildings, the heat-chain ratios, the research pacing and the tool-cap decision.

The most important closure decision is that the nuclear reactor remains part of the heat-conversion economy. It generates `9000` heat/s, consumes `120` energy/s, has a `3x3` footprint and requires `nuclear_power`. It must be supported by mature conversion, storage, safety and research infrastructure.

## Regression coverage

The current regression coverage includes runtime-data consistency checks for:
- building required research ids;
- research unlock building ids;
- research managed building ids;
- research prerequisite ids;
- upgrade target building ids;
- upgrade required research ids;
- UI build ids;
- UI research ids;
- UI upgrade ids;
- complete build/research/upgrade UI reachability.

Focused runtime-data tests also lock the nuclear reactor design at the JSON/catalog level. Local tests were reported passing after Step 26G. Step 26H changes documentation only.

## Next milestone

```text
Milestone 27 - Save, stability and quality of life
```

Recommended first step:

```text
Step 27A - Save compatibility check for new building properties
```

Milestone 27 should confirm that saves remain robust after the expanded building model and should improve persistence feedback before more gameplay tiers are added.
