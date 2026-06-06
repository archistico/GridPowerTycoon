# Milestone 29 Visuals

Milestone 29 focuses on the visual identity of GridPowerTycoon. The target is a strict pixel-art world with clearer terrain, natural obstacles, buildings, state overlays and selected animations.

## Direction

The game remains top-down and orthogonal. The visual work must support the existing grid-based tycoon gameplay instead of changing it. The official world tile size is 32x32 pixels. Pixel art is mandatory for terrain, forests, mountains, buildings, overlays and future map effects.

The first priority is not decorative richness, but readability. The player must be able to understand the map state quickly: what terrain is buildable, which cells contain resources or obstacles, what each building is, whether it is active, at risk, expired or exploded, and whether an action is valid.

## Current state

| Step | Feature | Status | Role |
|---|---|---:|---|
| 29A.1 | Pixel art rules, style guide and asset pipeline | Prepared | Establish the visual contract before producing assets |
| 29B | Terrain and nature prototype pass | Prepared | Add first 32x32 prototype PNG assets for terrain, cloud/hidden coverage, forests and mountains |
| 29C | Terrain sprite renderer integration | Prepared | Wire terrain/nature PNGs into runtime rendering with deterministic per-tile variants and fallback colors |
| 29C.1 | Terrain and nature refinement pass | Prepared | Refine grass, forest, mountain, cloud/hidden, dirt and rock assets using quieter texture, stronger modular forms and consistent top-left lighting |
| 29D | Core building sprite prototype | Planned | Introduce first building sprites for wind turbine, solar panel, battery and research center |
| 29D | First animation pass | Planned | Add selected animations such as turbine blades, lights, steam and smoke |
| 29E | Building expansion pass | Planned | Extend art to industrial, heat, tool, maintenance and nuclear buildings |
| 29F | Effects and state overlays | Planned | Add pixel-art overlays for selection, placement, warning, inactive and exploded states |
| 29G | Visual integration and polish | Planned | Check consistency, contrast, readability and performance across the full UI |

## Step 29A.1 result

Step 29A.1 creates the formal visual foundation. It adds two project documents:

| Document | Purpose |
|---|---|
| `PIXEL_ART_STYLE_GUIDE.md` | Defines the pixel-art visual language, tile size, palette direction, lighting, animation rules and quality checklist |
| `ASSET_PIPELINE.md` | Defines folder structure, naming rules, sprite dimensions, animation strip conventions and future visual registry direction |

The key decisions are:

- use strict pixel art;
- keep the map top-down and orthogonal;
- use 32x32 px as the official tile size;
- avoid isometric projection;
- use point/nearest rendering with no smoothing;
- keep terrain quieter than buildings;
- use limited, coherent palette families;
- animate only meaningful gameplay elements;
- separate gameplay ids from visual definitions.

No runtime rendering, gameplay, economy, balancing or save-format behavior changes in this step.

## Step 29B result

Step 29B adds the first concrete terrain and nature prototype PNGs under `src/GridPowerTycoon.MonoGame/Content/Sprites`. These are source assets, not final production art, and they are intentionally not wired into runtime rendering yet. The goal is to validate scale, palette direction, naming and folder organization before replacing the current primitive map drawing.

Added asset families:

| Family | Count | Folder | Role |
|---|---:|---|---|
| grass | 4 | `Terrain/grass` | quiet buildable baseline terrain |
| dirt | 3 | `Terrain/dirt` | secondary ground / disturbed terrain direction |
| rock | 3 | `Terrain/rock` | rocky ground direction |
| cloud | 3 | `Terrain/cloud` | visible cloud/fog coverage direction |
| hidden | 2 | `Terrain/hidden` | unrevealed tile direction |
| forest | 4 | `Nature/forest` | dense natural resource/obstacle variants |
| mountain | 3 | `Nature/mountain` | rocky natural resource/obstacle variants |

All sprites use the official 32x32 px tile size and follow the lower snake case naming contract from `ASSET_PIPELINE.md`.

The assets should be considered prototype placeholders. The next implementation step should introduce a visual registry / loader and render these assets on the map without changing gameplay rules.

## Recommended next step

The next step should be:

```text
Milestone 29C - Terrain sprite renderer integration
```

This should map terrain tile types to the new 32x32 sprite families, choose stable per-tile variants and draw them pixel-perfect with point sampling.

## Step 29C result

Step 29C wires the 32x32 terrain and nature prototype PNGs into the runtime map renderer. The integration is deliberately narrow and conservative: it only replaces primitive tile fills when a matching sprite family is available, while preserving all existing gameplay, selection, footprint, build preview, heat range, clear/unlock preview and status overlays.

