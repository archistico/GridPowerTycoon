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
