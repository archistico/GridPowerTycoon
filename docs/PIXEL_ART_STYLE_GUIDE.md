# Pixel Art Style Guide

This document defines the visual direction for GridPowerTycoon from Milestone 29 onward. The goal is to move the game toward a coherent, readable pixel-art identity without changing the current top-down grid gameplay.

## Core direction

GridPowerTycoon uses a strict pixel-art style. The world is top-down and orthogonal, with square grid tiles and clear silhouettes. The style must favor readability over realism: the player should immediately understand terrain type, buildability, building identity and critical state.

The project does not use isometric projection for the main map. Isometric art would require a different footprint model, hit testing model and placement language. The current game remains a square-grid tycoon, so the art must reinforce that structure instead of fighting it.

## Tile size

The official world tile size is **32x32 pixels**.

This scale is large enough to distinguish terrain, forests, mountains and small buildings while remaining compact enough for a large tycoon map. Multi-tile buildings must use the same unit:

| Footprint | Sprite area |
|---:|---:|
| 1x1 | 32x32 px |
| 2x2 | 64x64 px |
| 3x3 | 96x96 px |

Sprites may contain transparent pixels inside their assigned footprint, but they must not visually imply interaction outside their footprint unless the renderer later supports explicit decorative overhang rules. Selection, placement and occupancy remain tied to the grid footprint.

## Camera and rendering constraints

Pixel art must be rendered pixel-perfect. The MonoGame renderer should use point sampling for pixel assets, avoid texture smoothing and keep sprite positions aligned to integer pixels after camera transforms wherever possible.

The intended rendering rules are:

| Rule | Required behavior |
|---|---|
| Sampling | Point/nearest-neighbor sampling |
| Scaling | Controlled scaling only; avoid arbitrary fractional blur |
| Positioning | Snap world sprite draw positions to pixel-aligned coordinates |
| Texture filtering | No smoothing/linear filtering for pixel assets |
| Mixed art | Do not mix semi-HD or painterly sprites with pixel-art world assets |
| Overlay art | Build previews, warnings and highlights must also follow the pixel-art language |

The visual pass must not introduce blurred assets or anti-aliased outlines. Any future scaling option must be tested specifically for pixel crispness.

## Lighting and shading

Lighting is consistent and simple. The default light direction is **top-left**. Shadows should fall subtly toward the bottom-right. The game should not use complex dynamic lighting for normal map elements.

Recommended shading style:

- one readable outline or dark edge where needed;
- 2-4 shade levels for most surfaces;
- small highlights only where they clarify material or activity;
- simple bottom-right contact shadows for buildings and mountains;
- avoid noisy dithering on gameplay-critical areas.

Terrain should be quieter than buildings. Buildings and active infrastructure must stand out from the map.

## Palette direction

The palette should be limited and consistent. The exact final palette can evolve during the first art passes, but every asset should follow the same families:

| Area | Direction |
|---|---|
| Grass | muted greens, not neon |
| Dirt | warm browns with readable contrast |
| Rock/mountain | neutral grays and desaturated browns |
| Forest | darker greens than grass, with trunk accents |
| Technology | blue-gray, steel, muted cyan active accents |
| Energy | cyan/blue or yellow highlights, used sparingly |
| Heat/fire | orange/red accents |
| Warning/error | yellow/orange/red, high contrast |
| Nuclear/late game | cold gray, controlled green/cyan glow, strong silhouette |

The map should not look like a rainbow. Each accent color must communicate state or material.

## Terrain rules

Terrain tiles must communicate buildability and resource role. They should not be visually busier than buildings.

Initial terrain families:

| Terrain | Role |
|---|---|
| grass | normal buildable land |
| dirt | variation / transitional land |
| rocky ground | harsher land, possible mountain context |
| forest | natural obstacle/resource |
| mountain | natural obstacle/resource |
| cloud/hidden | unrevealed expansion area |

Terrain should use variants to avoid a repetitive checkerboard effect. Variant selection should be deterministic per tile or saved, not randomly changed every frame.

Recommended first variant count:

| Asset family | Initial variants |
|---|---:|
| grass | 4 |
| dirt | 3 |
| rocky ground | 3 |
| forest | 3-4 |
| mountain | 3 |
| cloud/hidden | 2-4 animation or drift variants later |

## Nature rules

Forests must read as clusters of trees, not just green squares. Use different crown shapes, darker internal shadows and small trunk hints. If a forest is cleared, the resulting state should eventually show stumps or disturbed terrain rather than instantly becoming visually identical to untouched grass.

Mountains must read as solid obstacles/resources. They need a strong silhouette, top-left highlights and bottom-right shadow. A later mining/cleared state may use rubble or flattened rocky ground.

Nature can contain slight animation only when it is subtle and useful. Forest sway should be sparse and slow; mountains should remain static.

