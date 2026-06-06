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

## Step 11C - Large centered archipelago and camera zoom limit
- Default map expanded from 64x40 to 128x80 cells, giving 4x the previous area.
- Initial visible island is now centered in the map; surrounding islands remain cloud-covered and reveal their real terrain from hiddenRows.
- Camera now starts centered on the map/initial island instead of the top-left corner.
- Camera minimum zoom increased from 0.40 to 0.75 so the player cannot zoom out to see too much of the archipelago at once and must pan to explore.
- Wind turbine battery capacity contribution reduced from 10 to 5 in buildings.json.

## Step 12 - Save/load JSON

Added `GridPowerTycoon.Core.Save` with `SaveGame`, `SaveResources`, `SaveMap`, `SaveTile`, `SaveBuildingInstance` and `SaveGameService`.

`SaveGameService.CreateSave(GameWorld)` snapshots resources, full map state, building instances, completed research and upgrade levels. `RestoreWorld(SaveGame, GameData)` rebuilds a `GameWorld` from the save while reusing the current game data catalogs/settings.

Support methods were added:
- `ResourceState.Restore(...)` restores resources safely.
- `BuildingInstance.Restore(...)` recreates instances with lifetime, heat and state.
- `UpgradeState.SetLevel(...)` restores purchased upgrade levels.

MonoGame integration:
- Save path: `AppContext.BaseDirectory/Saves/savegame.json`.
- Startup loads the save if it exists, otherwise starts a new world from `default-map.json`.
- F5 saves.
- F9 loads.
- ESC saves then exits.
- Bottom status bar shows save/load messages and key hints.

Tests added under `tests/GridPowerTycoon.Core.Tests/Save/SaveGameServiceTests.cs` for round-tripping resources, map state, buildings, research and upgrades.


## Step 12A

Bilanciamento: il pannello solare ora ha `lifetimeSeconds: 180` in `Data/buildings.json`, invece dei 30 secondi iniziali. La modifica è solo dati/documentazione e non cambia la logica.

## Step 13 - Offline progress
- Added `OfflineProgressSystem` and `OfflineProgressResult` in Core.
- Save files already contained `SavedAt`; startup now reads the save, restores the world, applies capped offline progress using `EconomySettings.MaxOfflineSeconds`, then writes the updated save back to avoid repeated offline gain.
- Offline progress simulates in 1-second internal steps, so energy consumers, auto-sell, lifetime and tool generation remain consistent.
- Offline heat conversion runs, but explosions are disabled while the player is away via `HeatSystem.Update(deltaSeconds, allowExplosions: false)`. Buildings can still expire offline.
- Startup status message summarizes offline energy, research, money, axes, mines and expired buildings.


## Step 13 fix - OfflineProgressResult None

Fixed `OfflineProgressResult.None` constructor arguments after adding `BuildingsExploded`, so the Core project compiles with the offline progress model.

## Step 14 - Cloud area group unlock

Implemented area-based cloud unlocking. `AreaUnlockSettings` now includes `CloudUnlockRadius` and `MaxCloudTilesPerUnlock`. `AreaUnlockSystem.UnlockCloud` reveals the clicked cloud plus nearby connected cloud cells within a Manhattan radius, limited by the max tile count. `AreaUnlockResult` now reports `TilesUnlocked` and exposes the revealed tile list while keeping `RevealedTileType` for compatibility with older UI/status logic. The UI cloud panel now shows radius, max tiles, estimated tiles to unlock, and status reports how many tiles were unlocked.

Current default in `Data/area-unlock.json`: radius 2, max 9 tiles per unlock, fixed action cost 2500 money and 25 research.

## Step 15 - Stato operativo edifici

Aggiunto namespace `GridPowerTycoon.Core.Operations` con `BuildingOperationalState`, `BuildingOperationalStatus` e `BuildingOperationalStatusCalculator`. Il calcolatore non modifica il mondo: produce solo diagnostica per UI/test. La UI usa questo stato nel pannello edificio per mostrare `ACTIVE`, `NO ENERGY`, `EXPIRED`, `EXPLODED`, `HEAT WARNING`, `NO HEAT CONVERSION`, output effettivi e output lordi quando l'edificio non sta producendo. Aggiunti test in `tests/GridPowerTycoon.Core.Tests/Operations/BuildingOperationalStatusCalculatorTests.cs`.
## Step 16 - Primi edifici mid-game

Aggiunto il primo blocco di progressione industriale configurato nei JSON:

- Centrale a carbone: 155k$, 680 calore/s, vita 300s.
- Ufficio grande: 150k$, vende 200 energia/s e consuma energia operativa.
- Generatore medio: 100k$, converte 1k calore/s, raggio 1.
- Centrale a gas: 7.5M$, 25k calore/s, vita 300s.
- Centro ricerca grande: 10M$, produce 100 ricerca/s e consuma energia operativa.

Aggiunte ricerche collegate e primi upgrade specifici. La UI BUILD/RESEARCH/UPGRADE include i nuovi elementi. La formattazione numerica della UI ora usa prefissi SI: k, M, G, T, P, E, Z, Y.


## Step 17 - Upgrade multi-livello

Implementazione preparata sul sorgente corrente.

Modifiche principali:
- `UpgradeDefinition` ora contiene `CostGrowthMultiplier`.
- `UpgradeSystem` calcola costo denaro/ricerca del prossimo livello con `GetMoneyCost` e `GetResearchCost`.
- `UpgradeResult` ora include `NewLevel`.
- La UI degli upgrade mostra `LV current/max`, effetto e costo `NEXT`.
- `UpgradeEffectType` include `MultiplyHeatProduction`.
- `UpgradeCalculator.GetHeatPerSecond` applica gli upgrade di produzione calore.
- `HeatSystem`, `ResourceRateSnapshot`, `BuildingOperationalStatusCalculator` e pannello UI usano la produzione calore effettiva.
- `upgrades.json` è stato aggiornato con upgrade ripetibili e costo crescente.
- Aggiunti test per upgrade multi-livello e costo crescente.

Nota: il container non ha `dotnet`; eseguire build/test localmente.

## Step 18 - Gestori automatici

Implementato `GridPowerTycoon.Core.Managers` con `ManagerSystem` e `ManagerRenewalResult`.

