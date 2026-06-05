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

## Step 10 - Upgrade da JSON

Aggiunto sistema upgrade configurabile da `src/GridPowerTycoon.MonoGame/Data/upgrades.json`.

Nuovi namespace/classi Core:
- `GridPowerTycoon.Core.Upgrades.UpgradeDefinition`
- `UpgradeCatalogData`
- `UpgradeCatalog`
- `UpgradeState`
- `UpgradeSystem`
- `UpgradeCalculator`
- `UpgradeEffectType`
- `UpgradeResult`
- `UpgradeFailureReason`

`GameData` e `GameWorld` ora includono catalogo e stato upgrade. `GameDataLoader` carica anche `upgrades.json`.

Sistemi aggiornati per usare i valori effettivi:
- `BuildSystem`: vita edificio e capacità batteria tengono conto degli upgrade;
- `ProductionSystem`: energia/ricerca tengono conto degli upgrade;
- `AutoSellSystem`: vendita automatica tiene conto degli upgrade;
- `HeatSystem`: conversione calore tiene conto degli upgrade;
- `ToolGenerationSystem`: asce/mine tengono conto degli upgrade;
- `ResourceRateSnapshot`: mostra rate stimati già aggiornati.

UI MonoGame:
- pannello `UPGRADES` sulla destra;
- click su upgrade acquista se requisiti/costi sono soddisfatti;
- feedback nella status bar.

Decisione: l'upgrade durata vita non modifica retroattivamente la vita rimanente degli edifici già costruiti; vale per nuove costruzioni e rimpiazzi.

## Step 10B - UI polish applied

The UI was reorganized after the first upgrades implementation. The left menu is now a three-column layout with BUILD, RESEARCH and UPGRADE columns. Upgrade entries show the affected stat and percentage effect. The top bar displays Energy, Research, Money, Axes and Mines using the same visual hierarchy, each with a per-second rate below it. Building details now include cost, size, all production/consumption values, battery capacity, heat conversion input/output, and integer lifetime seconds. Small generators now explicitly show heat input and energy output. The MonoGame window is resizable through `Window.AllowUserResizing = true` and a client-size handler updates the back buffer.

## Step 10C - Top bar energy fill bar

User requested keeping the energy fill bar near SELL because it is useful as a quick visual indicator. `UiRenderer.DrawTopBar` now reserves space before the SELL button, draws the ENERGY/RESEARCH/MONEY/AXES/MINES metrics to the left, and renders a compact `FILL xx%` bar immediately before SELL. The bar is based on `Resources.Energy / Resources.MaxEnergy`.

## Step 10D - Consumo energia operativo e fullscreen

Introdotto il campo `energyConsumptionPerSecond` in `BuildingDefinition`, letto direttamente da `buildings.json`. `ResourceState` espone ora `TrySpendEnergy`, usato dai sistemi di produzione/ricerca, autosell e calore per alimentare gli edifici che richiedono energia operativa. Se l'energia disponibile non basta, l'edificio salta il proprio output per quel tick invece di produrre/vendere/accumulare calore.

Valori dati iniziali:
- `wind_turbine`: `batteryCapacity = 10`, così ogni pala aumenta leggermente la capacità energetica massima;
- `office_small`: `energyConsumptionPerSecond = 0.2`;
- `research_small`: `energyConsumptionPerSecond = 0.5`.

`ResourceRateSnapshot` include `EnergyConsumptionPerSecond` e stima i valori netti considerando produzione, conversione calore, consumi operativi e vendita automatica. `UiRenderer` mostra `ENERGY IN -x/s` nel pannello edificio. `Game1` avvia il gioco in fullscreen borderless tramite `StartFullscreen()`.

## Step 11 - Cloud area unlocks

Added configurable cloud-area unlocking. `area-unlock.json` defines `cloudUnlockMoneyCost` and `cloudUnlockResearchCost`. `MapDefinition` now supports optional `hiddenRows`; visible `C` cells remain clouds, while `hiddenRows` stores the real tile revealed after unlocking. `Tile` now stores `CoveredType` and exposes `RevealCoveredType()`.

Core additions: `AreaUnlockSettings`, `AreaUnlockSystem`, `AreaUnlockResult`, `AreaUnlockFailureReason`. Unlocking a cloud spends money/research and reveals the covered tile. UI additions: clicking a cloud opens a cloud panel with cell, revealed terrain, costs, and UNLOCK button. Status bar reports unlock success/failure.

## Step 11B - Mappa arcipelago più ampia
- La mappa predefinita è stata portata da 20x12 a 64x40 celle.
- Il file `Data/maps/default-map.json` ora rappresenta un arcipelago con più isole e coste più irregolari/naturali.
- L'isola iniziale resta visibile; le isole successive sono coperte da nuvole e usano `hiddenRows` per rivelare terreno, boschi e montagne allo sblocco.
- La modifica è solo dati/documentazione: non cambia sistemi Core, input o rendering.
