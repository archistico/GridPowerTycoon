# Asset Pipeline

This document defines how GridPowerTycoon pixel-art assets should be organized, named and integrated. It is a planning contract for Milestone 29 and later visual work.

## Scope

The asset pipeline covers world sprites, terrain, nature, buildings, effects and UI pixel-art elements. It does not change gameplay rules, save format, economy or progression by itself.

The initial pipeline is intentionally simple. The project should avoid introducing a heavy editor or complex runtime asset database until the first visual pass proves what is actually needed.

## Folder structure

World assets should live under the MonoGame content area:

```text
src/GridPowerTycoon.MonoGame/Content/Sprites/
  Terrain/
  Nature/
  Buildings/
  Effects/
  UI/
```

Recommended subfolders:

```text
Terrain/
  grass/
  dirt/
  rock/
  cloud/
  hidden/

Nature/
  forest/
  mountain/
  stumps/
  debris/

Buildings/
  wind_turbine/
  solar_panel/
  battery/
  research_center/
  coal_power_plant/
  geothermal_plant/
  gas_power_plant/
  heat_converter/
  heat_dissipator/
  maintenance_center/
  tool_warehouse/
  nuclear_reactor/

Effects/
  selection/
  placement/
  smoke/
  explosion/
  warning/
  inactive/

UI/
  icons/
  panels/
  buttons/
  cursors/
```

Empty folders should not be committed without a clear purpose. If a folder must exist before assets are added, include a short `README.md` explaining its role.

## File naming rules

File names use lower snake case. The file name should include the asset identity and state.

Terrain variants:

```text
grass_01.png
grass_02.png
grass_03.png
dirt_01.png
rock_01.png
```

Nature variants:

```text
forest_01.png
forest_02.png
mountain_01.png
mountain_02.png
stump_01.png
```

Building states:

```text
wind_turbine_idle.png
wind_turbine_active.png
wind_turbine_damaged.png
wind_turbine_exploded.png
```

Animation strips:

```text
wind_turbine_active_anim.png
research_center_active_anim.png
geothermal_plant_steam_anim.png
exploded_smoke_anim.png
```

Effect sprites:

```text
selection_outline.png
placement_valid.png
placement_invalid.png
warning_icon_anim.png
inactive_badge.png
```

## Sprite dimensions

World sprites use 32x32 px per tile. A building sprite must match its footprint multiple unless a future metadata file explicitly defines a different transparent canvas.

| Footprint | Expected sprite canvas |
|---:|---:|
| 1x1 | 32x32 px |
| 2x2 | 64x64 px |
| 3x3 | 96x96 px |

Animation strips should place frames horizontally by default:

```text
frame_width = tile_width * footprint_width
frame_height = tile_height * footprint_height
sheet_width = frame_width * frame_count
sheet_height = frame_height
```

For example, a 1x1 wind turbine animation with 6 frames uses a 192x32 px strip. A 3x3 nuclear reactor animation with 4 frames uses a 384x96 px strip.

## Suggested animation metadata

The first implementation can hardcode a small registry, but the intended future shape is a metadata file per animated asset or a central visual registry. The data should describe frames, timing, loop and state.

Example metadata shape:

```json
{
  "id": "wind_turbine",
  "state": "active",
  "texture": "Sprites/Buildings/wind_turbine/wind_turbine_active_anim",
  "frameWidth": 32,
  "frameHeight": 32,
  "frameCount": 6,
  "frameDurationMs": 100,
  "loop": true
}
```

The renderer should not scatter animation constants across unrelated drawing code. Once animation begins, the project should introduce a small central visual definition layer.


## Runtime terrain integration

The first runtime integration is intentionally lightweight. `TerrainSpriteCatalog` loads source PNGs from the copied `Content/Sprites` tree and provides deterministic per-tile variants to the map renderer.

Current mappings:

| Tile type | Folder |
|---|---|
| `Land` | `Sprites/Terrain/grass` |
| `Forest` | `Sprites/Nature/forest` |
| `Mountain` | `Sprites/Nature/mountain` |
| `Cloud` | `Sprites/Terrain/cloud` |

If a folder is missing or empty, the renderer must fall back to the previous simple color tile. This fallback is part of the development contract and should remain until the full visual registry is stable.

The current gameplay tile size remains 64 px. Source sprites are 32x32 px and are scaled by the renderer with point sampling, so the output remains pixel-perfect.

## Visual registry direction

A future implementation should map game ids to visual assets without changing gameplay data. The game already uses ids such as `wind_turbine`, `solar_panel` and `nuclear_reactor`; the visual layer should use the same ids.

Recommended future types:

```text
BuildingVisualDefinition
TerrainVisualDefinition
AnimatedSpriteDefinition
AnimatedSpritePlayer
BuildingVisualState
```

The first version can remain lightweight, but the architecture should keep this separation:

| Layer | Responsibility |
|---|---|
| Core data | gameplay rules and ids |
| MonoGame visual registry | maps ids/states to sprite assets |
| Renderer | draws the chosen sprite/animation |
| Feedback formatter | text and UI feedback, not sprite selection |