Le ricerche supportano ora il campo `managedBuildingIds`. Quando una ricerca è completata, ogni edificio scaduto con `DefinitionId` incluso in quel campo può essere rinnovato automaticamente se ci sono soldi sufficienti. Gli edifici esplosi non vengono rinnovati automaticamente.

`GameSimulation` esegue ora l'ordine:

1. LifetimeSystem
2. ManagerSystem
3. ProductionSystem
4. HeatSystem
5. AutoSellSystem
6. ToolGenerationSystem

Anche `OfflineProgressSystem` applica il manager system dopo la scadenza degli edifici, così i gestori funzionano durante il guadagno offline.

La UI mostra `MANAGED YES/NO` nel pannello edificio e visualizza un messaggio quando un manager rinnova uno o più edifici.

## 2026-06-06 - Step 17A Upgrade cost growth balancing

Richiesta utente: aumentare la moltiplicazione del costo a ogni livello di aggiornamento. Modificato `src/GridPowerTycoon.MonoGame/Data/upgrades.json`, alzando i `costGrowthMultiplier` da ~1.65-1.85 a ~2.15-2.60. Nessuna modifica di logica richiesta; il sistema multi-livello usa già `baseCost * costGrowthMultiplier ^ currentLevel`.


## Step 18A - Restore edifici esplosi

- Corretto il ripristino manuale: il pulsante di ripristino ora è disponibile anche per edifici in stato `Exploded`.
- Il ripristino manuale paga il costo dell'edificio, azzera il calore accumulato e riporta lo stato ad `Active`.
- I gestori automatici continuano a non rinnovare edifici esplosi: il ripristino degli esplosi resta una scelta manuale del giocatore.

## 2026-06-06 - Step 18B: fixed properties column

The UI now has a fixed right-hand properties panel. MapInputController keeps a SelectedTilePosition in addition to selected building/terrain/cloud references. MapRenderer highlights the selected tile separately from hover/build preview. UiRenderer uses a unified property table for buildings, terrain and clouds, with stable row labels (TYPE, STATE, COST, LIFE, MANAGED, ENERGY IN/OUT, HEAT, RESEARCH, AUTO SELL, BATTERY, etc.). Context buttons are still routed through the same UiRenderer hit-test methods but are anchored in the fixed properties panel.

## 2026-06-06 - Step 18C: scrollable left columns

Added independent mouse-wheel scrolling for the fixed left UI columns BUILD, RESEARCH and UPGRADE. `UiRenderer` now keeps separate scroll offsets for each column, shifts button rectangles accordingly, clamps offsets to visible content height, and shows a small `MORE/TOP/SCROLL` hint in column headers when content overflows. Hit testing now respects the scrolled button positions and ignores off-screen buttons.

`Game1.Update` forwards mouse-wheel deltas to `UiRenderer.HandleScroll(...)`. `CameraInputController` accepts an optional UI hit-test delegate and skips mouse wheel zoom / middle-button pan when the cursor is over UI, so scrolling menus no longer zooms the map.

## 2026-06-06 - Step 18D Game command buttons

Added explicit bottom status-bar command buttons in MonoGame UI: SAVE, LOAD, NEW, VIEW, EXIT. Game1 now handles these clicks through UiRenderer hit testing. NEW reloads the map from Data/maps/default-map.json and creates a fresh GameWorld using the already loaded GameData. VIEW toggles fullscreen/windowed mode and recenters the camera. Existing shortcuts are preserved: F5 save, F9 load, ESC save+exit.

## 2026-06-06 - Step 18E: demolizione manuale edifici

Aggiunta la demolizione manuale degli edifici dal pannello proprietà fisso. Ogni edificio selezionato mostra ora l'azione `DEMOLISH`; gli edifici scaduti o esplosi continuano a mostrare anche `REPLACE`/`RESTORE`. La demolizione rimuove l'istanza dal mondo, libera tutte le celle occupate e annulla gli effetti immediati di capacità batteria, riportando `MaxEnergy` almeno al valore iniziale configurato e clampando l'energia accumulata se necessario.

Sono stati aggiunti `BuildSystem.Demolish(...)`, `BuildSystem.DemolishAt(...)` e `GameWorld.RemoveBuilding(...)`. La UI MonoGame intercetta il pulsante DEMOLISH e pulisce la selezione edificio lasciando selezionata la cella. Aggiunti test Core per demolizione, liberazione celle multi-tile, riduzione capacità batteria e caso edificio inesistente.

## 2026-06-06 - Step 18F: feedback fondi insufficienti

Il rinnovo manuale degli edifici scaduti/esplosi ora distingue tra azione disponibile e azione non finanziabile. `BuildSystem` espone `CanReplaceExpired(...)`, così la UI può verificare lo stato prima del click. Nel pannello proprietà il pulsante `REPLACE`/`RESTORE` diventa grigio e non è cliccabile quando il giocatore non ha abbastanza denaro; il testo mostra `NEED $...` e la riga `ACTION` indica che mancano soldi per sostituire/ripristinare.

La costruzione continua invece a permettere la selezione del tipo edificio anche se al momento non ci sono fondi. Se il giocatore clicca una cella e `BuildSystem.Build(...)` fallisce con `NotEnoughMoney`, `MapInputController` conserva posizione e motivo dell'ultimo fallimento; `MapRenderer` disegna sulla cella un marker rosso con simbolo `$` sbarrato. La status bar continua a mostrare il motivo testuale (`BUILD FAILED NotEnoughMoney`). Nel menu BUILD i costi non finanziabili sono evidenziati come `NEED $...`.

## 2026-06-06 - Step 18G: conferma demolizione e proprietà celle vuote

Aggiornata la UX del pannello proprietà. Il pulsante `DEMOLISH` è stato spostato nella parte alta del pannello edificio, subito sotto nome/cella, e non demolisce più al primo click: il primo click arma la conferma e trasforma il pulsante in `CONFIRM DEMOLISH`; il secondo click sullo stesso edificio esegue davvero la demolizione. Cliccare sulla mappa o selezionare un altro edificio annulla la conferma pendente.

La selezione di uno strumento BUILD ora parte disattivata all'avvio/nuova partita e viene disattivata quando il giocatore clicca un edificio già costruito. Questo permette di ispezionare edifici senza lasciare attivo il tool di costruzione e riduce i click accidentali sulle celle libere. Le celle senza edifici ora mantengono comunque il dettaglio nel pannello proprietà: le celle `Land` sono mostrate come `Plain` con stato `FREE / BUILDABLE`, mentre acqua, foreste, montagne e cloud mantengono una descrizione coerente.

