# Terrain Pixel Art Prototype

Milestone 29B introduces the first 32x32 terrain prototype assets.

These files are source PNGs and are intentionally simple. They are not final art and are not wired into the renderer yet. Their purpose is to establish the first concrete terrain palette, dimensions and naming contract for the later visual registry.

Current families:

| Family | Files | Role |
|---|---:|---|
| grass | 4 | quiet buildable baseline terrain |
| dirt | 3 | secondary ground / disturbed terrain direction |
| rock | 3 | rocky ground direction for harder terrain |
| cloud | 3 | visible fog/cloud cover direction |
| hidden | 2 | dark unrevealed tile direction |

All sprites are 32x32 px, nearest-neighbor friendly, and use opaque or intentionally alpha-capable RGBA pixels only.
