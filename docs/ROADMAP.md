# Roadmap

## Step 01 — Solution e base tecnica

Stato: completato.

La solution contiene `GridPowerTycoon.Core`, `GridPowerTycoon.MonoGame` e `GridPowerTycoon.Core.Tests`. La mappa, gli edifici e l'economia sono separati dalla parte grafica.

## Step 02 — BuildSystem Core

Stato: completato.

Il Core permette di costruire edifici sulla mappa tramite `BuildSystem`, con controllo su denaro, celle fuori mappa, celle non edificabili e celle occupate. Le batterie applicano immediatamente l'aumento di capacità energetica.

## Step 03 — Collegamento BuildSystem a MonoGame

Stato: completato in questa modifica.

La schermata MonoGame ora permette di selezionare gli edifici iniziali dal menu laterale o con i tasti `1`, `2`, `3`, `4`. Il click sulla griglia chiama `BuildSystem.Build(...)`. La mappa mostra gli edifici costruiti, evidenzia la cella sotto il mouse e mostra una preview verde/rossa in base alla possibilità di costruire.

## Prossimo step — Produzione, vendita e vita edifici

Il prossimo blocco deve introdurre la simulazione base: produzione energia, produzione ricerca, scadenza edifici, vendita manuale e vendita automatica degli uffici.

## Step 04 — Simulazione base produzione/vendita

Stato: preparato.

Questo step introduce la prima simulazione economica continua: gli edifici attivi producono energia e ricerca, gli edifici con durata residua scadono, il tasto SELL vende tutta l'energia accumulata e gli uffici vendono automaticamente una quota di energia al secondo. I valori continuano a provenire da JSON tramite `BuildingDefinition` ed `EconomySettings`.


## Fix JSON enum converter
- Added JsonStringEnumConverter to GameDataLoader so JSON string enum values such as building category "PowerProducer" deserialize correctly.
- Added regression test for loading building categories from JSON strings.

## Step 06 - Ricerca e sblocco edifici

Stato: implementato.

- `research.json` caricato all'avvio.
- Catalogo e stato ricerca separati nel Core.
- Sistema ricerca con controlli: ricerca sconosciuta, già completata, prerequisiti mancanti, punti ricerca insufficienti.
- `BuildSystem` rispetta `requiredResearchId` degli edifici.
- UI minima per acquistare ricerche e visualizzare edifici bloccati.

## Step 06 UI polish - Barre vita e valori stabili

Stato: implementato.

- Gli edifici con `LifetimeSeconds > 0` mostrano ora una barretta di vita nella parte bassa del riquadro edificio.
- La barra diventa verde/gialla/rossa in base alla durata residua.
- Gli edifici scaduti mostrano la barra vuota e mantengono la diagonale di stato scaduto.
- I valori della TopBar (`ENERGY`, `RESEARCH`, `MONEY`) usano sempre due decimali per evitare oscillazioni visive quando cambiano velocemente.

## Step 06 UI polish - Resource rates in top bar

Added per-second resource rate display below the main top bar counters.
Energy rate is computed as an estimated net change for the next second, taking into account active producers, max storage cap and automatic selling. Research rate is the sum of active research buildings. Money rate is the estimated automatic-sale income per second.

## Step 07 - Calore e generatori

Stato: implementato in questo pacchetto.

Il gioco ora legge `Data/heat.json` e usa `HeatSystem` per accumulare calore sugli edifici produttori, convertirlo in energia tramite generatori entro raggio configurato e far esplodere gli edifici che superano la soglia di sicurezza. La mappa mostra una barra calore nella parte alta degli edifici che generano o accumulano calore; il pannello edificio mostra calore prodotto, conversione, raggio e calore accumulato.

## Step 08 - Tools e pulizia ostacoli naturali

Stato: implementato.

Aggiunta la gestione configurabile di asce e mine tramite `Data/tools.json`. Il gioco genera strumenti nel tempo, mostra i contatori nella barra superiore e permette di selezionare boschi/montagne sulla mappa. Il pannello terreno consente di disboscare un bosco spendendo asce o spianare una montagna spendendo mine; la cella diventa terreno libero e quindi edificabile.


## Step 09A - Bilanciamento strumenti

Stato: implementato.

Rallentata la generazione di asce e mine in `Data/tools.json`: ora le asce crescono a `0.005/s` e le mine a `0.0025/s`. Con i costi attuali, rimuovere un bosco richiede circa 13 minuti e 20 secondi di accumulo, mentre rimuovere una montagna richiede circa 26 minuti e 40 secondi. Aggiunto `docs/BALANCE_NOTES.md` per tracciare le decisioni di bilanciamento.