Rinominata la riga poco leggibile `HEAT CONV` in `HEAT TO ENERGY`, con valore esplicito `HEAT .../S -> ENERGY .../S R...`.


## 2026-06-06 - Step 18H: proprietà HEAT IN

Aggiunta la riga `HEAT IN` nel pannello proprietà fisso. Per gli edifici convertitori di calore mostra il consumo/capacità di assorbimento calore al secondo e il raggio operativo, mentre `HEAT TO ENERGY` resta dedicato all'energia prodotta dalla conversione. Questo rende più leggibile la distinzione fra calore prodotto, calore accumulato, calore assorbito e energia ottenuta.


## 2026-06-06 - Step 18I: testo HEAT IN più leggibile

Resi più espliciti i testi del pannello proprietà per i convertitori di calore. `HEAT IN` ora usa la forma `ABSORBS .../S, RANGE ... CELLS` invece dell'abbreviazione `R...`; `HEAT TO ENERGY` usa `PRODUCES .../S ENERGY`. Aggiornati anche i dettagli estesi degli edifici per evitare abbreviazioni poco chiare.

## 2026-06-06 - Step 18J: build tool cancel e dettagli strumento

Migliorata la sicurezza dello strumento di costruzione attivo. Il giocatore ora può annullare il tool BUILD con click destro; premere di nuovo lo stesso tasto numerico o cliccare di nuovo lo stesso edificio nel menu BUILD disattiva lo strumento invece di lasciarlo armato. Quando un tool BUILD è selezionato, la status bar indica chiaramente `LEFT CLICK BUILD, RIGHT CLICK CANCEL`.

Il pannello proprietà mostra anche il tool di costruzione attivo: se non è selezionata nessuna cella, appare una scheda `BUILD TOOL` con categoria, nome, costo, dimensione e azione consigliata. Se è selezionata una cella vuota, la riga `BUILD TOOL` e l'azione spiegano se si può costruire, se mancano soldi o se la cella non è edificabile. Aggiunto `InputManager.IsRightClickPressed()` per gestire il cancel senza interferire con il click sinistro.

## 2026-06-06 - Step 18K: riepilogo economico properties panel

Aggiunto un riepilogo economico direttamente nel pannello proprietà e nella scheda del BUILD TOOL. Le righe principali ora distinguono `BUILD COST`, `NEXT UPGRADE`, `MONEY/S`, `NET ENERGY` e `PAYBACK`, così il giocatore può leggere a colpo d'occhio costo iniziale, prossimo investimento disponibile, flusso economico stimato, bilancio energetico e tempo stimato di rientro.

La stima economica usa il valore configurato di vendita dell'energia e i moltiplicatori manuale/autosell già presenti in `EconomySettings`. Per gli edifici selezionati usa lo stato operativo effettivo calcolato da `BuildingOperationalStatusCalculator`; per il BUILD TOOL usa invece una previsione basata sulla definizione dell'edificio e sugli upgrade già acquistati. I valori di `MONEY/S` e `PAYBACK` sono marcati come `EST`, perché dipendono dal contesto reale di accumulo/vendita energia e, per gli autoseller, dalla disponibilità di energia da vendere.

## 2026-06-06 - Step 18L: leggibilità LIFE/PAYBACK e righe proprietà

Rifinito il pannello proprietà dopo il riepilogo economico. `LIFE` ora separa sempre il numero dall'unità (`50 S / 300 S`), così la durata è più leggibile. `PAYBACK` non usa più il carattere `<`, che nel font del gioco poteva apparire come `?`, e non mostra più l'abbreviazione `EST`; i valori sono ora espressi come `ABOUT ... SEC/MIN/H`. Anche `MONEY/S` usa `APPROX` invece di `EST` per evitare abbreviazioni poco chiare.

Corretto anche il background alternato delle righe del properties panel: l'alternanza ora dipende dall'indice reale della riga e non dalla coordinata verticale, quindi resta stabile anche quando cambia l'offset iniziale tra edifici, celle vuote e tool di costruzione.

## 2026-06-06 - Step 18M: pannello sinistro più informativo e bottoni uniformi

Uniformate le tre colonne laterali `BUILD`, `RESEARCH` e `UPGRADE`: i pulsanti ora usano la stessa altezza e lo stesso passo di scroll, così la UI non cambia densità tra una colonna e l'altra. La colonna `BUILD` non mostra più soltanto nome e costo, ma include anche `BUILD COST`, `NET ENERGY`, informazione sintetica sul calore (`HEAT`, `HEAT IN` o `NO HEAT`) e una riga di descrizione/purpose ricavata dalla definizione edificio. Questo permette di capire a cosa serve una costruzione prima di selezionarla o piazzarla.

Anche `RESEARCH` ora mostra più contesto direttamente nel pulsante: stato/costo, cosa sblocca o cosa gestisce, e descrizione breve della ricerca. La colonna `UPGRADE`, già considerata chiara, è stata mantenuta come riferimento visivo ma allineata alla nuova altezza comune; in più mostra la descrizione breve dell'upgrade come quarta riga quando disponibile.

## 2026-06-06 - Step 18N: glyph mancanti nel font pixel

Il testo UI non usa un font TrueType: viene disegnato da `PixelTextRenderer`, un renderer bitmap 5x7 interno basato su una tabella manuale di glifi. Per questo motivo i caratteri non presenti nella tabella venivano sostituiti con `?`, in particolare apostrofo, apostrofo tipografico, lettere accentate italiane e separatori usati nei testi recenti.

Aggiunti glifi per apostrofo dritto, apostrofo tipografico, barra verticale, underscore e vocali accentate maiuscole più comuni (`À`, `Á`, `È`, `É`, `Ì`, `Í`, `Ò`, `Ó`, `Ù`, `Ú`). Poiché `DrawString` converte i caratteri con `char.ToUpperInvariant`, questi glifi coprono anche le corrispondenti lettere minuscole presenti nei JSON (`à`, `è`, `ù`, ecc.).

## 2026-06-06 - Step 19A: normalizzazione pannello sinistro

