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