## Content pipeline notes

The project should keep pixel-art source files simple PNGs. When adding them to MonoGame content, preserve point sampling behavior in the renderer. The source PNGs should remain editable and should not be overwritten by generated artifacts.

Recommended initial approach:

- commit source PNGs under `Content/Sprites`;
- load them via MonoGame content or direct texture loading consistently with the existing project setup;
- keep asset ids aligned with game ids;
- avoid introducing external tools unless they solve a clear problem.

## Asset checklist for Milestone 29B and 29C

The first production pass should avoid trying to draw every building at once. The recommended order is:

### 29B - Terrain and nature prototype

| Asset family | Initial target | Current prototype |
|---|---:|---:|
| grass | 4 variants | 4 PNGs |
| dirt | 3 variants | 3 PNGs |
| rocky ground | 3 variants | 3 PNGs |
| forest | 3-4 variants | 4 PNGs |
| mountain | 3 variants | 3 PNGs |
| cloud/hidden | 2-4 variants or simple placeholder animation | 3 cloud PNGs + 2 hidden PNGs |

### 29C - Core building prototype

| Building | Minimum target |
|---|---|
| wind_turbine | idle + active animation |
| solar_panel | idle + active/readable panel surface |
| battery | idle + charge/status indicator direction |
| research_center | idle + light animation direction |

### Later building expansion

| Building | Notes |
|---|---|
| coal_power_plant | smoke/active state later |
| geothermal_plant | steam animation later |
| gas_power_plant | flame/smoke/vent indicator later |
| heat_converter | clear heat-to-energy identity |
| heat_dissipator | cooling/fan/vent identity |
| maintenance_center | support/repair identity |
| tool_warehouse | storage/tool identity |
| nuclear_reactor | 3x3 late-game identity with strong silhouette |

## Integration rules

When the renderer starts using sprite assets, it must preserve existing gameplay behavior. The first visual integration should be a drop-in replacement for current primitive drawing.

Do not change in the same step:

- building costs;
- research prerequisites;
- save format;
- footprint rules;
- placement rules;
- selection rules;
- map generation.

Visual integration should have its own regression checklist: existing build placement, selection, save/load, right panel information and hover details must still work after sprites replace colored rectangles.


## Milestone 29B prototype notes

The first prototype assets are committed as plain 32x32 PNG source files. They are deliberately simple and are not yet connected to runtime rendering. Their purpose is to validate folder layout, naming, dimensions and palette direction before introducing loader code.

The renderer integration should not randomly choose a variant every frame. Terrain and nature variants must be stable per tile, either by storing a variant index in future map visual state or by deriving one deterministically from tile coordinates and tile type.

The runtime should keep terrain visually quiet. Buildings, warnings and selection overlays must remain more prominent than grass, dirt and rocky ground.

## Terrain and nature refinement rules

The first refinement pass after runtime integration keeps the existing folders and file names but improves the visual language of each family. Terrain and nature should remain readable at gameplay scale and must not compete with buildings, warning overlays or selection highlights.

Grass should not be a uniform field of random green pixels. Use a calm base, a small number of repeated clusters, visible negative space and only rare accent details. A dense grass variant is useful, but it should still avoid noisy single-pixel scatter.

Forest tiles should read as modular masses of foliage. The canopy can vary between variants, but leaf clusters should remain consistent in shape and scale. Use darker lower/right areas for depth, lighter upper/left areas for the shared light direction, and only small trunk hints where they help readability.

Mountain and rock tiles should use larger clusters and faceted planes instead of many unrelated speckles. Highlights belong mostly on the upper-left side and shadow belongs mostly on the lower-right side. The result should read as a resource/obstacle tile even when several variants are mixed together.

Cloud and hidden tiles should communicate coverage without becoming harsh visual noise. Cloud variants should feel softer and brighter; hidden variants should be darker and more opaque, so the player can distinguish temporary fog from unrevealed territory.

The renderer already picks variants deterministically per tile. Do not add per-frame randomness to these assets or to their selection logic.

## Runtime building sprite loading

From Milestone 29D, building sprites are loaded directly from the MonoGame content output by `BuildingSpriteCatalog`.

The folder name must match the gameplay building id from `Data/buildings.json`:

```text
Content/Sprites/Buildings/wind_turbine/wind_turbine_idle.png
Content/Sprites/Buildings/solar_panel/solar_panel_idle.png
Content/Sprites/Buildings/battery_small/battery_small_idle.png
Content/Sprites/Buildings/research_small/research_small_idle.png
```

State suffixes are resolved by convention:

```text
<building_id>_idle.png
<building_id>_active.png
<building_id>_expired.png
<building_id>_exploded.png
```

The renderer may request a state-specific sprite, but the catalog must always fall back to `idle` when that state does not exist. Buildings without a sprite family must remain valid and must use the category-color rectangle fallback. This allows the art pass to proceed incrementally without blocking gameplay development.