Avviata la Milestone 19 sulla leggibilità UI. Le tre colonne del pannello sinistro continuano a usare la stessa altezza e lo stesso passo di scroll, ma ora condividono anche coordinate interne comuni per titolo, riga stato/costo, riga effetto primario e descrizione. Questo riduce gli allineamenti divergenti tra BUILD, RESEARCH e UPGRADE.

La colonna BUILD è stata resa più simile alla colonna UPGRADE: rimosso il riquadro icona grande che accorciava troppo nome e costo, sostituito da una barra verticale colorata di categoria. I pulsanti edificio hanno quindi più spazio per mostrare nome, `BUILD COST`/`NEED`/requisito ricerca, `NET ENERGY`, impatto sul calore e descrizione breve. Gli edifici bloccati indicano la ricerca richiesta quando disponibile.

La colonna RESEARCH ora mostra il prerequisito specifico mancante invece del generico `REQ RESEARCH`, quando il dato è disponibile nel catalogo. Le righe interne sono allineate alle stesse coordinate usate dalle altre colonne.

## 2026-06-06 - Step 19B: colonne sinistre più larghe e titoli più leggibili

Allargato il pannello laterale sinistro per dare più respiro alle tre colonne `BUILD`, `RESEARCH` e `UPGRADE`. La larghezza colonna passa a 230 px e il pannello laterale a 740 px; i pulsanti restano uniformi tra le tre colonne, ma crescono leggermente in altezza per ospitare una prima riga più grande.

I titoli dei pulsanti nelle tre colonne ora sono disegnati a scala 2, così nomi come `PALA EOLICA` e `BATTERIA` risultano più evidenti. In BUILD è stata rimossa la numerazione testuale davanti al nome (`1 PALA EOLICA` diventa `PALA EOLICA`), lasciando il pulsante più pulito. Anche le etichette delle risorse in alto (`ENERGY`, `RESEARCH`, `MONEY`, `AXES`, `MINES`) sono state aumentate per essere più leggibili a colpo d'occhio.

## 2026-06-06 - Step 19C: stati leggibili nel pannello sinistro

Rifinita la leggibilità delle tre colonne laterali aggiungendo badge di stato direttamente nella prima riga dei pulsanti. BUILD ora distingue visivamente `READY`, `ACTIVE`, `LOCKED` e `NEED $`; RESEARCH distingue `READY`, `DONE`, `LOCKED` e `NEED R`; UPGRADE distingue `READY`, `MAX`, `LOCKED` e `NEED`. Le righe di costo/stato sono state rese più esplicite, usando testi come `READY - BUILD COST ...`, `NEED MONEY - ...`, `LOCKED - ...`, `DONE - RESEARCH COMPLETED` e `NEED RESOURCES - ...`.

L'obiettivo è ridurre l'ambiguità del pannello sinistro: il giocatore deve capire subito se un'azione è disponibile, già completata, bloccata da ricerca o non acquistabile per risorse insufficienti, senza dover cliccare o leggere la status bar.

## 2026-06-06 - Step 19D: descrizioni operative pannello sinistro

Rifinite le righe informative dei pulsanti laterali. In `UiRenderer`, BUILD non usa più una combinazione generica `NET ENERGY | HEAT` come informazione principale, ma una frase operativa calcolata dai valori effettivi con upgrade correnti: `PRODUCES ... ENERGY`, `PRODUCES ... HEAT`, `CONVERTS ... HEAT`, `ADDS ... STORAGE`, `PRODUCES R...`, `SELLS ... ENERGY` o `USES ... ENERGY`. La riga di supporto spiega il ruolo pratico: generatori che necessitano calore vicino, produttori di calore che richiedono un generatore, batterie che evitano sprechi, uffici che convertono energia accumulata in denaro e centri ricerca che sbloccano tecnologie.

RESEARCH usa ora `GetResearchActionText(...)` per chiarire se la ricerca crea un nuovo edificio o abilita automazioni. UPGRADE usa `GetUpgradeTargetText(...)` per mostrare il bersaglio dell'upgrade invece di ripetere una descrizione lunga spesso meno utile nel pulsante. Questo step non modifica regole di gioco o bilanciamento, solo la leggibilità del pannello sinistro.

## 2026-06-06 - Step 19E: purpose nel properties panel

Prosegue la Milestone 19 sulla leggibilità UI. Il pannello proprietà ora espone una riga `PURPOSE` subito dopo `TYPE`, così anche quando si seleziona un edificio, una cella vuota, una foresta, una montagna, una cloud area o un tool BUILD attivo viene spiegato in modo sintetico a cosa serve quell'elemento o quale azione è possibile.

Per gli edifici la riga `PURPOSE` è derivata dalla categoria funzionale: produzione energia, storage, vendita automatica, ricerca, produzione calore, conversione calore o edificio economico avanzato. Per celle vuote e terreni bloccanti la riga chiarisce se la cella è edificabile, se blocca la costruzione, se può essere liberata con asce/mines o se una cloud area permette di sbloccare nuova mappa. Non sono state modificate regole di gioco o bilanciamento.


## 2026-06-06 - Step 19F: single left panel mode

Riorganizzata l'interfaccia principale per evitare le tre colonne laterali sempre aperte. `BUILD`, `RESEARCH` e `UPGRADE` sono ora modalità del pannello sinistro: è visibile una sola lista alla volta, selezionata dalla nuova fascia di navigazione sotto la top bar delle risorse. La larghezza della colonna unica è maggiore rispetto alle vecchie colonne affiancate, così nomi e testi descrittivi hanno più spazio e non devono essere abbreviati inutilmente.

I comandi di partita `NEW`, `LOAD`, `SAVE` ed `EXIT` sono stati spostati nella stessa fascia dei tab di navigazione, allineati verso destra prima del properties panel. Il vecchio pulsante `VIEW` non viene più disegnato in questa fascia, perché la destinazione naturale sarà una futura sezione impostazioni. La status bar resta in basso per messaggi operativi.

Rimossa anche la ridondanza visiva dei badge di stato nei pulsanti laterali: lo stato resta nella seconda riga (`READY - ...`, `NEED MONEY - ...`, `LOCKED - ...`, ecc.) e il pulsante BUILD attivo continua a essere evidenziato con bordo giallo. Cambiare tab verso `RESEARCH` o `UPGRADE` disattiva l'eventuale tool BUILD, evitando costruzioni involontarie mentre si sta consultando un'altra sezione.