## Building rules

Every building must be recognizable from its silhouette. Details are secondary. The player should be able to identify a wind turbine, battery, research center, heat converter or reactor before reading the panel.

The standard visual states are:

| State | Meaning |
|---|---|
| idle | building exists but has no visible active animation requirement |
| active | building is operating and may animate |
| damaged | building is at risk or near end of life, when supported later |
| exploded | building is unusable and must look clearly broken |

Not every building needs a separate sprite for every state immediately. The renderer may combine a base sprite with overlays for selection, warning, inactive and exploded states. However, the asset naming and future pipeline must assume those states can exist.

## Animation rules

Animations should be short, readable and purposeful. The game must not animate everything just because animation is available.

Recommended animation standards:

| Animation type | Suggested frames | Suggested frame time |
|---|---:|---:|
| wind turbine blades | 4-6 | 80-120 ms |
| research lights | 3-5 | 120-250 ms |
| steam/smoke | 4-6 | 120-200 ms |
| warning pulse | 2-4 | 200-350 ms |
| explosion smoke | 4-6 | 150-250 ms |

High-priority animations:

1. wind turbine blades;
2. geothermal steam;
3. coal/gas smoke;
4. research center lights;
5. nuclear reactor activity lights/steam;
6. exploded building smoke.

Animations should pause or switch state when a building is inactive, expired or exploded. The visual state must reflect gameplay state.

## UI and overlay rules

Map overlays must stay readable and must not hide the sprite identity. Selection, placement preview and warnings should be pixel-art overlays using simple shapes and strong contrast.

Recommended overlay language:

| Overlay | Direction |
|---|---|
| selected tile/building | thin bright outline or corner brackets |
| valid build preview | green/cyan transparent pixel overlay |
| invalid build preview | red/orange transparent pixel overlay |
| warning | small icon/pulse, not a full-cover block |
| inactive | muted overlay or small status badge |
| exploded | smoke/debris and clear broken silhouette |

The UI panels may remain simple for now, but any future iconography should follow the same pixel-art discipline.

## Asset quality checklist

Before accepting a world sprite, verify:

- it fits the correct 32x32 grid footprint multiple;
- it has no anti-aliased blurred edges;
- it is readable on grass, dirt and rocky terrain;
- it has a strong silhouette at gameplay zoom;
- its colors follow the project palette direction;
- it does not visually claim more space than its footprint;
- active/damaged/exploded states are planned even if not yet drawn;
- animation, if present, is short and communicates gameplay state.

## Current milestone usage

Milestone 29A defines the rules. Milestone 29B should prototype terrain and nature assets. Milestone 29C should prototype the first core building sprites. Rendering infrastructure should be introduced only after the asset contract is clear enough to avoid hardcoded one-off drawing paths.


## Terrain and nature production rules

The first integrated map pass showed that terrain changes the perceived quality of the whole game immediately. Future terrain work must therefore be treated as production art, not decorative background.

Grass should be quiet and breathable. The best grass tiles mix flatter zones with small controlled clusters, instead of filling every pixel with texture. The pattern should be hard to notice when repeated across the map. Dense variants are allowed, but they must not become visual static.

Forests should be built from coherent foliage modules. Use repeated leaf-cluster shapes with subtle variation, not many unrelated pixels. The tile should read as a single mass of vegetation first, and as individual leaves only second. Trunk hints should remain secondary.

Mountains should prioritize silhouette and faceted volume. Large planes, crisp highlights and deep lower-right shadows communicate rock better than many scattered dots. A mountain tile must immediately read as a solid obstacle/resource.

Cloud and hidden coverage should be softer than rock and less saturated than active terrain. Hidden areas should clearly feel unrevealed, while cloud/fog coverage should remain light enough that the player understands it as a temporary visual layer.

The shared light direction remains top-left. Every terrain and nature tile should respect that lighting rule unless a specific gameplay effect intentionally overrides it.

## First building sprite rules

The first building pass establishes a practical rule: each building must be recognizable by silhouette before decorative detail is added. For 1x1 buildings, the full source canvas is 32x32 px and transparent pixels are allowed, but the readable mass should usually sit inside the tile with enough breathing room for overlays and bars.

Building sprites should use the same top-left light direction as terrain. Terrain remains quieter; buildings may use stronger contrast, darker outlines and clearer icons. The first visual language assignments are:

| Building | Visual language |
|---|---|
| wind turbine | mast, nacelle and readable blade shape |
| solar panel | angled blue panel grid with metal support |
| battery | compact industrial block with yellow charge symbol |
| research center | small lab block with blue windows and antenna |

Until animated states are introduced, the idle sprite must be good enough to represent the building in normal play. Runtime overlays continue to communicate operational state, lifetime, heat, management and selection.
