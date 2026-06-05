# Architettura

GridPowerTycoon è diviso in tre aree principali.

`GridPowerTycoon.Core` contiene il dominio del gioco: mappa, edifici, risorse, cataloghi dati e sistemi di gioco. Non deve dipendere da MonoGame.

`GridPowerTycoon.MonoGame` contiene finestra, rendering, input, camera, UI e integrazione tra input utente e comandi del Core.

`GridPowerTycoon.Core.Tests` contiene i test automatici del dominio.

## Dati esterni

I contenuti di gioco sono caricati da JSON:

- `Data/buildings.json` per gli edifici;
- `Data/economy.json` per le impostazioni economiche;
- `Data/maps/default-map.json` per la mappa iniziale.

Questa scelta permette di bilanciare costi, produzioni, vite, capacità e layout della mappa senza ricompilare.

## Rendering e input

La mappa è renderizzata con `MapRenderer`, usando coordinate mondo. La camera `Camera2D` gestisce pan e zoom. La UI è renderizzata separatamente senza trasformazione di camera.

`MapInputController` converte il mouse da coordinate schermo a coordinate cella e invia i comandi di costruzione al `BuildSystem`. `UiRenderer` espone i rettangoli dei pulsanti edificio e disegna la UI minima.

## Simulazione base

La simulazione è orchestrata da `GridPowerTycoon.Core.Simulation.GameSimulation`. Il ciclo attuale aggiorna prima la vita degli edifici, poi produzione e vendita automatica. Questo significa che un edificio che scade nel tick corrente non produce in quello stesso tick; la scelta evita produzione residua dopo la scadenza ed è coerente con l'idea che la durata sia un limite operativo netto.

La vendita manuale e automatica è gestita da `SellSystem`, che usa i valori di `EconomySettings` caricati da `economy.json`. Il frontend MonoGame non modifica direttamente denaro o energia: chiama `SellSystem.SellAll()` dal pulsante `SELL`.

## Building replacement flow

Expired building replacement stays in `GridPowerTycoon.Core.Build.BuildSystem`, not in the MonoGame UI. The UI selects a building instance, shows its state and calls `ReplaceExpired` when the player presses the replace button. This keeps the same boundary used for initial construction: MonoGame collects input, Core validates rules and mutates game state.

## Research data

La ricerca è definita esternamente in `Data/research.json`. Il Core mantiene separati `ResearchCatalog`, che contiene le definizioni statiche, e `ResearchState`, che contiene le ricerche completate nella partita corrente. `ResearchSystem` è l'unico punto in cui vengono spesi punti ricerca e completate nuove tecnologie. Gli edifici leggono il requisito da `BuildingDefinition.RequiredResearchId`; `BuildSystem` impedisce la costruzione finché la ricerca richiesta non è completata.

## Heat data and simulation

Heat balance values are externalized in `src/GridPowerTycoon.MonoGame/Data/heat.json`. The Core exposes `HeatSettings` and `HeatSystem`; MonoGame only renders the resulting building state. Heat producers accumulate heat, heat converters absorb heat from active buildings within Chebyshev range and convert it into energy, and buildings exceeding the explosion threshold are marked as exploded.

## Tools e ostacoli naturali

La gestione di asce, mine e pulizia del terreno è nel Core, non nello strato MonoGame. Le quantità e i costi sono configurati in `Data/tools.json`, caricato da `GameDataLoader` dentro `ToolSettings`.

`ToolGenerationSystem` viene aggiornato dalla simulazione e incrementa progressivamente `ResourceState.Axes` e `ResourceState.Mines` rispettando i massimi configurati. `TerrainClearSystem` applica le regole di pulizia: i boschi consumano asce, le montagne consumano mine, e in caso di successo la cella diventa `TileType.Land`.

## Upgrade system

Gli upgrade sono contenuto di gioco e vengono caricati da `Data/upgrades.json`.

Il codice contiene solo le regole generali:
- acquisto;
- controllo costo;
- controllo ricerca richiesta;
- livello massimo;
- applicazione moltiplicatori tramite `UpgradeCalculator`.

I valori di bilanciamento restano nel JSON: costi, moltiplicatori, edificio target e tipo effetto.