Runtime mapping:

| Tile type | Sprite family |
|---|---|
| `Land` | `Content/Sprites/Terrain/grass` |
| `Forest` | `Content/Sprites/Nature/forest` |
| `Mountain` | `Content/Sprites/Nature/mountain` |
| `Cloud` | `Content/Sprites/Terrain/cloud` |
| `Water` | color fallback, until a water family exists |

Variant selection is deterministic per tile coordinate and tile type. This avoids visual flicker and prevents terrain variants from changing every frame. If a sprite folder is missing or empty, the renderer falls back to the previous solid-color tile drawing. This keeps development builds resilient while the art pipeline evolves.

The MonoGame project now copies `Content/Sprites/**/*.png` to the output directory. Sprite drawing continues to use `SamplerState.PointClamp`, so the 32x32 sources render pixel-perfect when scaled to the current 64 px gameplay tile size.

No gameplay, balance, save format, simulation or input behavior changes in this step.

## Recommended next step

The next step should be:

```text
Milestone 29C.1 - Terrain and nature refinement pass
```

This should refine the integrated terrain and nature sprites before buildings are introduced. Terrain must stay quieter than buildings, but it should read as a coherent pixel-art world instead of a set of placeholder textures.

## Step 29C.1 result

Step 29C.1 replaces the first terrain/nature prototypes with a more mature pixel-art pass. The implementation keeps the same file names and folder structure introduced in 29B, so the runtime integration from 29C continues to work without code changes.

Updated asset families:

| Family | Count | Refinement direction |
|---|---:|---|
| grass | 4 | calmer base, controlled clusters, more negative space and fewer random pixels |
| dirt | 3 | warmer disturbed ground with compact cracks and coherent speckling |
| rock | 3 | larger readable stone clusters and stronger top-left highlights |
| cloud | 3 | softer fog shapes with less harsh noise |
| hidden | 2 | darker unrevealed coverage with restrained texture |
| forest | 4 | modular canopy masses, stronger leaf clusters, trunk hints and bottom-right depth |
| mountain | 3 | faceted rocky silhouettes, clearer shadow direction and less scattered noise |

The refinement follows the visual principles recorded in the project style guide: strict 32x32 pixel art, top-down orthogonal readability, quiet terrain, modular nature forms, consistent top-left light and no runtime gameplay changes. It is still a prototype art pass, but it is now suitable as the base for the first building sprite work.

No gameplay, balance, save format, simulation, input or renderer-code behavior changes in this step.

## Recommended next step

The next step should be:

```text
Milestone 29D - Core building sprite prototype
```

This should introduce first building visuals for wind turbine, solar panel, battery and research center while keeping the current category-color fallback for buildings without sprites.

## Step 29D result

Step 29D introduces the first runtime building sprite pipeline and the first four strict pixel-art building prototypes. The change is intentionally narrow: buildings with a matching sprite are drawn through PNG assets, while all other buildings continue to use the existing category-color fallback. This lets the visual pass grow one building family at a time without breaking the current map renderer.

Added building sprite families:

| Building id | Sprite | Footprint | Role |
|---|---|---:|---|
| `wind_turbine` | `Buildings/wind_turbine/wind_turbine_idle.png` | 1x1 | first recognizable power producer silhouette |
| `solar_panel` | `Buildings/solar_panel/solar_panel_idle.png` | 1x1 | first heat/solar industrial tile visual |
| `battery_small` | `Buildings/battery_small/battery_small_idle.png` | 1x1 | first storage visual with charge/readability iconography |
| `research_small` | `Buildings/research_small/research_small_idle.png` | 1x1 | first research visual with lab/monitor/antenna language |

Added `BuildingSpriteCatalog` in the MonoGame rendering layer. It loads PNGs from `Content/Sprites/Buildings/<building_id>/` and supports state suffixes such as `idle`, `active`, `expired` and `exploded`. Only `idle` sprites are provided in this step; if a state-specific file is missing, the catalog falls back to `idle`. If a building folder or sprite is missing, `MapRenderer` falls back to the previous colored rectangle.

The existing lifetime bar, heat bar, operational badge, manager badge, heat-range outlines, selected outlines and expired diagonal overlay remain drawn above the sprite. Gameplay, balance, save format, simulation and input behavior do not change.

## Recommended next step

The next step should be:

```text
Milestone 29E - First animation pass
```

This should introduce a tiny animation foundation and begin with the wind turbine blades, keeping static fallback for all systems and avoiding broad visual rewrites.
