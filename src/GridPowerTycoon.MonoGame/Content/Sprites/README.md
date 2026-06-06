# Sprites

This folder is reserved for the Milestone 29 pixel-art world assets.

The visual direction is documented in:

- `docs/PIXEL_ART_STYLE_GUIDE.md`
- `docs/ASSET_PIPELINE.md`
- `docs/MILESTONE_29_VISUALS.md`

World sprites use a 32x32 px tile unit and must remain strict pixel art. Do not add blurred, anti-aliased or non-pixel-art assets here.

Current prototype assets were added in Milestone 29B for terrain and nature. Recommended subfolders for ongoing implementation steps:

```text
Terrain/
Nature/
Buildings/
Effects/
UI/
```


## Milestone 29B prototype inventory

The first committed prototype sprite families are:

| Folder | Count |
|---|---:|
| `Terrain/grass` | 4 |
| `Terrain/dirt` | 3 |
| `Terrain/rock` | 3 |
| `Terrain/cloud` | 3 |
| `Terrain/hidden` | 2 |
| `Nature/forest` | 4 |
| `Nature/mountain` | 3 |

They are source PNGs only. Runtime integration should happen in a later step through a small visual registry/loader instead of hardcoding every file path inside unrelated rendering code.

## Milestone 29D building prototype inventory

The first runtime building sprite families are:

| Folder | Sprite |
|---|---|
| `Buildings/wind_turbine` | `wind_turbine_idle.png` |
| `Buildings/solar_panel` | `solar_panel_idle.png` |
| `Buildings/battery_small` | `battery_small_idle.png` |
| `Buildings/research_small` | `research_small_idle.png` |

Building folder names must match gameplay building ids from `Data/buildings.json`. Runtime loading is handled by `BuildingSpriteCatalog`; if a sprite family is missing, the renderer falls back to the previous colored building block.