## 2026-06-06 - Step 19G: BUILD column full-width cards
- Expanded the single left panel width so one visible section has more room for full building names and explanations.
- BUILD cards are taller and now use an extra detail row with size and functional category.
- BUILD titles allow longer names, reducing truncation such as short `PANNELLO SO.` / `CENTRO DI R.` style labels.
- Kept active build tool highlight as the yellow card border.

## 2026-06-06 - Step 19H: RESEARCH column full-width cards

Stato: preparato.

La sezione `RESEARCH` del pannello sinistro a colonna unica è stata portata allo stesso modello delle schede BUILD full-width. Le ricerche ora usano più larghezza disponibile, titolo grande, stato/costo più leggibile, indicazione chiara di cosa sbloccano o gestiscono, descrizione operativa e riga finale di dettaglio. È stata aggiunta una barra verticale colorata per distinguere ricerche completate, bloccate, ricerche che sbloccano edifici e ricerche di automazione/management.

## Step 19I - UPGRADE full-width cards

- Updated the single left panel `UPGRADE` section to match the full-width card approach already used by `BUILD` and `RESEARCH`.
- Upgrade cards now show: title, status/cost, effect, target, and a final level/detail line.
- Added effect-colored left accent bars for upgrade categories.
- Completed upgrades use a green accent/status; locked upgrades use a muted accent.
- Gameplay logic and upgrade cost calculations were not changed.

## Step 19J - Left panel clipping and full-width status bar

- Added an explicit bottom status bar rectangle spanning the full viewport width.
- The properties panel and left panel now stop above the status bar.
- Left panel card visibility now uses an explicit list rectangle, so BUILD/RESEARCH/UPGRADE cards are drawn only when fully inside the list area.
- This prevents scrolled cards from overlapping the tab strip or top resource bar.
- No gameplay logic was changed.

## Step 19J Fix1 - Top mask draw order and removed scroll hint

- Fixed the remaining scroll overlap by drawing the scrollable left menu before the top resource bar and tab bar.
- The top resource bar and tab bar are now redrawn after the left list, acting as a visual mask for any scrolled card edge.
- Removed the `MORE/TOP/SCROLL` hint text from the gap between tabs and the left menu.
- Status bar full-width behavior from Step 19J is retained.

## Step 19J Fix2 - StatusBarHeight compile fix

- Added the missing `StatusBarHeight` constant declaration used by the Step 19J status bar/layout calculations.
- No behavioral changes beyond making the previous Fix1 compile.

## Step 19J Fix3 - Card-aligned left panel scrolling

- Changed left panel scrolling to advance exactly by one card stride.
- Scroll offsets are now snapped to whole-card positions.
- This prevents the empty gap at the top of BUILD/RESEARCH/UPGRADE lists after scrolling.
- The previous top bar clipping and full-width status bar behavior is retained.

## Step 19K - Left panel card state polish

- Refined card status text/color consistency across BUILD, RESEARCH and UPGRADE.
- BUILD cards now show `ACTIVE - BUILD COST ...` when the building tool is selected.
- BUILD cards now show `NEED MONEY - BUILD COST ...` instead of only the amount.
- RESEARCH cards now visually distinguish ready, locked and insufficient research states.
- UPGRADE cards now visually distinguish ready, locked and insufficient resource states.
- No gameplay, economy, research or upgrade logic was changed.

## Step 19L - Properties panel readability polish

- Improved properties panel readability without changing gameplay logic.
- Property rows now use clearer display labels, for example `MONEY / S`, `RESEARCH / S`, `STORAGE`, `FOOTPRINT`, and `REVEALS`.
- Value text has more horizontal room, reducing truncation for longer values such as heat conversion descriptions.
- Added subtle separator lines between major property groups without consuming extra row height.
- Property labels now use group-oriented colors for identity, economy, lifetime, energy, heat, research/sell and terrain/action rows.

## Step 19M - Command strip future sections and status alignment

- Added disabled future section buttons in the command strip: `STATS`, `HELP`, `SETTINGS`.
- Future buttons are drawn only when there is enough space before `NEW/LOAD/SAVE/EXIT`, avoiding overlap on narrower windows.
- Removed the old `VIEW` command strip residue from the unused command button helper.
- Status bar text now uses the real status bar rectangle for vertical positioning instead of a fixed viewport offset.
- No gameplay logic was changed.

## Step 19M Fix1 - Restore VIEW command

- Restored the `VIEW` command in the command strip between `SAVE` and `EXIT`.
- `VIEW` again uses the existing fullscreen/windowed toggle hit-test rectangle.
- Command strip width calculation now includes `VIEW`, so future disabled placeholders still disappear when there is not enough horizontal space.
- No gameplay logic was changed.

## Step 19N - Responsive command strip placeholders

- Improved command strip responsiveness after restoring `VIEW`.
- Future disabled placeholders `STATS`, `HELP`, `SETTINGS` are now drawn progressively when each one fits.
- This avoids the previous all-or-nothing behavior where all future placeholders disappeared if `SETTINGS` did not fit.
- Added a slightly larger safety gap before the properties panel area when positioning `NEW/LOAD/SAVE/VIEW/EXIT`.
- No gameplay logic was changed.

## Step 20A - Base building roles and first balance pass

- Started Milestone 20 gameplay balancing.
- Updated `buildings.json` and `economy.json` to establish a clearer early -> mid -> industrial progression.
- Preserved the immediate first action: starting money now matches the new first wind turbine cost.
- Reduced the extreme `$1` wind turbine distortion by moving wind cost to `$10`.
- Rebalanced starter storage, automation, research, solar/generator, coal, gas and large research values.
- Added detailed balancing notes to `docs/BALANCE_NOTES.md`.

## Step 20B - Research and upgrade balance alignment

- Updated `research.json` to align unlock costs with the revised Step 20A building economy.
- Updated `upgrades.json` to avoid old over-inflated mid/industrial upgrade prices.
- Reduced repeated production upgrade multipliers from `1.5` to `1.35` where appropriate.
- Reduced lifetime upgrade multipliers from `2.0` to `1.5` where appropriate.
- Kept early unlocks accessible while lowering mid/industrial unlock costs.
- Updated `docs/BALANCE_NOTES.md`.

## Step 20C - Heat, explosion and conversion balance

