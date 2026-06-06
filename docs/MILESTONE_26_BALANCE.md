# Milestone 26 Balance

Milestone 26 is the first content-expansion milestone after the initial progression and guidance work. Its purpose is not to add only stronger producers, but to add buildings that create new strategic pressure: grid efficiency, heat safety, lifetime stability, tool logistics, stable heat, research demand and end-game thermal production.

All numeric values below reflect the runtime JSON files under `src/GridPowerTycoon.MonoGame/Data` at the close of Step 26H.

## New building balance table

| Building id | Name | Cost | Size | Role | Main values | Unlock |
|---|---|---:|---:|---|---|---|
| `substation_small` | Trasformatore | 2,200 | 1x1 | Grid efficiency support | grid +10%; energy in 0.5/s | `grid_substation` |
| `heat_sink_small` | Raffreddatore | 900 | 1x1 | Heat safety | dissipates 18 heat/s, range 1; energy in 0.4/s | `heat_management` |
| `maintenance_center_small` | Centro manutenzione | 3,200 | 1x1 | Operational stability | wear -25%; energy in 0.8/s | `maintenance_center` |
| `tool_warehouse_small` | Magazzino strumenti | 1,800 | 1x1 | Expansion logistics | tools cap +25; energy in 0.25/s | `tool_storage` |
| `geothermal_plant` | Centrale geotermica | 12,000 | 2x2 | Stable heat source | heat 180/s; energy in 1.5/s; life 600s | `geothermal_power` |
| `data_center` | Data center | 250,000 | 2x2 | Late-game research sink | research 120/s; energy in 80/s | `data_center` |
| `nuclear_reactor` | Reattore nucleare avanzato | 18,000,000 | 3x3 | End-game heat source | heat 9000/s; energy in 120/s; life 900s | `nuclear_power` |

## Heat-chain reference

Heat remains the main power-scaling mechanic. Heat producers do not create electricity by themselves; they need generators or they become an explosion risk. Heat sinks can remove excess heat but intentionally do not produce energy.

| Producer | Heat/s | Small generators needed | Medium generators needed | Notes |
|---|---:|---:|---:|---|
| `solar_panel` | 18 | 1 | 1 | early safe pair with one small generator |
| `coal_power_plant` | 450 | 18 | 1 | mid-game source, normally a medium-generator target |
| `geothermal_plant` | 180 | 8 | 1 | stable lower-pressure 2x2 source |
| `gas_power_plant` | 3500 | 140 | 6 | industrial source, needs a conversion cluster |
| `nuclear_reactor` | 9000 | 360 | 14 | end-game source, requires many converters and safety support |

Current heat safety thresholds are warning at `250` accumulated heat and explosion at `500` accumulated heat. Because the nuclear reactor generates `9000` heat/s, it must be treated as a layout and infrastructure challenge, not as a normal single-building upgrade.

## Research pacing

The final Milestone 26 research chain deliberately makes nuclear power depend on three different mature capabilities: geothermal heat production, maintenance stability and data-center research throughput.

| Research | Cost | Prerequisites | Unlocks |
|---|---:|---|---|
| `heat_management` | 120 | `generator_small` | `heat_sink_small` |
| `tool_storage` | 220 | `heat_management` | `tool_warehouse_small` |
| `grid_substation` | 180 | `generator_small` | `substation_small` |
| `maintenance_center` | 300 | `grid_substation` | `maintenance_center_small` |
| `coal_power` | 450 | `generator_small` | `coal_power_plant` |
| `geothermal_power` | 1,600 | `coal_power` | `geothermal_plant` |
| `generator_medium` | 1,000 | `coal_power` | `generator_medium` |
| `gas_power` | 4,500 | `generator_medium` | `gas_power_plant` |
| `research_large` | 9,000 | `gas_power` | `research_large` |
| `data_center` | 8,000 | `research_large` | `data_center` |
| `nuclear_power` | 25,000 | `geothermal_power`, `maintenance_center`, `data_center` | `nuclear_reactor` |

## Tool and expansion caps

Base tools now generate at `0.025` axes/s and `0.0125` mines/s, with caps of `25` each. Clearing costs are `3` axes for a forest and `4` mines for a mountain. A tool warehouse adds `+25` to both caps, so it helps the player store expansion resources without accelerating generation by itself.

## Closure decision

Milestone 26 should now be considered content-complete. The next milestone should not add more production tiers immediately. It should stabilize save compatibility, player-facing feedback and data-version handling now that the runtime data model has grown with new building properties and additional ids.
