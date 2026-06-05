# AI Handoff — GridPowerTycoon

## Stato attuale

GridPowerTycoon è un gioco 2D top-down a griglia sviluppato in C#/.NET 8 con MonoGame DesktopGL. La struttura segue una separazione simile a OpenCad2D: il progetto `GridPowerTycoon.Core` contiene la logica testabile, mentre `GridPowerTycoon.MonoGame` contiene rendering, input e UI.

## Decisioni principali

- Il gioco usa MonoGame DesktopGL.
- La parte Core non deve dipendere da MonoGame.
- I valori di bilanciamento sono caricati da JSON esterni.
- La mappa è caricata da `Data/maps/default-map.json`.
- Gli edifici sono caricati da `Data/buildings.json`.
- Le impostazioni economiche sono caricate da `Data/economy.json`.
- La UI iniziale è custom e usa testo pixel disegnato via codice, senza SpriteFont/MGCB obbligatorio in questa fase.

## Completato

- Solution iniziale.
- Mappa con `GridMap`, `Tile`, `TileType`, `GridPosition`.
- Loader JSON per edifici, economia e mappa.
- BuildSystem Core con risultati espliciti.
- Test del Core passati localmente dall'utente fino allo Step 02.
- Collegamento BuildSystem a MonoGame:
  - selezione edifici dal menu laterale;
  - selezione con tasti 1-4;
  - click sulla mappa per costruire;
  - preview verde/rossa sulla cella;
  - rendering degli edifici costruiti;
  - feedback testuale del risultato di costruzione.

## Prossimo step consigliato

Implementare la simulazione base:

- `ProductionSystem`;
- `LifetimeSystem`;
- `SellSystem`;
- `AutoSellSystem`;
- energia prodotta dalle pale eoliche;
- ricerca prodotta dai centri di ricerca;
- vita residua e stato `Expired`;
- bottone o tasto per vendere energia;
- test automatici per produzione/vendita/scadenza.

## 2026-06-05 — Step 04 preparato: simulazione base

Aggiunti `ProductionSystem`, `LifetimeSystem`, `SellSystem`, `AutoSellSystem` e `GameSimulation`. `GameWorld` espone ora `EconomySettings` perché i sistemi di vendita devono leggere `EnergySellValue`, `ManualSellMultiplier` e `AutoSellMultiplier` dal JSON economia. `ResourceState` ora restituisce la quantità effettivamente rimossa da `RemoveEnergy` e fornisce `RemoveAllEnergy`.

MonoGame istanzia `SellSystem` e `GameSimulation`; ad ogni frame aggiorna la simulazione. La TopBar mostra un pulsante `SELL`, cliccabile per vendere tutta l'energia. Gli uffici (`AutoSellPerSecond`) vendono automaticamente energia se attivi. Gli edifici scaduti non producono più.


## Fix JSON enum converter
- Added JsonStringEnumConverter to GameDataLoader so JSON string enum values such as building category "PowerProducer" deserialize correctly.
- Added regression test for loading building categories from JSON strings.

## Step 06 - Ricerca e sblocco edifici

Aggiunto il sistema ricerca configurabile da `Data/research.json`. Il Core ora contiene `ResearchDefinition`, `ResearchCatalog`, `ResearchState`, `ResearchSystem`, `ResearchResult` e `ResearchFailureReason`. `GameData` e `GameWorld` espongono il catalogo ricerca e lo stato ricerche completate. `BuildSystem` ora blocca gli edifici con `requiredResearchId` non ancora completata restituendo `BuildFailureReason.ResearchRequired`.

La UI MonoGame mostra nel menu laterale gli edifici bloccati e una sezione `RESEARCH` con le ricerche acquistabili usando i punti ricerca prodotti dai centri di ricerca. Gli edifici inizialmente costruibili sono quelli senza ricerca richiesta, in particolare pala eolica e centro di ricerca piccolo. Batterie, ufficio piccolo, pannello solare e generatore piccolo richiedono ricerca.

## Step 06 UI polish - Barre vita e valori stabili

Aggiunta in `MapRenderer` la visualizzazione della vita residua per tutti gli edifici con durata (`LifetimeSeconds > 0`). La barra è disegnata nella parte bassa del rettangolo dell'edificio e usa colore verde sopra il 50%, giallo sotto il 50%, rosso sotto il 25%. Per edifici scaduti la barra è vuota.

Aggiornata la TopBar in `UiRenderer`: energia, ricerca e denaro usano `FormatNumberFixed2`, quindi mostrano sempre due decimali anche quando i valori cambiano velocemente. Il formato abbreviato K/M/B resta disponibile ma con due decimali fissi nella TopBar.

## 2026-06-05 - Step 06 UI polish: resource rates

Added `ResourceRateSnapshot` in `GridPowerTycoon.Core/Economy` to compute estimated per-second rates for Energy, Research and Money without mutating the world. `UiRenderer` now shows a second line under ENERGY, RESEARCH and MONEY in the top bar using fixed two-decimal signed values. Energy rate is net for the next second and accounts for storage cap plus auto-sell; Money rate reflects automatic selling only, not manual SELL button clicks.

## 2026-06-05 - Step 07 Heat and Generators

Implemented configurable heat mechanics. Added `HeatSettings` loaded from `Data/heat.json`, `HeatSystem`, tests for heat accumulation/conversion/out-of-range/explosion and integration with simulation. `GameSimulation` now runs lifetime, direct production, heat conversion, then auto-sell. `ResourceRateSnapshot` estimates heat-converted energy for top-bar rates. `MapRenderer` draws a heat bar at the top of heat-producing/overheated buildings. `UiRenderer` shows heat production, conversion capacity/range and accumulated heat in the selected building panel.

## 2026-06-05 - Step 08 Tools e clearing terreno

Aggiunta la gestione degli strumenti naturali:

- nuovo `ToolSettings` caricato da `Data/tools.json`;
- `ResourceState` ora gestisce `Axes` e `Mines` come valori double, così possono crescere progressivamente nel tempo;
- nuovo `ToolGenerationSystem`, integrato in `GameSimulation`, che genera asce e mine rispettando i massimi configurati;
- nuovo `TerrainClearSystem` con `TerrainClearResult` e `TerrainClearFailureReason`;
- boschi e montagne sono selezionabili dalla mappa;
- pannello UI terreno con costo e risorse disponibili;
- pulsante `CLEAR` per trasformare bosco/montagna in `Land`;
- top bar aggiornata con asce e mine;
- test per generazione strumenti, pulizia boschi/montagne e caricamento `tools.json`.

File dati aggiunto: `src/GridPowerTycoon.MonoGame/Data/tools.json`.


## Step 09A - Bilanciamento strumenti

La generazione strumenti è stata rallentata in `src/GridPowerTycoon.MonoGame/Data/tools.json`:

- `axesPerSecond`: `0.005`;
- `minesPerSecond`: `0.0025`.

Con costo 4 strumenti per rimuovere un ostacolo, i boschi richiedono circa 13m20s e le montagne circa 26m40s. Aggiunto `docs/BALANCE_NOTES.md` per documentare le scelte di bilanciamento. Il prossimo blocco consigliato è il sistema di upgrade da JSON.