- Updated `heat.json` thresholds: warning `250`, explosion `500`.
- Rebalanced thermal buildings and converters in `buildings.json`.
- Reduced heat production/conversion upgrade multipliers to `1.30` in `upgrades.json`.
- Added `HEAT RISK` row to the properties panel in `UiRenderer.cs`.
- Heat risk now reports controlled state or approximate time to explosion.
- Updated `docs/BALANCE_NOTES.md`.

## Step 20D - Expansion, tools and map unlock pacing

- Updated `tools.json` to make forest/mountain clearing fit the revised economy.
- Increased passive tool generation and storage caps.
- Reduced forest clear cost from `4` axes to `3`.
- Updated `area-unlock.json`: cloud unlock now costs less money but more research.
- Added detailed pacing notes to `docs/BALANCE_NOTES.md`.

## Step 20E - Early and mid-game progression pacing

- Adjusted early building costs so the first research/automation/storage choices arrive sooner.
- Reduced `research_small` cost and output to make it accessible earlier without flooding research.
- Reduced solar/small-generator setup cost to establish the first thermal loop sooner.
- Reduced coal/medium-generator/large-office tier costs to create a clearer mid-game transition.
- Reduced gas/research-large industrial costs and outputs to keep the scale consistent.
- Updated matching research and upgrade costs.
- Updated `docs/BALANCE_NOTES.md`.

## Step 21A - Clear operational issue text

- Started Milestone 21 gameplay feedback/readability.
- Added an `ISSUE` row to selected building properties in `UiRenderer.cs`.
- The row explains why a building is stopped, risky, expired, exploded, or operating correctly.
- No simulation, economy, balance or save-data logic was changed.

## Step 21B - Map operational state badges

- Added small operational badges directly on map buildings in `MapRenderer.cs`.
- Badges use the same `BuildingOperationalStatusCalculator` state used by the properties panel.
- Badge meanings:
  - `E`: no energy;
  - `G`: heat producer needs a generator/converter in range;
  - `H`: heat warning;
  - `T`: expired/timed out;
  - `X`: exploded.
- No simulation, economy, balance or save-data logic was changed.

## Step 21C - Status bar badge legend

- Added a responsive badge legend to the status bar in `UiRenderer.cs`.
- The legend explains the map operational badges introduced in Step 21B:
  - `E`: energy issue;
  - `G`: generator/conversion issue;
  - `H`: heat warning;
  - `T`: timed out/expired;
  - `X`: exploded.
- The legend is drawn only when the status bar has enough horizontal space.
- No simulation, economy, balance or save-data logic was changed.

## Step 21D - Clear failure messages

- Improved status bar failure messages in `UiRenderer.cs`.
- Build, research, terrain clear, area unlock and upgrade failures now show actionable text instead of raw enum names.
- Examples:
  - `BUILD FAILED: NEED MONEY`
  - `BUILD FAILED: CELL OCCUPIED`
  - `RESEARCH FAILED: MISSING PREREQUISITE`
  - `CLEAR FAILED: NEED AXES`
  - `UNLOCK FAILED: NEED RESEARCH`
  - `UPGRADE FAILED: MAX LEVEL`
- No simulation, economy, balance or save-data logic was changed.

## Step 21E - Status bar selected building feedback

- Improved status bar priority/readability in `UiRenderer.cs`.
- When a map building is selected and no newer action/save/demolish message has priority, the status bar now shows the selected building operational summary.
- Examples:
  - `PALA EOLICA: ACTIVE - PRODUCING ENERGY`
  - `CENTRO DI RICERCA PICCOLO: NEED STORED ENERGY`
  - `PANNELLO SOLARE: PLACE GENERATOR IN RANGE`
  - `CENTRALE A CARBONE: HEAT WARNING 320/500`
  - `...: EXPIRED - REPLACE OR DEMOLISH`
  - `...: EXPLODED - RESTORE OR DEMOLISH`
- The existing action result/save/load/demolish confirmation messages still override the selected-building summary.
- Status bar is now included in UI hit-testing so clicks on the bottom bar do not pass through to the map.
- No simulation, economy, balance or save-data logic was changed.

## Step 21F - Feedback system documentation

- Added `docs/FEEDBACK_SYSTEM.md`.
- Documented the player feedback surfaces introduced in Milestone 21:
  - properties panel `ISSUE`;
  - map operational badges;
  - status badge legend;
  - selected-building status bar summary;
  - failure message style;
  - heat feedback semantics.
- No code, simulation, economy, balance or save-data logic was changed.

## Step 22A - Manager visibility in properties

- Started Milestone 22 automation/manager work.
- Added a `MANAGER` row to selected building properties in `UiRenderer.cs`.
- The row shows:
  - `-` for buildings that have no manager research;
  - `UNLOCK: <research name>` for buildings that can be managed but whose manager is not unlocked yet;
  - `ACTIVE: <research name>` for buildings currently covered by an unlocked manager.
- No manager behavior, simulation, economy, balance or save-data logic was changed.

## Step 22B - Manager renewal feedback

- Improved manager feedback in `Game1.cs`.
- The status bar now reports both successful automatic renewals and manager failures caused by insufficient money.
- Added anti-spam tracking so repeated identical manager failures do not continuously overwrite newer user feedback.
- Examples:
  - `MANAGER RENEWED 2 BUILDING(S) -$20`
  - `MANAGER RENEWED 1 -$10 | NEED MONEY FOR 3`
  - `MANAGER NEEDS MONEY FOR 2 EXPIRED BUILDING(S)`
- No manager behavior, simulation rules, economy, balance or save-data logic was changed.

## Step 22C - Manager map badge

- Added a small `M` badge to managed buildings in `MapRenderer.cs`.
- The badge is drawn at the bottom-left of the building tile and does not replace operational badges such as `E/G/H/T/X`.
- The badge uses `ManagerSystem.IsManaged`, the same manager check used by the properties panel.
- No manager behavior, simulation, economy, balance or save-data logic was changed.

## Step 22D - Manager research impact text

- Improved manager research card details in `UiRenderer.cs`.
- Manager research cards now show the impact on the current world:
  - `WILL MANAGE 0 BUILT`
  - `WILL MANAGE 3 BUILT`
  - `MANAGING 5 BUILT`
  - `MANAGING 5 BUILT | EXPIRED 2`
- The count is based on current building instances whose definition id appears in the research `managedBuildingIds`.
- No manager behavior, simulation, economy, balance or save-data logic was changed.

## Step 23A - Cloud unlock map preview

- Started Milestone 23 map expansion/obstacles.
- Updated `MapRenderer` to receive `AreaUnlockSystem`.
- Added cloud unlock preview on the map for the selected cloud tile.
- Preview uses `AreaUnlockSystem.GetUnlockableCloudTiles`, so it matches the real unlock logic.
- Preview coloring:
  - blue overlay/border when the selected cloud can be unlocked;
  - red overlay/border when the selected cloud cannot currently be unlocked.
- The selected cloud shows a small number marker indicating how many cloud tiles will be revealed.
- Updated `Game1.cs` to pass `_areaUnlockSystem` and selected cloud position to the map renderer.
- No expansion logic, economy, balance or save-data logic was changed.

## Step 23B - Terrain clear map preview

- Added selected obstacle clear preview to `MapRenderer.cs`.
- Forest and mountain cells now show a colored preview when selected:
  - green when the player has enough tools to clear;
  - red when tools are missing.
- The preview marker shows the required tool and amount:
  - `A#` for axes required by forests;
  - `M#` for mines required by mountains.
- Updated `Game1.cs` to pass selected terrain position to the map renderer.
- No terrain clear logic, tool generation, economy, balance or save-data logic was changed.

## Step 23C - Expansion and obstacle property clarity

- Improved selected obstacle and cloud property rows in `UiRenderer.cs`.
- Forest and mountain properties now show:
  - clearer `PURPOSE`;
  - actionable `ISSUE`;
  - `CLEAR COST` as available / required tools.
- Cloud properties now show:
  - `STATE` as `READY TO UNLOCK` when possible;
  - actionable `ISSUE`;
  - `UNLOCK COST` as current money/research compared with required money/research.
- No terrain clear logic, cloud unlock logic, economy, balance or save-data logic was changed.

## Step 23D - Area unlock result summary

- Improved cloud unlock success feedback in `UiRenderer.cs`.
- The status bar now summarizes revealed terrain types after a successful area unlock.
- Example:
  - `AREA UNLOCKED 5: FOREST 1, LAND 3, MOUNTAIN 1`
- Uses `AreaUnlockResult.RevealedTiles`; no unlock logic was duplicated or changed.
- No expansion logic, economy, balance or save-data logic was changed.

## Step 23E - Expansion system documentation

- Added `docs/EXPANSION_SYSTEM.md`.
- Updated `docs/FEEDBACK_SYSTEM.md` with Milestone 23 expansion feedback rules.
- Documented:
  - cloud unlock preview;
  - cloud property rows;
  - cloud unlock result summary;
  - forest/mountain clear preview;
  - obstacle property rows;
  - implementation boundaries between `AreaUnlockSystem`, `TerrainClearSystem`, `MapRenderer` and `UiRenderer`.
- No code, expansion logic, economy, balance or save-data logic was changed.

## Step 23F - Building range overlay

- Added selected building range overlay in `MapRenderer.cs`.
- When a built heat converter with `HeatRange > 0` is selected, the map now highlights the covered cells.
- The overlay uses Chebyshev distance, matching the existing heat conversion logic.
- Heat producers covered by the selected converter receive a highlighted outline.
- Updated `Game1.cs` to pass `SelectedMapBuildingId` into `MapRenderer.Draw`.
- No heat conversion logic, economy, balance or save-data logic was changed.

## Step 23G - Heat coverage inverse feedback

- Improved heat coverage map feedback in `MapRenderer.cs`.
- Selecting a heat producer now highlights active heat converters that cover it.
- The previous Step 23F behavior remains unchanged: selecting a converter still shows its range and highlights covered producers.
- Color distinction:
  - aqua outline = producer covered by selected converter;
  - yellow outline = converter covering selected producer.
- Uses Chebyshev distance, matching existing heat conversion logic.
- No heat conversion logic, economy, balance or save-data logic was changed.

## Step 23H - Heat converter placement range preview

- Added build-time range preview in `MapRenderer.cs`.
- When a heat converter building tool is selected and the mouse is over the map, the future `HeatRange` is previewed around the hovered tile.
- The preview appears only for building definitions with `HeatRange > 0` and positive heat conversion.
- Valid placements use the normal range overlay color; invalid placements use a red/attenuated overlay.
- Added a compact `R#` marker on the hovered cell to show the converter range.
- Refactored selected-converter range drawing to reuse `DrawHeatRangeOverlay`.
- No build logic, heat conversion logic, economy, balance or save-data logic was changed.

## Step 23I - Milestone 23 final documentation

- Consolidated Milestone 23 documentation.
- Updated `docs/EXPANSION_SYSTEM.md` with a final milestone state section.
- Updated `docs/FEEDBACK_SYSTEM.md` with a feedback summary for cloud unlocks, obstacles and heat coverage.
- Recorded that Milestone 23 is UI/readability-only and does not change map data, unlock logic, clear logic, heat conversion, economy or save-data format.
- No code, gameplay, balance or data changes were made.

## Step 23J - Useful stability tests

- Added targeted core tests for Milestone 23 map feedback rules.
- Added `HeatSystemTests.GeneratorDiagonalWithinChebyshevRange_ShouldConvertHeatToEnergy`.
- Added `BuildingOperationalStatusCalculatorTests.Calculate_ForHeatProducerWithGeneratorInDiagonalChebyshevRange_ShouldReturnActive`.
- Added `AreaUnlockSystemTests.GetUnlockableCloudTiles_WhenResourcesMissing_ShouldStillReturnPreviewTiles`.
- Added `TerrainClearSystemTests.CanClearMountain_WhenNotEnoughMines_ShouldReturnNotEnoughMines`.
- These tests protect the rules shown by the UI:
  - heat range is Chebyshev;
  - heat producer status matches diagonal converter coverage;
  - cloud preview tiles are still available when money/research is missing;
  - terrain clear validation returns the correct missing-tool reason.
- No gameplay, balance, UI rendering or save-data logic was changed.

## Step 24A - First objective hint

- Started Milestone 24 onboarding.
- Added a lightweight dynamic objective hint in the status bar.
- The objective is derived from current world state and does not require saved mission state.
- Early sequence guides the player through:
  - building the first wind turbine;
  - selling stored energy and building the first office;
  - building the first research center;
  - building the first battery;
  - building solar/heat generation and placing a generator in range.
- The hint is shown only as the default status message when no higher-priority status is active.
- Existing status priorities remain unchanged: action results, save/load messages, selected building status and demolish confirmation still override the objective.
- No economy, simulation, build logic, research logic or save-data format was changed.

## Step 24B - Early game checklist

- Added a compact non-interactive early checklist in `UiRenderer.cs`.
- The checklist is drawn above the status bar in the map area when early onboarding is still incomplete.
- Checklist items are derived from current world state and do not require saved mission state.
- Items:
  - build wind turbine;
  - build small office;
  - build research center;
  - build small battery;
  - build solar + generator with heat coverage.
- The checklist hides automatically when all items are complete.
- Uses simple `OK` / `--` markers for pixel-font compatibility.
- No economy, simulation, build logic, research logic or save-data format was changed.

## Step 24C - Contextual heat and generator hints

- Improved contextual onboarding text for heat-related buildings in `UiRenderer.cs`.
- Build tool properties now populate `ISSUE` with heat-specific guidance.
- Heat producers now hint that a generator should be added after placement.
- Heat converters now hint to place them with a heat source inside range.
- Build tool action text now mentions range preview for heat converters.
- Build menu support text is clearer:
  - heat producers: `PLACE GENERATOR IN RANGE`;
  - heat converters: `PLACE NEAR HEAT SOURCE, RANGE #`.
- Selected active heat converters now report `ABSORBING HEAT IN RANGE`.
- Idle heat converters now report `WAITING FOR HEAT IN RANGE`.
- No build rules, heat conversion logic, economy, balance or save-data format was changed.

## Step 24D - Help panel / quick guide

- Added a non-modal quick guide panel.
- Added `HELP` command button in the top command strip.
- Added `H` keyboard shortcut to toggle the help panel.
- The help panel explains:
  - early energy/money/research flow;
  - batteries;
  - heat producers and generators;
  - terrain clearing;
  - cloud expansion;
  - basic controls.
- The panel is UI-only and uses `_showHelpPanel` runtime state in `Game1`.
- No save-data format, economy, simulation, build, heat, research or expansion logic was changed.

## Step 24E - Help panel current objective/checklist

- Improved the HELP panel in `UiRenderer.cs`.
- Added a dynamic `CURRENT` section to the quick guide.
- The `CURRENT` section shows:
  - the current objective from `GetCurrentObjectiveHint`;
  - the same early checklist used by the map overlay.
- Renamed the static guide section to `BASICS`.
- Increased max help panel height to fit the dynamic guidance while keeping viewport-based bounds.
- No save-data, economy, simulation, build, heat, research or expansion logic was changed.

## Step 24F - Milestone 24 final documentation

- Consolidated Milestone 24 documentation.
- Added final onboarding-state summary to `docs/GAME_DESIGN.md`.
- Added onboarding feedback summary to `docs/FEEDBACK_SYSTEM.md`.
- Recorded that onboarding remains non-blocking and derived from world state.
- Recorded that no persistent mission/tutorial state is introduced.
- No code, gameplay, balance or save-data changes were made.

## Step 25A - Mid-game objective hints

- Started Milestone 25 progression guidance.
- Extended `UiRenderer.GetCurrentObjectiveHint` beyond the early-game checklist.
- Added `GetMidGameObjectiveHint` and helper methods to derive mid-game guidance from current world state.
- Mid-game objectives now guide the player toward:
  - first affordable/relevant upgrade;
  - available research;
  - research accumulation;
  - cloud unlocks;
  - terrain clearing;
  - manager research;
  - next heat power tier;
  - later affordable upgrades.
- Objective state is still derived from world state only.
- No persistent quest state, economy logic, simulation logic, build rules, research rules, expansion rules or save-data format was changed.

## Step 25B - Goal-aware HELP details

- Improved the HELP panel current-progress section.
- Added a `NEXT` line below the current objective.
- `NEXT` explains what the player should do or what is missing for the current objective.
- Detail hints cover:
  - missing money for buildings;
  - missing required research for buildings;
  - first/next upgrade affordability;
  - missing money/research for upgrades;
  - available research;
  - missing research for next research;
  - cloud unlock money/research requirements;
  - terrain clear tool requirements;
  - manager research;
  - next heat tier.
- Added helper methods in `UiRenderer.cs`:
  - `GetCurrentObjectiveDetailHint`;
  - `GetMidGameObjectiveDetailHint`;
  - `GetBuildObjectiveDetail`;
  - `GetUpgradeSavingDetail`;
  - `TryGetNextRelevantUpgradeToSaveFor`.
- No persistent quest state, economy logic, simulation logic, build rules, research rules, expansion rules or save-data format was changed.

## Step 25C - Progression bottleneck feedback

- Added bottleneck feedback to the HELP panel.
- The `CURRENT` section now shows:
  - current objective;
  - `NEXT` practical action/resource gap;
  - `BOT` current bottleneck classification.
- Added `GetCurrentBottleneckHint` in `UiRenderer.cs`.
- Bottleneck detection uses `ResourceRateSnapshot.Calculate(_world)` and current world state.
- Covered bottlenecks:
  - heat producer without generator coverage;
  - negative net energy;
  - no energy production;
  - no money because no office or no sellable energy;
  - no research because no research center or no energy supply;
  - cloud unlock resource gap;
  - terrain clear tool gap;
  - nearly full energy storage;
  - affordable upgrade/research available;
  - stable fallback.
- No economy, simulation, heat, expansion, build, research, upgrade or save-data logic was changed.

## Step 25D - Progression advisor extraction and tests

- Extracted progression guidance from `UiRenderer` into `GridPowerTycoon.Core.Progression.ProgressionAdvisor`.
- `ProgressionAdvisor` now owns:
  - current objective;
  - current objective detail;
  - current bottleneck;
  - heat producer coverage checks used by onboarding UI.
- `UiRenderer` now calls `ProgressionAdvisor` through small wrapper methods and remains focused on rendering.
- Added `ProgressionAdvisorTests` covering:
  - no wind turbine objective;
  - missing money detail for first office;
  - first affordable upgrade objective after early game;
  - heat producer without converter bottleneck;
  - cloud unlock resource gap detail.
- This is a structural/testability pass only.
- No economy, simulation, heat, expansion, build, research, upgrade or save-data behavior was intentionally changed.
